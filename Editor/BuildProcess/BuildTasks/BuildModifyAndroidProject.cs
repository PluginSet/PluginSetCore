using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildModifyAndroidProject : BuildProcessorTask
    {
        private string AndroidProjectPath;

        public BuildModifyAndroidProject(string path)
        {
            AndroidProjectPath = path;
        }

        public override void Execute(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.Android)
                return;
            
            var metadatas = new Dictionary<string, MetaDataInfo>();
            var methods = Global.GetMethods<AndroidMetadataAttribute, BuildToolsAttribute>();
            foreach (var method in methods)
            {
                var dict = method.Invoke(null, new object[] {context}) as Dictionary<string, string>;
                if (dict == null)
                    continue;

                string typeName = method.Module.Name;
                foreach (var kv in dict)
                {
                    AppendMetaData(metadatas, kv.Key, kv.Value, typeName);
                }
            }
            
            var projectManager = new AndroidProjectManager(AndroidProjectPath);
            XmlDocument doc = projectManager.LauncherManifest;
            foreach (var info in metadatas.Values)
            {
                string value = info.Value;
                if (int.TryParse(value, out _))
                    value = $"\\ {value}";
                doc.SetMetaData(info.Key, value, $"Set in {info.Source}");
            }

            Global.CallCustomOrderMethods<AndroidProjectModifyAttribute, BuildToolsAttribute>(context, projectManager);
            RemoveDebuggable(projectManager.LibraryManifest);
            RemoveDebuggable(projectManager.LauncherManifest);
            projectManager.Save();
        }

        private static void RemoveDebuggable(XmlDocument doc)
        {
            // debuggable
            var element = doc.findFirstElement(AndroidConst.META_DATA_PARENT);
            if (element.HasAttribute("debuggable", AndroidConst.NS_URI))
            {
                element.SetAttribute("debuggable", AndroidConst.NS_URI, "false");
            }
        }

        struct MetaDataInfo
        {
            public string Key;
            public string Value;
            public string Source;
        }

        private static void AppendMetaData(IDictionary<string, MetaDataInfo> dict, string key, string value, string source)
        {
            if (dict.TryGetValue(key, out var info))
            {
                if (info.Value.Equals(value))
                {
                    info.Source = $"{info.Source}|{source}";
                    return;
                }

                throw new BuildException($"The same meta value named {key} from {info.Source} and {source} is different!");
            }

            dict.Add(key, new MetaDataInfo
            {
                Key = key,
                Value = value,
                Source = source
            });
        }
    }
}
