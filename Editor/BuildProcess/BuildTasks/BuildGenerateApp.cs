using System;
using System.IO;
using System.Text;
using UnityEditor;
#if UNITY_IOS
    using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class BuildGenerateApp: IBuildProcessorTask
    {
        public void Execute(BuildProcessorContext context)
        {
            bool isDebug = context.DebugMode;
#if UNITY_2020_1_OR_NEWER
            string productName = "launcher";
#else
            string productName = PlayerSettings.productName;
#endif
            string projectPath = Path.Combine(context.ProjectPath, productName);

            string targetPath = string.Empty;
#if TY_ANDROID
            string command = "gradle";
            string[] args = new string []
            {
                "-p",
                projectPath,
                "\"-Dorg.gradle.jvmargs=-Xmx4096m\"",
                isDebug ? "assembleDebug": "assembleRelease"
            };
            Global.ExecuteCommand(command, args);

            string outPutPath = Path.Combine(projectPath, "build", "outputs", "apk");
            string apkFilePath;
            if (isDebug)
            {
                apkFilePath = Path.Combine(outPutPath, "debug", $"{productName}-debug.apk");
            }
            else
            {
                apkFilePath = Path.Combine(outPutPath, "release", $"{productName}-release.apk");
            }

            targetPath = Path.Combine(context.ProjectPath, "android.apk");
            File.Copy(apkFilePath, targetPath, true);
#elif UNITY_IOS
//            string ipaPath = Path.Combine(context.ExportProjectPath, "build");
//            if (!File.Exists(ipaPath))
//            {
//                Directory.CreateDirectory(ipaPath);
//            }
//            string archivePath = Path.Combine(context.ExportProjectPath, "build", "ios_Archive.xcarchive");
//            var exportOptionsPlist = context.TryGet<string>("iosExportOptionsPlist", null);
//            string cd = "cd " + context.ExportProjectPath;
//            string archive = "xcodebuild archive -jobs 16 -scheme Unity-iPhone -configuration Release -UseModernBuildSystem=YES -archivePath " + archivePath;
//            // string export = "xcodebuild -exportArchive -allowProvisioningUpdates -configuration Release -exportOptionsPlist " + exportOptionsPlist + " -exportPath " + ipaPath + " -archivePath " + archivePath;
//            
//            var asset = context.BuildChannels;
//            var setting = asset.Get<IOSBuildParams>("iOS");
//            
//            // 打出xarchive
//            Global.ExecuteCMDInBash(cd + "&&" + archive);
//            
//            var appPath = Path.Combine(archivePath, "Products/Applications", setting.Product_Name + ".app");
//            var payloadPath = Path.Combine(ipaPath, "Payload");
//            var payloadApp = Path.Combine(payloadPath, setting.Product_Name + ".app");
//
//            string export = $"mkdir -p {payloadPath} && cp -r {appPath} {payloadApp}";
//            Global.ExecuteCMDInBash(export);
//
//            string package = "";
//            // 是否重新签名
//            if (!string.IsNullOrEmpty(setting.Sign))
//            {
//                string sign = $"cd {ipaPath} && codesign --force --sign '{setting.Sign}' --entitlements {Path.Combine(Application.dataPath, "../keyStore/entitle.plist")} Payload/{setting.Product_Name}.app";
//                package = sign;
//            }
//
//            package += " && zip -qr ios.ipa Payload";
//
//            // 把app 拷贝到Payload 打成ipa
//            Global.ExecuteCMDInBash(package);
//            
//            string[] ipaList = Directory.GetFiles(ipaPath, "*.ipa");
//            if (ipaList.Length >= 1) { srcAppPath = ipaList[0]; }
//            File.Copy(srcAppPath, context.AppOutPath, true);
#endif
            Global.CallCustomOrderMethods<OnBuildAppCompletedAttribute, BuildToolsAttribute>(context, targetPath);
        }
    }
}