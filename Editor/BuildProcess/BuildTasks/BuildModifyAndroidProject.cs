#if UNITY_ANDROID
#define ENABLE
#endif
#if ENABLE
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
#endif

namespace PluginSet.Core.Editor
{
    public class BuildModifyAndroidProject : IBuildProcessorTask
    {
        private string AndroidProjectPath;

        public BuildModifyAndroidProject(string path)
        {
            AndroidProjectPath = path;
        }

        public void Execute(BuildProcessorContext context)
        {
#if ENABLE
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

            string mainPath = Path.Combine(AndroidProjectPath, "unityLibrary", "src", "main");
            context.Set("mainPath", mainPath);
            
            string manifestPath = Path.Combine(AndroidProjectPath, "launcher", "src", "main");
            XmlDocument doc = new XmlDocument();
            string fileName = Path.Combine(manifestPath, "AndroidManifest.xml");
            doc.Load(fileName);
            foreach (var info in metadatas.Values)
            {
                string value = info.Value;
                if (int.TryParse(value, out _))
                    value = $"\\ {value}";
                doc.SetMetaData(info.Key, value, $"Set in {info.Source}");
            }

            Global.CallCustomMethods<AndroidManifestModifyAttribute, BuildToolsAttribute>(context, doc);
            doc.Save(fileName);

            // 打包必要的混淆
            var proguard = new StringBuilder();
            Global.AppendProguardInLib(proguard, "PluginSet.Core");
            Global.CallCustomMethods<AndroidProguardModifyAttribute, BuildToolsAttribute>(context, proguard);
            var proguardPath = Path.Combine(mainPath, "proguard-user.txt");
            File.WriteAllText(proguardPath, proguard.ToString());
#endif
        }

#if ENABLE
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
#endif
    }
}
