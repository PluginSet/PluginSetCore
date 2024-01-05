#if UNITY_IOS
#define UNITY_IOS_API
#endif
using System;
using System.IO;
using System.Linq;
using PluginSet.Platform.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PluginSet.Core.Editor
{
    [BuildTools]
    public static class BuildHelper
    {
        public static void BuildIpaInstallerInternal(string ipa, string output, string remote)
        {
        }

        public static void PreBuildWithContext(BuildProcessorContext context)
        {
            context.TaskType = BuildTaskType.Prebuild;
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
            context.TaskType = BuildTaskType.BuildProject;
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
            context.TaskType = BuildTaskType.BuildBundles;
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
            context.TaskType = BuildTaskType.BuildPatches;
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

        public static void SyncPluginsConfig(bool resolveAndroidLibs = true)
        {
            var context = BuildProcessorContext.Default();
            context.SkipResolveAndroidLibs = !resolveAndroidLibs;
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

        public static void BuildPatchesDefault(string buildPath, PatchFiles patchFiles = null)
        {
            var context = BuildProcessorContext.Default();
            context.BuildPath = buildPath;
            context.PatchFiles = patchFiles;
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
            PlatformTools.BuildIpaInstaller(args["ipa"], args["output"], args["remote"]);
        }
        
        public static void BuildWithExistProject()
        {
            if (!Application.isBatchMode)
                throw new BuildException("Application is not in batch mode");

            var context = BuildProcessorContext.BatchMode();
            if (!Directory.Exists(context.ProjectPath))
                throw new BuildException("There is no exported project");

#if UNITY_ANDROID
            var projectManager = BuildUtils.GetProjectManager(context.ProjectPath);
            Global.CallCustomOrderMethods<AndroidMultipleBuildSetupAttribute, BuildToolsAttribute>(context, projectManager);
#elif UNITY_IOS
            var projectManager = BuildUtils.GetProjectManager(context.ProjectPath);
            Global.CallCustomOrderMethods<iOSMultipleBuildSetupAttribute, BuildToolsAttribute>(context, projectManager);
#elif UNITY_WEBGL
            Global.CallCustomOrderMethods<WebGLMultipleBuildSetupAttribute, BuildToolsAttribute>(context, context.ProjectPath);
#endif
        }

        public static void BuildCompleted()
        {
            if (!Application.isBatchMode)
                throw new BuildException("Application is not in batch mode");
            
            var context = BuildProcessorContext.BatchMode();
            if (!Directory.Exists(context.ProjectPath))
                throw new BuildException("There is no exported project");
            
            Global.CallCustomOrderMethods<BuildCompletedCallbackAttribute, BuildToolsAttribute>(context);
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
