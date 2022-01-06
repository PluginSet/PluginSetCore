using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class BuildPrepareGradleTemplates : IBuildProcessorTask
    {
        private readonly Dictionary<string, string> _propMap = new Dictionary<string, string>()
        {
            {"org.gradle.jvmargs", "JVM_HEAP_SIZE"},
            {"android.enableR8", "MINIFY_WITH_R_EIGHT"}
        };

        private static string GetTemplatePath(string fileName, string templatePath1, string templatePath2)
        {
            var path1 = Path.Combine(templatePath1, fileName);
            if (File.Exists(path1))
                return path1;

            return Path.Combine(templatePath2, fileName);
        }

        public void Execute(BuildProcessorContext context)
        {
#if UNITY_ANDROID
            string pluginPath = Path.Combine(Application.dataPath, "Plugins");
            string androidPluginPath = Path.Combine(pluginPath, "Android");
            if (!Directory.Exists(androidPluginPath))
                Directory.CreateDirectory(androidPluginPath);

            string projectTempPath = Path.Combine(Application.dataPath, "Plugins", "Android", "Templates");

            string templatesPath = Path.Combine(EditorApplication.applicationPath, ".."
#if UNITY_EDITOR_WIN
                , "Data"
#endif
                , "PlaybackEngines", "AndroidPlayer", "Tools", "GradleTemplates");

            string gradleTemp = Path.Combine(androidPluginPath, "gradleTemplate.properties");
            Global.CheckAndRemoveFile(gradleTemp);
            var gradleProperties = GetAllGradleProperties();
            if (gradleProperties.Count > 0)
            {
                var propTempFile = GetTemplatePath("gradleTemplate.properties", projectTempPath, templatesPath);
                if (File.Exists(propTempFile))
                {
                    var text = File.ReadAllText(propTempFile);
                    foreach (var map in _propMap)
                    {
                        if (gradleProperties.ContainsKey(map.Key))
                        {
                            text = text.Replace($"**{map.Value}**", gradleProperties[map.Key]);
                            gradleProperties.Remove(map.Key);
                        }
                    }

                    var addition = string.Join("\n", gradleProperties.Select(kv => $"{kv.Key}={kv.Value}"));
                    text = text.Replace("**ADDITIONAL_PROPERTIES**", $"{addition}\n**ADDITIONAL_PROPERTIES**");
                    File.WriteAllText(gradleTemp, text);
                }
                else
                {
                    File.WriteAllLines(gradleTemp, gradleProperties.Select(kv => $"{kv.Key}={kv.Value}"));
                }
            }

#if UNITY_2020
            string[] gradleFiles = new[]
            {
                "mainTemplate.gradle",
                "baseProjectTemplate.gradle",
                "launcherTemplate.gradle",
            };
#else
            string[] gradleFiles = new[]
            {
                "mainTemplate.gradle",
                "baseProjectTemplate.gradle"
            };
#endif

            var mainTemplateValues = GetMainTemplateValues();
            foreach (var file in gradleFiles)
            {
                string temp = Path.Combine(androidPluginPath, file);
                Global.CheckAndRemoveFile(temp);

                string tempFile = GetTemplatePath(file, projectTempPath, templatesPath);
                string tempContext = File.ReadAllText(tempFile);

                bool dirty = false;
                foreach (var val in mainTemplateValues)
                {
                    if (tempContext.Contains(val.Key))
                    {
                        tempContext = tempContext.Replace($"**{val.Key}**", val.Value);
                        dirty = true;
                    }
                }

                if (dirty)
                {
                    File.WriteAllText(temp, tempContext);
                }
            }

            AssetDatabase.Refresh();
#endif
        }

        private static Dictionary<string, string> GetAllGradleProperties()
        {
            var infos = new Dictionary<string, GradlePropertyInfo>();
            var props = Global.GetProperties<AndroidGradlePropertiesAttribute, BuildToolsAttribute>();

            foreach (var prop in props)
            {
                int priority = ((AndroidGradlePropertiesAttribute) prop.GetCustomAttributes(
                    typeof(AndroidGradlePropertiesAttribute), false).First()).Priority;

                var dict = prop.GetValue(null, null) as Dictionary<string, string>;
                if (dict == null)
                    continue;

                foreach (var kv in dict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    if (infos.TryGetValue(key, out var info))
                    {
                        if (info.Value.Equals(value))
                            continue;
                        if (info.Priority == priority)
                            throw new BuildException(
                                $"AndroidGradlePropertiesAttribute property {key} has different values: {info.Value}: {info.Priority} >> {value}:{priority}");
                        if (info.Priority > priority)
                            continue;
                        info.Value = value;
                    }
                    else
                    {
                        infos.Add(key, new GradlePropertyInfo
                        {
                            Key = key,
                            Value = value,
                            Priority = priority
                        });
                    }
                }
            }

            return infos.Values.ToDictionary(info => info.Key, info => info.Value);
        }

        private static Dictionary<string, string> GetDependLibs<T>() where T : Attribute
        {
            var result = new Dictionary<string, string>();

            var props = Global.GetProperties<T, BuildToolsAttribute>();
            foreach (var prop in props)
            {
                var dict = prop.GetValue(null, null) as Dictionary<string, string>;
                if (dict == null)
                    continue;

                foreach (var kv in dict)
                {
                    string key = kv.Key;
                    string value = kv.Value;
                    if (result.TryGetValue(key, out var version))
                    {
                        if (string.Compare(version, value, StringComparison.Ordinal) < 0)
                        {
                            result[key] = value;
                        }
                    }
                    else
                    {
                        result.Add(key, value);
                    }
                }
            }

            return result;
        }

        private static List<string> GetList<T>() where T : Attribute
        {
            var result = new List<string>();

            var props = Global.GetProperties<T, BuildToolsAttribute>();
            foreach (var prop in props)
            {
                var list = prop.GetValue(null, null) as List<string>;
                if (list == null)
                    continue;

                foreach (var val in list)
                {
                    if (result.Contains(val))
                        continue;

                    result.Add(val);
                }
            }

            return result;
        }

        private static Dictionary<string, string> GetMainTemplateValues()
        {
            var result = new Dictionary<string, string>();

            var depsMap = GetDependLibs<AndroidDependLibsAttribute>();
            if (depsMap.Count > 0)
            {
                List<string> deps = depsMap.Select(
                    kv => $"    implementation '{kv.Key}:{kv.Value}'"
                ).ToList();
                deps.Add("\n**DEPS**"); // 保留替换占位符
                result.Add("DEPS", string.Join("\n", deps));
            }

            var scriptMap = GetDependLibs<AndroidBuildScriptAttribute>();
            if (scriptMap.Count > 0)
            {
                List<string> deps = scriptMap.Select(
                    kv => $"    classpath '{kv.Key}:{kv.Value}'"
                ).ToList();
                deps.Add("\n**BUILD_SCRIPT_DEPS**"); // 保留替换占位符
                result.Add("BUILD_SCRIPT_DEPS", string.Join("\n", deps));
            }

            var pluginList = GetList<AndroidApplyPluginsAttribute>();
            if (pluginList.Count > 0)
            {
                List<string> plugins = pluginList.Select(val => $"apply plugin: \"{val}\"").ToList();
                plugins.Add("\n**APPLY_PLUGINS**"); // 保留替换占位符
                result.Add("APPLY_PLUGINS", string.Join("\n", plugins));
            }   

            var externalSources = GetList<AndroidExternalSourcesAttribute>();
            if (externalSources.Count > 0)
            {
                List<string> sources = new List<string>(externalSources);
                sources.Add("\n**SOURCE_BUILD_SETUP**"); // 保留替换占位符
                result.Add("SOURCE_BUILD_SETUP", string.Join("\n", sources));
            }

            return result;
        }
    }

    struct GradlePropertyInfo
    {
        public string Key;
        public string Value;
        public int Priority;
    }
}
