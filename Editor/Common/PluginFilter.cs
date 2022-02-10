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
        public static Func<string, BuildProcessorContext, bool> IsBuildParamsEnable<T>() where T : ScriptableObject
        {
            return delegate(string s, BuildProcessorContext context)
            {
                var @params = context.BuildChannels.Get<T>();
                var fieldInfo = typeof(T).GetField("Enable");
                if (fieldInfo == null)
                    throw new BuildException($"Expect type {typeof(T)} to have field Enable, got null");
                var notEnable = !(bool) fieldInfo.GetValue(@params);
                if (notEnable)
                    Debug.Log($"Filter lib file ::::::: {s}");
                return notEnable;
            };
        }
        
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
