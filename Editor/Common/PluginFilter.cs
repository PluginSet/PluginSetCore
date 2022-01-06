using System;
using System.Collections.Generic;
using System.IO;
using PluginSet.Core.MiniJSON;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class PluginFilter : IShouldIncludeInBuildCallback
    {
        /// <summary>
        /// 推荐 PluginFilter.RegisterFilterExpress
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="filter"></param>
        public static void RegisterFilter(string packageName, Func<string, BuildProcessorContext, bool> filter)
        {
            BuildUtilities.RegisterShouldIncludeInBuildCallback(new PluginFilter(packageName, filter));
        }

        public static void RegisterFilter(string packageName, Dictionary<string, Func<string, BuildProcessorContext, bool>> filters)
        {
            BuildUtilities.RegisterShouldIncludeInBuildCallback(new PluginFilter(packageName, filters));
        }

        /// <summary>
        /// 打包的时候忽略 package 下面的某个目录
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="excludePath"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="BuildException"></exception>
        public static void RegisterFilterExpress<T>(string packageName, string excludePath) where T : ScriptableObject
        {
            bool FilterFunc(string s, BuildProcessorContext context)
            {
                var @params = context.BuildChannels.Get<T>();
                var fieldInfo = typeof(T).GetField("Enable");
                if (fieldInfo == null)
                    throw new BuildException($"Expect type {typeof(T)} to have field Enable, got null");
                var notEnable = !(bool) fieldInfo.GetValue(@params);
                if (notEnable)
                    Debug.Log($"Filter lib file ::::::: {s}");
                return notEnable;
            }

            var packageDirReplacePath = Global.GetPackageFullPath(packageName);
            var allSubDirs = DirectoryExtension.GetAllDirectoriesAndSubDirectories($"{packageDirReplacePath}/{excludePath}");
            allSubDirs.Add($"{packageName}/{excludePath}");
            foreach (var subDir in allSubDirs)
            {
                var packageDir = subDir.Replace(packageDirReplacePath, packageName);
                BuildUtilities.RegisterShouldIncludeInBuildCallback(new PluginFilter(packageDir, FilterFunc));
            }
        }

        public string PackageName { get; private set; }

        private Func<string, BuildProcessorContext, bool> Filter;

        public bool ShouldIncludeInBuild(string path)
        {
            return !Filter.Invoke(Path.GetFileName(path), BuildProcessorContext.Current);
        }

        private PluginFilter(string packageName, Func<string, BuildProcessorContext, bool> filter)
        {
            PackageName = packageName;
            Filter = filter;
        }

        private PluginFilter(string packageName, Dictionary<string, Func<string, BuildProcessorContext, bool>> filters)
        {
            PackageName = packageName;
            Filter = (fileName, context) =>
            {
                if (filters.TryGetValue(fileName, out var filter))
                {
                    return filter.Invoke(fileName, context);
                }

                return false;
            };
        }
    }
}
