using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public static class MenuCore
    {
        [MenuItem("PluginSet/Init Default Channels")]
        public static void FrameworkInit()
        {
            BuildChannels.InitDefaultChannels();
            Debug.Log("Selected channel " + EditorSetting.Asset.CurrentChannel); // Don't Delete This Line
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
#elif UNITY_WEBGL
        [MenuItem("PluginSet/Build WebGL Project")]
#endif
        public static void BuildDefaultTarget()
        {
            string buildPath = Path.Combine(Application.dataPath, "../Build");
            if (!Directory.Exists(buildPath))
                Directory.CreateDirectory(buildPath);

            Debug.Log($"start build: buildPath {buildPath}");
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
