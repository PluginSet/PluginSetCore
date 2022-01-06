using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PluginSet.Core.Editor
{
    [BuildTools]
    public static class BuildHelper
    {
        private static bool HasBuildScene;

        public static EditorBuildSettingsScene[] BuildScenes => EditorBuildSettings.scenes;

        [OnFrameworkInit]
        public static void CopyToolsToProject(BuildProcessorContext context)
        {
            var targetFile = "fabfile.py";
            if (File.Exists(targetFile))
                return;

            var libPath = Global.GetPackageFullPath("com.pluginset.core");
            if (string.IsNullOrEmpty(libPath))
                return;

            File.Copy(Path.Combine(libPath, "Scripts", "fabfile.py"), "fabfile.py");
        }

        [PostProcessScene(-int.MaxValue)]
        public static void OnPostprocessScene()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlaying)
                return;

            if (HasBuildScene) return;
            HasBuildScene = true;

            BuildProcessorContext context = BuildProcessorContext.Current;
            Global.CallCustomOrderMethods<OnCompileCompleteAttribute, BuildToolsAttribute>(context);

            var handler = new BuildTaskHandler();
            // android
//            handler.AddNextTask(new BuildMergeAndroidManifest());

            handler.Execute(context);
        }

        public static void PreBuildWithContext(BuildProcessorContext context)
        {
            var handler = new BuildTaskHandler();
            handler.AddNextTask(new BuildInitialize());
            handler.AddNextTask(new BuildSyncEditorSettings());
            handler.Execute(context);
        }

        public static void BuildAllWithContecxt(BuildProcessorContext context)
        {
            var handler = new BuildTaskHandler();
            handler.AddNextTask(new BuildInitialize());
            handler.AddNextTask(new BuildSyncEditorSettings());
            handler.AddNextTask(new BuildPrepareGradleTemplates());
            handler.AddNextTask(new BuildCheckExportedProject());
            handler.AddNextTask(new BuildEnd());
            handler.Execute(context);
        }

        public static void BuildWithContext(BuildProcessorContext context)
        {
            var handler = new BuildTaskHandler();
            handler.AddNextTask(new BuildInitialize());
//            handler.AddNextTask(new BuildSyncEditorSettings());
            handler.AddNextTask(new BuildPrepareGradleTemplates());
            handler.AddNextTask(new BuildCheckExportedProject());
            handler.AddNextTask(new BuildEnd());
            handler.Execute(context);
        }

        /// <summary>
        /// 不用重新导出工程
        /// </summary>
        /// <param name="context"></param>
        public static void BuildNotExportProject(BuildProcessorContext context)
        {
            var handler = new BuildTaskHandler();
            handler.AddNextTask(new BuildCheckExportedProject());
            handler.Execute(context);
        }

        public static void BuildBundlesWithContext(BuildProcessorContext context)
        {
            var handler = new BuildTaskHandler();
            handler.AddNextTask(new BuildInitialize());
//            handler.AddNextTask(new BuildSyncEditorSettings());
            handler.AddNextTask(new BuildPrepareBundles());
            handler.AddNextTask(new BuildExportAssetBundles());
            handler.AddNextTask(new BuildCopyBundles());
            handler.AddNextTask(new BuildEnd());
            handler.Execute(context);
        }

        public static void BuildPatchesWithContext(BuildProcessorContext context)
        {
            var handler = new BuildTaskHandler();
            handler.AddNextTask(new BuildInitialize());
//            handler.AddNextTask(new BuildSyncEditorSettings());
            handler.AddNextTask(new BuildPreparePatches());
            handler.AddNextTask(new BuildExportAssetBundles());
            handler.AddNextTask(new BuildCopyBundles());
            handler.AddNextTask(new BuildEnd());
            handler.Execute(context);
        }

        public static void InitStudio()
        {
            PreBuildWithContext(BuildProcessorContext.Default());
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

            if (context.ForceExportProject)
            {
                BuildAllWithContecxt(context);
            }
            else
            {
                BuildNotExportProject(context);
            }
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

        private static string AppendSpaces(string text, int targetLen)
        {
            var diff = targetLen - text.Length;
            if (diff > 0)
                return text + new string(' ', diff);

            return text;
        }
    }
}
