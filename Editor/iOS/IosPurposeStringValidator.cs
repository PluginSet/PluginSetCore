using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using PluginSet.Core;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    /// <summary>
    /// iOS 权限用途说明（Info.plist purpose string）导出后校验器（通用机制）。
    ///
    /// 背景：App Store 审核逐条校验用途说明，出现「空字符串」或「占位/模板文案」会按
    /// Guidelines 5.1.1 直接拒审；引用了系统 API 却缺少对应键则可能被上传校验拦下（ITMS-90683）。
    ///
    /// 设计边界：
    ///   · 机制在插件：本类负责校验、报错、落盘结果文件；
    ///   · 政策在项目：哪些键“必须存在/禁止出现”由 IosBuildParams 的
    ///     RequiredUsageDescriptions / ForbiddenUsageDescriptions 声明（留空则只做通用检查）。
    ///
    /// 结果文件：&lt;导出工程&gt;/iosPurposeStringGuard.json（status=pass|fail），
    /// 供 CI 判定构建失败 —— Debug.LogError 本身不会改变进程退出码。
    /// </summary>
    public static class IosPurposeStringValidator
    {
        private const string PlistFileName = "Info.plist";
        private const string ResultFileName = "iosPurposeStringGuard.json";

        private static readonly string[] PlaceholderMarkers =
        {
            "your consent is required",
            "xyz benefits",
            "requires access to the photo library",
            "app would like to access",
        };

        private static readonly char[] ListSeparators = { '\n', '\r', ',', ';', ' ', '\t' };

        [PostProcessBuild(int.MaxValue - 1)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS)
                return;

            var plistPath = Path.Combine(buildPath, PlistFileName);
            if (!File.Exists(plistPath))
            {
                Debug.LogWarning("[IosPurposeStringValidator] 未找到 " + plistPath + "，跳过用途说明校验");
                return;
            }

            var parameters = ReadParams();
            var required = SplitKeys(parameters == null ? null : parameters.RequiredUsageDescriptions);
            var forbidden = SplitKeys(parameters == null ? null : parameters.ForbiddenUsageDescriptions);
            var policySource = parameters == null ? "未取到 IosBuildParams（仅做通用检查）" : "IosBuildParams";

            var problems = new List<string>();
            try
            {
                CollectProblems(plistPath, required, forbidden, problems);
            }
            catch (XmlException e)
            {
                Debug.LogError("[IosPurposeStringValidator] 解析 " + plistPath + " 失败：" + e.Message);
                return;
            }

            WriteResult(buildPath, policySource, required, forbidden, problems);

            if (problems.Count == 0)
            {
                Debug.Log("[IosPurposeStringValidator] 用途说明校验通过（必需 " + required.Count
                          + " 个 / 禁止 " + forbidden.Count + " 个）：" + policySource);
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine("[IosPurposeStringValidator] ❌ 用途说明校验未通过，提交前必须处理：");
            foreach (var problem in problems)
                builder.AppendLine("  · " + problem);
            builder.AppendLine("  提示：不要在用途说明里使用占位文案（如 \"Your consent is required to access xxx\"）；");
            builder.AppendLine("        留空的用途说明不会写入 Info.plist，未申请权限的键应保持留空。");
            Debug.LogError(builder.ToString());
        }

        /// <summary>安全读取渠道参数：类型/条目异常时返回 null，绝不让构建崩在读取上</summary>
        private static IosBuildParams ReadParams()
        {
            try
            {
                var context = BuildProcessorContext.Current;
                if (context == null || context.BuildChannels == null)
                    return null;

                return context.BuildChannels.TryGet<IosBuildParams>(null);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[IosPurposeStringValidator] 读取 IosBuildParams 失败，仅做通用检查："
                                 + e.GetType().Name + " - " + e.Message);
                return null;
            }
        }

        private static List<string> SplitKeys(string value)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(value))
                return result;

            foreach (var item in value.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                var key = item.Trim();
                if (key.Length > 0 && !result.Contains(key))
                    result.Add(key);
            }

            return result;
        }

        private static void CollectProblems(string plistPath, List<string> required, List<string> forbidden,
            List<string> problems)
        {
            var document = new XmlDocument();
            document.Load(plistPath);

            var root = document.DocumentElement;
            var dict = root == null ? null : FindFirstChild(root, "dict");
            if (dict == null)
            {
                problems.Add("Info.plist 结构异常：找不到顶层 dict");
                return;
            }

            var found = new Dictionary<string, string>();
            var children = dict.ChildNodes;
            for (var i = 0; i + 1 < children.Count; i++)
            {
                var keyNode = children[i];
                if (keyNode.Name != "key")
                    continue;

                var key = keyNode.InnerText == null ? null : keyNode.InnerText.Trim();
                if (string.IsNullOrEmpty(key) || !key.EndsWith("UsageDescription"))
                    continue;

                var valueNode = children[i + 1];
                found[key] = valueNode != null && valueNode.Name == "string" ? valueNode.InnerText : null;
            }

            // 通用检查 1：空值（空用途说明同样判 insufficient）
            foreach (var pair in found)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                    problems.Add(pair.Key + " 为空字符串，审核会判定 insufficient");
            }

            // 通用检查 2：占位/模板文案
            foreach (var pair in found)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                    continue;

                var value = pair.Value.ToLowerInvariant();
                foreach (var marker in PlaceholderMarkers)
                {
                    if (value.Contains(marker))
                    {
                        problems.Add(pair.Key + " 使用了占位/模板文案：\"" + pair.Value + "\"");
                        break;
                    }
                }
            }

            // 项目政策：必须存在且非空
            foreach (var key in required)
            {
                if (!found.ContainsKey(key))
                    problems.Add("缺少必需的用途说明 " + key);
                else if (string.IsNullOrWhiteSpace(found[key]))
                    problems.Add("必需的用途说明 " + key + " 为空");
            }

            // 项目政策：禁止出现
            foreach (var key in forbidden)
            {
                if (!found.ContainsKey(key))
                    continue;

                problems.Add(key + " 不应存在（按当前项目配置不申请该权限），当前值：\"" + found[key] + "\"");
            }
        }

        private static void WriteResult(string buildPath, string policySource, List<string> required,
            List<string> forbidden, List<string> problems)
        {
            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine("  \"status\": \"" + (problems.Count == 0 ? "pass" : "fail") + "\",");
            json.AppendLine("  \"errorCount\": " + problems.Count + ",");
            json.AppendLine("  \"policySource\": " + Quote(policySource) + ",");
            json.AppendLine("  \"errors\": " + JsonArray(problems) + ",");
            json.AppendLine("  \"requiredKeys\": " + JsonArray(required) + ",");
            json.AppendLine("  \"forbiddenKeys\": " + JsonArray(forbidden));
            json.AppendLine("}");

            var path = Path.Combine(buildPath, ResultFileName);
            try
            {
                File.WriteAllText(path, json.ToString(), new UTF8Encoding(false));
                Debug.Log("[IosPurposeStringValidator] 校验结果已写入 " + path);
            }
            catch (IOException e)
            {
                Debug.LogWarning("[IosPurposeStringValidator] 写入校验结果失败：" + e.Message);
            }
        }

        private static string JsonArray(ICollection<string> items)
        {
            var builder = new StringBuilder("[");
            var first = true;
            foreach (var item in items)
            {
                if (!first)
                    builder.Append(", ");
                builder.Append(Quote(item));
                first = false;
            }

            builder.Append("]");
            return builder.ToString();
        }

        private static string Quote(string value)
        {
            if (value == null)
                return "\"\"";

            var builder = new StringBuilder("\"");
            foreach (var c in value)
            {
                switch (c)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (c < ' ')
                            builder.Append("\\u").Append(((int)c).ToString("x4"));
                        else
                            builder.Append(c);
                        break;
                }
            }

            builder.Append("\"");
            return builder.ToString();
        }

        private static XmlNode FindFirstChild(XmlNode parent, string name)
        {
            foreach (XmlNode child in parent.ChildNodes)
            {
                if (child.Name == name)
                    return child;
            }

            return null;
        }
    }
}
