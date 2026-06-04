using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            File.Copy(Path.Combine(libPath, "Scripts", "requirements.txt"), "requirements.txt", true);
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
                BuildHelper.BuildPatchesDefault(Application.streamingAssetsPath, new PatchFiles
                {
                    ModFiles = Directory.GetFiles(Path.Combine(Application.dataPath, "Resources"), "*.*")
                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            EditorUtility.ClearProgressBar();
        }

        [MenuItem("PluginSet/Find Missing Scripts")]
        public static void FindMissingScripts()
        {
            var assetPaths = CollectScannableAssetPaths();
            var scenePaths = assetPaths.Where(p => p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)).ToList();
            var otherPaths = assetPaths.Where(p => !p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)).ToList();

            int issueCount = 0;
            int processed = 0;
            int total = assetPaths.Count;

            try
            {
                for (int i = 0; i < otherPaths.Count; i++)
                {
                    string path = otherPaths[i];
                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Find Missing Scripts",
                            path,
                            total > 0 ? (float)processed / total : 1f))
                        break;

                    ScanGameObjectAsset(path, ref issueCount);
                    processed++;
                }

                if (scenePaths.Count > 0)
                {
                    var previousSetup = EditorSceneManager.GetSceneManagerSetup();
                    try
                    {
                        for (int i = 0; i < scenePaths.Count; i++)
                        {
                            string path = scenePaths[i];
                            if (EditorUtility.DisplayCancelableProgressBar(
                                    "Find Missing Scripts",
                                    path,
                                    total > 0 ? (float)processed / total : 1f))
                                break;

                            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                            try
                            {
                                foreach (var root in scene.GetRootGameObjects())
                                    ScanGameObjectHierarchy(root, path, ref issueCount);
                            }
                            finally
                            {
                                EditorSceneManager.CloseScene(scene, true);
                            }

                            processed++;
                        }
                    }
                    finally
                    {
                        EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (issueCount == 0)
                Debug.Log("[Find Missing Scripts] 未发现 Missing Script。");
            else
                Debug.LogWarning($"[Find Missing Scripts] 共发现 {issueCount} 处 Missing Script，点击上方警告可定位资源。");
        }

        private static List<string> CollectScannableAssetPaths()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab t:Scene", new[] { "Assets" }))
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));

            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (!path.StartsWith("Assets/", StringComparison.Ordinal))
                    continue;

                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (type == typeof(GameObject) && !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    paths.Add(path);
            }

            return paths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void ScanGameObjectAsset(string assetPath, ref int issueCount)
        {
            if (assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (root != null)
                    ScanGameObjectHierarchy(root, assetPath, ref issueCount);
                return;
            }

            foreach (var go in AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<GameObject>())
            {
                if (go.transform.parent != null)
                    continue;
                ScanGameObjectHierarchy(go, assetPath, ref issueCount);
            }
        }

        private static void ScanGameObjectHierarchy(GameObject root, string assetPath, ref int issueCount)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var go = transform.gameObject;
                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] != null)
                        continue;

                    issueCount++;
                    var pingTarget = AssetDatabase.LoadMainAssetAtPath(assetPath);
                    Debug.LogWarning(
                        $"[Missing Script] 节点: {GetTransformPath(go)} | 资源: {assetPath}",
                        pingTarget != null ? pingTarget : go);
                }
            }
        }

        private static string GetTransformPath(GameObject go)
        {
            var names = new List<string>();
            var current = go.transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names);
        }
    }
}
