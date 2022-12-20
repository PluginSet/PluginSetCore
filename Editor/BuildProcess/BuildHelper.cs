using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.iOS;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PluginSet.Core.Editor
{
    [BuildTools]
    public static class BuildHelper
    {
        public static void BuildIpaInstallerInternal(string ipa, string output, string remote)
        {
            var libPath = Global.GetPackageFullPath("com.pluginset.core");
            var templatePath = Path.Combine(libPath, "IosTools~");

            var ipaName = string.Empty;
            if (!string.IsNullOrEmpty(ipa))
                ipaName = Path.GetFileName(ipa);

            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            var index = File.ReadAllText(Path.Combine(templatePath, "index.html"));
            var manifest = File.ReadAllText(Path.Combine(templatePath, "manifest.plist"));

            index = index.Replace("{{DISPLAY_NAME}}", PlayerSettings.productName);
            manifest = manifest.Replace("{{DISPLAY_NAME}}", PlayerSettings.productName);
            
            index = index.Replace("{{BUNDLE_ID}}", PlayerSettings.applicationIdentifier);
            manifest = manifest.Replace("{{BUNDLE_ID}}", PlayerSettings.applicationIdentifier);
            
            index = index.Replace("{{VERSION_NAME}}", PlayerSettings.bundleVersion);
            manifest = manifest.Replace("{{VERSION_NAME}}", PlayerSettings.bundleVersion);
            
            index = index.Replace("{{VERSION_CODE}}", PlayerSettings.iOS.buildNumber);
            manifest = manifest.Replace("{{VERSION_CODE}}", PlayerSettings.iOS.buildNumber);
            
            index = index.Replace("{{ICON_URL}}", $"{remote}/app.png");
            manifest = manifest.Replace("{{ICON_URL}}", $"{remote}/app.png");
            
            index = index.Replace("{{MANIFEST_URL}}", $"{remote}/manifest.plist");
            manifest = manifest.Replace("{{MANIFEST_URL}}", $"{remote}/manifest.plist");
            
            index = index.Replace("{{IPA_URL}}", $"{remote}/{ipaName}");
            manifest = manifest.Replace("{{IPA_URL}}", $"{remote}/{ipaName}");
            
            File.WriteAllText(Path.Combine(output, "index.html"), index);
            File.WriteAllText(Path.Combine(output, "manifest.plist"), manifest);
            if (!string.IsNullOrEmpty(ipa))
                File.Copy(ipa, Path.Combine(output, ipaName), true);

            var appIconPath = Path.Combine(templatePath, "app.png");
            
            var icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.iOS, iOSPlatformIconKind.Application);
            if (icons != null && icons.Length > 0)
            {
                var icon = icons[0].GetTexture();
                if (icon != null)
                {
                    var path = AssetDatabase.GetAssetPath(icon);
                    if (!string.IsNullOrEmpty(path))
                        appIconPath = path;
                }
            }
            File.Copy(appIconPath, Path.Combine(output, "app.png"), true);
        }

        public static void PreBuildWithContext(BuildProcessorContext context)
        {
            try
            {
                var handler = new BuildTaskHandler();
                handler.AddNextTask(new BuildSyncEditorSettings());
                handler.AddNextTask(new BuildEnd());
                handler.Execute(context);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
            }
        }

        public static void BuildWithContext(BuildProcessorContext context)
        {
            try
            {
                context.ExportProject = true;
                var handler = new BuildTaskHandler();
                handler.AddNextTask(new BuildSyncEditorSettings());
                handler.AddNextTask(new BuildCheckExportedProject());
                handler.AddNextTask(new BuildEnd());
                handler.Execute(context);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
            }
        }

        public static void BuildBundlesWithContext(BuildProcessorContext context)
        {
            try
            {
                var handler = new BuildTaskHandler();
                handler.AddNextTask(new BuildPrepareBundles());
                handler.AddNextTask(new BuildExportAssetBundles());
                handler.AddNextTask(new BuildEnd());
                handler.Execute(context);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
            }
        }

        public static void BuildPatchesWithContext(BuildProcessorContext context)
        {
            try
            {
                var handler = new BuildTaskHandler();
                handler.AddNextTask(new BuildPreparePatches());
                handler.AddNextTask(new BuildExportAssetBundles());
                handler.AddNextTask(new BuildCopyBundles());
                handler.AddNextTask(new BuildEnd());
                handler.Execute(context);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
            }
        }

        public static void SyncPluginsConfig()
        {
            var context = BuildProcessorContext.Default();
            PreBuildWithContext(context);
        }

        public static void BuildAppDefault(string buildPath)
        {
#if UNITY_ANDROID
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
#endif
            var context = BuildProcessorContext.Default();
#if UNITY_ANDROID
            context.BuildTarget = BuildTarget.Android;
#elif  UNITY_IOS
            context.BuildTarget = BuildTarget.iOS;
#endif
            context.BuildPath = buildPath;
            BuildWithContext(context);
        }

        public static void BuildBundlesDefault(string buildPath, bool editorTest = false)
        {
            var context = BuildProcessorContext.Default();
            context.BuildPath = buildPath;
            context.ForEditor = editorTest;
            BuildBundlesWithContext(context);
        }

        public static void BuildPatchesDefault(string buildPath)
        {
            var context = BuildProcessorContext.Default();
            context.BuildPath = buildPath;
            var path1 = Path.Combine(Application.dataPath, "Lua/UI/UIValid.lua");
            context.PatchFiles = JsonUtility.FromJson<PatchFiles>($"{{\"AddFiles\": [], \"ModFiles\": [\"{path1}\"]}}");
            BuildPatchesWithContext(context);
        }

        public static void PreBuild()
        {
            if (!Application.isBatchMode)
                throw new BuildException("Application is not in batch mode");

            PreBuildWithContext(BuildProcessorContext.BatchMode());
        }

        public static void Build()
        {
            if (!Application.isBatchMode)
                throw new BuildException("Application is not in batch mode");

            BuildWithContext(BuildProcessorContext.BatchMode());
        }

        public static void BuildPatch()
        {
            if (!Application.isBatchMode)
                throw new BuildException("Application is not in batch mode");

            BuildPatchesWithContext(BuildProcessorContext.BatchMode());
        }

        public static void BuildIpaInstaller()
        {
            if (!Application.isBatchMode)
                throw new BuildException("Application is not in batch mode");

            var context = BuildProcessorContext.BatchMode();
            var args = context.CommandArgs;
            Debug.Log("BuildIpaInstaller args::: " + string.Join(";", args.Select(kv => $"{kv.Key}={kv.Value}")));
            BuildIpaInstallerInternal(args["ipa"], args["output"], args["remote"]);
        }
        
        public static void BuildWithExistProject()
        {
            if (!Application.isBatchMode)
                throw new BuildException("Application is not in batch mode");

            var context = BuildProcessorContext.BatchMode();
            if (!Directory.Exists(context.ProjectPath))
                throw new BuildException("There is no exported project");

#if UNITY_ANDROID
            var projectManager = new AndroidProjectManager(context.ProjectPath);
            Global.CallCustomOrderMethods<AndroidMultipleBuildSetupAttribute, BuildToolsAttribute>(context, projectManager);
#elif UNITY_IOS
            string xcodeProjectPath = Path.Combine(context.ProjectPath, "Unity-iPhone.xcodeproj", "project.pbxproj");
            var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
            var bundleStrings = bundleId.Split('.');
            var project = new PBXProjectManager(xcodeProjectPath
                , $"Unity-iPhone/{bundleStrings[bundleStrings.Length - 1]}.entitlements"
                , "Unity-iPhone");

            Global.CallCustomOrderMethods<iOSMultipleBuildSetupAttribute, BuildToolsAttribute>(context, project);
#endif
        }

        private static string AppendSpaces(string text, int targetLen)
        {
            var diff = targetLen - text.Length;
            if (diff > 0)
                return text + new string(' ', diff);

            return text;
        }
    }
}
