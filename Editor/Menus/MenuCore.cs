using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public static class MenuCore
    {
        [MenuItem("PluginSet/Init Plugins")]
        public static void FrameworkInit()
        {
            Global.CallCustomOrderMethods<OnFrameworkInitAttribute>(BuildProcessorContext.Default());
        }

        [MenuItem("PluginSet/Copy fabric file")]
        public static void CopyFabricFile()
        {
            var libPath = Global.GetPackageFullPath("com.pluginset.core");
            if (string.IsNullOrEmpty(libPath))
                return;
            
            File.Copy(Path.Combine(libPath, "Scripts", "fabfile.py"), "fabfile.py", true);
        }

#if UNITY_ANDROID
        [MenuItem("PluginSet/Build Android APK")]
#elif UNITY_IOS
        [MenuItem("PluginSet/Build iOS Project")]
#endif
        public static void BuildDefaultTarget()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            string buildPath = null;
            if (target == BuildTarget.Android)
            {
                if (Directory.Exists(Path.Combine(Application.dataPath, "../Build")))
                {
                    buildPath = Path.Combine(Application.dataPath, "../Build");
                }
                else
                {
                    buildPath = Path.Combine(Application.dataPath, "../Build");
                    Directory.CreateDirectory(buildPath);
                }
            }
            else if (target == BuildTarget.iOS)
            {
                if (Directory.Exists(Path.Combine(Application.dataPath, "../Build")))
                {
                    buildPath = Path.Combine(Application.dataPath, "../Build");
                    var tempPath = Path.Combine(buildPath, "appstore");
                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath,true);
                    var md5Path = Path.Combine(buildPath, "channelMd5.txt");
                    if (File.Exists(md5Path))
                        File.Delete(md5Path);
                }
                else
                {
                    buildPath = Path.Combine(Application.dataPath, "../Build");
                    Directory.CreateDirectory(buildPath);
                }
            }

            Debug.Log($"start build: buildPath {buildPath}");
            if (string.IsNullOrEmpty(buildPath))
                return;

            BuildHelper.BuildAppDefault(buildPath);
        }

        [MenuItem("PluginSet/Build Bundles")]
        public static void BuildAllBundles()
        {
            EditorUtility.DisplayProgressBar("Build Bundles", "building...", 0);
            try
            {
                BuildHelper.BuildBundlesDefault(Application.streamingAssetsPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            EditorUtility.ClearProgressBar();
        }


        [MenuItem("PluginSet/Build Bundles (For Editor)")]
        public static void BuildAllBundlesForEditorTest()
        {
            EditorUtility.DisplayProgressBar("Build Bundles", "building...", 0);
            try
            {
                BuildHelper.BuildBundlesDefault(Application.streamingAssetsPath, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            EditorUtility.ClearProgressBar();
        }

        [MenuItem("PluginSet/Build Patches")]
        public static void PatchAllResources()
        {
            EditorUtility.DisplayProgressBar("Build Patches", "patching...", 0);
            try
            {
                BuildHelper.BuildPatchesDefault(Application.streamingAssetsPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
