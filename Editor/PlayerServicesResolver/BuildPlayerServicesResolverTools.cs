using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Tool.PlayerServicesResolver.Editor
{
    [BuildTools]
    public static class BuildPlayerServicesResolverTools
    {
        private static string[] POD_SEARCH_PATHS = new string[] {
            "/usr/local/bin",
            "/usr/bin",
        };
        
        [OnSyncEditorSetting(int.MinValue)]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
            var dependenciesPath = Path.Combine(Application.dataPath, "PluginDependencies", "Editor");
            Global.CheckAndDeletePath(dependenciesPath);
            context.Set("pluginDependenciesPath", dependenciesPath);
            Directory.CreateDirectory(dependenciesPath);
            Global.CopyDependenciesInLib("com.pluginset.core");
        }
        
        [OnSyncEditorSetting(int.MaxValue)]
        public static void OnSyncEditorSetting_after(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.Android)
                return;
            
            AssetDatabase.Refresh();
            if (GooglePlayServices.PlayServicesResolver.ResolveSync(true))
            {
                AssetDatabase.Refresh();
            }
            else
            {
                throw new BuildException("PlayServicesResolver ResolveSync failed");
            }
        }

        [iOSXCodeProjectModify(int.MaxValue)]
        public static void OnIOSXCodeProjectModifyCompleted(BuildProcessorContext context, PBXProjectManager projectManager)
        {
//            RemoveUnityIphoneTargetInPodFile(projectManager.ProjectPath);
        }

        private static void RemoveUnityIphoneTargetInPodFile(string projectPath)
        {
            var podFilePath = Path.Combine(projectPath, "Podfile");
            if (!File.Exists(podFilePath))
                return;

            var lines = new List<string>(File.ReadAllLines(podFilePath, Encoding.UTF8));

            int startIndex = -1;
            int endedIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (startIndex >= 0)
                {
                    if (Regex.IsMatch(line, @"^\s*end\s*$"))
                    {
                        endedIndex = i;
                        break;
                    } else if (Regex.IsMatch(line, @"\s*"))
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (Regex.IsMatch(line, "^\\s*target\\s*[\'\"]?Unity-iPhone[\"\']?\\s*do\\s*$"))
                {
                    startIndex = i;
                }
            }

            if (startIndex >= 0 && endedIndex >= 0)
            {
                lines.RemoveRange(startIndex, endedIndex - startIndex + 1);
                File.WriteAllLines(podFilePath, lines);
            }
            else if (startIndex >= 0)
            {
                Debug.LogWarning("Unity-iPhone target pods is not empty, please check the plugins");
            }
        }
            
    }
}