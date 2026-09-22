#if UNITY_IOS
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    /// <summary>
    /// iOS 27 启动崩溃修复 —— 构建期注入 UIScene 生命周期声明。
    ///
    /// 【为什么需要它】
    ///   iOS 27 起系统强制要求 App 采用 UIKit 的 scene-based 生命周期。若 Info.plist 中没有
    ///   UIApplicationSceneManifest，系统不会为 App 建立 UIWindowSceneSession，于是
    ///   UIApplication.connectedScenes 为空，Unity 2021.3 trampoline 的建窗代码
    ///       _window = [[UIWindow alloc] initWithWindowScene:
    ///                    [self pickStartupWindowScene: application.connectedScenes]];
    ///   会拿到 nil 并传入 initWithWindowScene:。该 API 在 iOS 26 及更早是静默降级，
    ///   iOS 27 起改为抛异常 —— 表现为「点开图标立刻闪退」，且只在 iOS 27 复现。
    ///
    ///   运行时侧的配合在 Plugins/iOS/PluginSetAppController.mm（UnityScene 兼容层）。
    ///   本类只负责把 manifest 写进导出工程，因此**不需要改动 Unity 安装目录**，
    ///   换打包机 / 重装 Unity / 升级 2021.3.x 都不会失效。
    ///
    /// 【为什么用 XmlDocument 而不是 UnityEditor.iOS.Xcode.PlistDocument】
    ///   与项目里 IosAppHardening 的做法保持一致：Unity 导出的 Info.plist 是文本 plist，
    ///   用 XmlDocument 直接改可保留原有 DOCTYPE，也避免受 Unity iOS 模块 API 变动影响。
    ///   写入必须发生在 plist 生成之后，故用 [PostProcessBuild]。
    ///
    /// 【幂等】重复导出的工程不会重复写入；已有 UIApplicationSceneManifest（例如未来
    ///   升级到自带 UIScene 支持的 Unity 6）时只做日志提示，不覆盖引擎/其他人的设置。
    /// </summary>
    public static class IosUISceneCompat
    {
        private const string PlistFileName = "Info.plist";
        private const string ManifestKey = "UIApplicationSceneManifest";
        private const string SceneConfigName = "Default Configuration";

        /// <summary>
        /// scene delegate 类名。
        /// 必须与 Plugins/iOS/PluginSetAppController.mm 里注册的 AppController 类同名 ——
        /// 该类用 IMPL_APP_CONTROLLER_SUBCLASS(PluginSetAppController) 注册，并通过
        /// Category 实现了 UIWindowSceneDelegate，因此这里直接写这个类名即可。
        /// </summary>
        private const string SceneDelegateClassName = "PluginSetAppController";

        [PostProcessBuild(int.MaxValue - 2)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS)
                return;

            var plistPath = Path.Combine(buildPath, PlistFileName);
            if (!File.Exists(plistPath))
            {
                Debug.LogWarning($"[IosUISceneCompat] 未找到 {plistPath}，跳过 UIScene 声明注入");
                return;
            }

            var document = new XmlDocument();
            try
            {
                document.Load(plistPath);
            }
            catch (XmlException e)
            {
                Debug.LogError($"[IosUISceneCompat] 解析 {plistPath} 失败：{e.Message}。" +
                               "iOS 27 上会因缺少 UIScene 声明而启动崩溃，请人工处理。");
                return;
            }

            var rootDict = FindRootDict(document);
            if (rootDict == null)
            {
                Debug.LogError("[IosUISceneCompat] Info.plist 结构异常：找不到顶层 dict");
                return;
            }

            var existing = FindValueNode(rootDict, ManifestKey);
            if (existing != null)
            {
                Debug.Log($"[IosUISceneCompat] Info.plist 已存在 {ManifestKey}（值类型={existing.Name}），保持不变。");
                return;
            }

            WriteManifest(document, rootDict);
            SavePlist(plistPath, document);

            Debug.Log($"[IosUISceneCompat] 已写入 {ManifestKey}（scene delegate = {SceneDelegateClassName}），" +
                      "用于修复 iOS 27 启动崩溃。");
        }

        /// <summary>
        /// 写入 UIApplicationSceneManifest：
        ///   UISceneConfigurations.UIWindowSceneSessionRoleApplication[0] = { 名字, delegate 类名 }
        /// 只声明 Application role —— 本 App 不使用多窗口，也不涉及 external display。
        /// </summary>
        private static void WriteManifest(XmlDocument document, XmlNode rootDict)
        {
            rootDict.AppendChild(document.CreateElement("key")).InnerText = ManifestKey;

            var manifest = document.CreateElement("dict");
            rootDict.AppendChild(manifest);

            // UIApplicationSupportsMultipleScenes = false：游戏为单窗口，声明 false 更贴合实际
            manifest.AppendChild(document.CreateElement("key")).InnerText = "UIApplicationSupportsMultipleScenes";
            manifest.AppendChild(document.CreateElement("false"));

            manifest.AppendChild(document.CreateElement("key")).InnerText = "UISceneConfigurations";
            var configurations = document.CreateElement("dict");
            manifest.AppendChild(configurations);

            configurations.AppendChild(document.CreateElement("key")).InnerText = "UIWindowSceneSessionRoleApplication";
            var roleArray = document.CreateElement("array");
            configurations.AppendChild(roleArray);

            var entry = document.CreateElement("dict");
            roleArray.AppendChild(entry);

            entry.AppendChild(document.CreateElement("key")).InnerText = "UISceneConfigurationName";
            entry.AppendChild(document.CreateElement("string")).InnerText = SceneConfigName;

            entry.AppendChild(document.CreateElement("key")).InnerText = "UISceneDelegateClassName";
            entry.AppendChild(document.CreateElement("string")).InnerText = SceneDelegateClassName;
        }

        private static XmlNode FindRootDict(XmlDocument document)
        {
            var root = document.DocumentElement;
            if (root == null)
                return null;

            foreach (XmlNode child in root.ChildNodes)
            {
                if (child.Name == "dict")
                    return child;
            }

            return null;
        }

        private static XmlNode FindValueNode(XmlNode dict, string key)
        {
            var children = dict.ChildNodes;
            for (var i = 0; i + 1 < children.Count; i++)
            {
                var node = children[i];
                if (node.Name == "key" && node.InnerText != null && node.InnerText.Trim() == key)
                    return children[i + 1];
            }

            return null;
        }

        private static void SavePlist(string plistPath, XmlDocument document)
        {
            // XmlDocument 重写会把 plist 的 DOCTYPE 外部子集改写成非法形式，先备份再还原。
            // （与 IosAppHardening.SavePlist 同源处理，两处都改的话请一起改。）
            var originalDocType = ReadDocType(plistPath);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new System.Text.UTF8Encoding(false),
                OmitXmlDeclaration = false,
            };
            using (var writer = XmlWriter.Create(plistPath, settings))
            {
                document.Save(writer);
            }

            if (string.IsNullOrEmpty(originalDocType))
                return;

            var lines = File.ReadAllLines(plistPath);
            var builder = new System.Text.StringBuilder();
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("<!DOCTYPE"))
                {
                    builder.AppendLine(originalDocType);
                    continue;
                }

                builder.AppendLine(line);
            }

            File.WriteAllText(plistPath, builder.ToString(), new System.Text.UTF8Encoding(false));
        }

        private static string ReadDocType(string plistPath)
        {
            foreach (var line in File.ReadAllLines(plistPath))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("<!DOCTYPE"))
                    return trimmed;
            }

            return null;
        }
    }
}
#endif
