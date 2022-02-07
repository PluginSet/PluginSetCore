using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PluginSet.Core.Editor
{
    [Serializable]
    public class PatchFiles
    {
        public string[] AddFiles;
        public string[] ModFiles;
    }

    public class BuildProcessorContext
    {
        private class LinkTypes
        {
            public string Assembly;
            public string[] Types;
        }

        public static BuildProcessorContext Current { get; protected set; }

        public static BuildProcessorContext Default()
        {
            return Current = new BuildProcessorContext().LoadFromDefault();
        }

        public static BuildProcessorContext BatchMode()
        {
            return Current = new BuildProcessorContext().LoadFromCommand();
        }

        public BuildTarget BuildTarget;
        public BuildTargetGroup BuildTargetGroup;
        public bool ForEditor = false;

        public string Channel;
        public bool DebugMode;
        /// <summary>
        /// 是否需要符号表 android
        /// </summary>
        public bool ProductMode;
        public string VersionName;
        public string VersionCode;
        public string Build;
        public bool ExportProject;
        public bool ForceBuildBundles = true;
        public bool ForceExportProject = true;
        public PatchFiles PatchFiles;
        public string BuildPath;

        public string ProjectPath => Path.Combine(BuildPath, Channel);

        public List<string> Symbols = new List<string>();
        public List<string> TemplatePaths = new List<string>();

        private Dictionary<string, LinkTypes> LinkAssemblies = new Dictionary<string, LinkTypes>();
        
        public Dictionary<string, string> CommandArgs { get; private set; }


        public XmlDocument LinkDocument
        {
            get
            {
                if (LinkAssemblies.Count <= 0)
                    return null;

                XmlDocument linkDoc = new XmlDocument();
                linkDoc.LoadXml("<linker></linker>");

                foreach (var info in LinkAssemblies.Values)
                {
                    XmlElement ele = linkDoc.createElementWithPath("/linker/assembly");
                    ele.SetAttribute("fullname", info.Assembly);
                    if (info.Types != null && info.Types.Length > 0)
                    {
                        foreach (var typeName in info.Types)
                        {
                            var node = ele.createSubElement("type");
                            node.SetAttribute("fullname", typeName);
                            node.SetAttribute("preserve", "all");
                        }
                    }
                    else
                    {
                        ele.SetAttribute("preserve", "all");
                    }
                }

                return linkDoc;
            }
        }

        public BuildChannels BuildChannels
        {
            get
            {
                if (_buildChannels == null)
                    _buildChannels = BuildChannels.GetAsset(Channel);

                if (_buildChannels == null)
                    throw new BuildException($"Cannot load BuildChannel with {Channel} at platform {BuildTarget}");

                if (!_buildChannels.IsMatchToPlatform(BuildTarget))
                    throw new BuildException($"The loaded BuildChannel with {Channel} is not match platform {BuildTarget}");

                return _buildChannels;
            }
        }

        private Dictionary<string, List<string>> _buildMap = new Dictionary<string, List<string>>();

        private Dictionary<string, object> _data = new Dictionary<string, object>();

        private BuildChannels _buildChannels;

        private BuildProcessorContext LoadFromDefault()
        {
            Reset();

            var editorSetting = EditorSetting.Asset;
            Channel = editorSetting.CurrentChannel;
            
            Build = editorSetting.Build.ToString();
            VersionName = editorSetting.versionName;
            VersionCode = editorSetting.versionCode.ToString();

            ForceBuildBundles = editorSetting.IsForceBuildBundles;
            ForceExportProject = editorSetting.IsForceExportProdject;

            Debug.Log($"LoadFromDefault VersionName:{VersionName} VersionCode:{VersionCode}");
#if UNITY_ANDROID
            ExportProject = true;// EditorUserBuildSettings.exportAsGoogleAndroidProject;
#else
            ExportProject = true;
#endif
            DebugMode = EditorUserBuildSettings.development;
            ProductMode = editorSetting.BuildProductMode;
            
            InitDataWithCommand(editorSetting.CommandsSimulation);
            
            return this;
        }
        

        private BuildProcessorContext LoadFromCommand()
        {
            Reset();

            ForceBuildBundles = true;
            ForceExportProject = true;
            ExportProject = true;
            DebugMode = false;
            
            InitDataWithCommand(Environment.GetCommandLineArgs());
            return this;
        }

        private void InitDataWithCommand(params string[] args)
        {
            CommandArgs = Global.GetCommandParams(args, "-");
            DebugMode = CommandArgs.ContainsKey("debug");
            ProductMode = CommandArgs.ContainsKey("product");
            BuildPath = CommandArgs.TryGet("path", BuildPath);
            Channel = CommandArgs.TryGet("channel", Channel);
            VersionName = CommandArgs.TryGet("versionname", VersionName);
            VersionCode = CommandArgs.TryGet("versioncode", VersionCode);
            PatchFiles = JsonUtility.FromJson<PatchFiles>(CommandArgs.TryGet("patchdata", "{}"));
            Build = CommandArgs.TryGet("build", Build);
        }

        private void Reset()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            BuildTarget = target;
            VersionName = PlayerSettings.bundleVersion;

            switch (target)
            {
                case BuildTarget.Android:
                    BuildTargetGroup = BuildTargetGroup.Android;
                    VersionCode = PlayerSettings.Android.bundleVersionCode.ToString();
                    break;
                case BuildTarget.iOS:
                    BuildTargetGroup = BuildTargetGroup.iOS;
                    VersionCode = PlayerSettings.iOS.buildNumber;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, $"BuildTarget:{target} is not support");
            }

            BuildPath = Path.Combine(Application.dataPath, "..", "Build");
            DebugMode = false;
            ProductMode = false;
            PatchFiles = null;
            Channel = "default";
            Build = "0";

            _buildChannels = null;

            _buildMap.Clear();
            _data.Clear();

            Symbols.Clear();
            LinkAssemblies.Clear();
            TemplatePaths.Clear();
        }

        public T Get<T>(string key)
        {
            return (T) _data[key];
        }

        public T TryGet<T>(string key, T defaultValue)
        {
            object value;
            if (_data.TryGetValue(key, out value))
            {
                return (T) value;
            }

            return defaultValue;
        }

        public void Set<T>(string key, T value)
        {
            _data[key] = value;
        }

        public void RemoveBuildBundle(string name)
        {
            if (_buildMap.ContainsKey(name))
                _buildMap.Remove(name);
        }

        public void AddBuildBundle(string name, params string[] files)
        {
            List<string> list;
            name = name.ToLower(); // 保持bundle名称为全小写
            string[] paths = Global.GetLowerSubPaths(".", files);
            if (_buildMap.TryGetValue(name, out list))
            {
                list.AddRange(paths);
            }
            else
            {
                _buildMap.Add(name, new List<string>(paths));
            }
        }

        public void ClearBuildBundles()
        {
            _buildMap.Clear();
        }

        public bool BuildNothing()
        {
            return _buildMap.Count <= 0;
        }

        public AssetBundleBuild[] GetBundleBuilds(string variant)
        {
            List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
            foreach (var kv in _buildMap)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                if (string.IsNullOrEmpty(variant))
                    build.assetBundleName = kv.Key;
                else
                    build.assetBundleName = $"{kv.Key}.{variant}";

                var list = kv.Value.Distinct().ToList();
                list.Sort();
                build.assetNames = list.ToArray();
                builds.Add(build);
            }

            builds.Sort((a, b) => string.CompareOrdinal(a.assetBundleName, b.assetBundleName));

            return builds.ToArray();
        }

        public AssetBundleManifest BuildAssetBundle(string exportPath, string variant = "")
        {
            var builds = GetBundleBuilds(variant);
            ClearBuildBundles();
            if (builds.Length <= 0)
                return null;

            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            var target = BuildTarget;
            var options = BuildChannels.BuildAssetBundleOptions;
            options |= BuildAssetBundleOptions.DeterministicAssetBundle;
            options |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
            options ^= BuildAssetBundleOptions.AppendHashToAssetBundleName;
            return Global.BuildAssetBundles(target, exportPath, builds, options);
        }

        public void AddLinkAssembly(string name, params string[] typeNames)
        {
            if (LinkAssemblies.TryGetValue(name, out var info))
            {
                if (info.Types == null || info.Types.Length <= 0)
                    return;

                if (typeNames == null || typeNames.Length <= 0)
                {
                    info.Types = null;
                    return;
                }

                var list = new List<string>(info.Types);
                list.AddRange(typeNames);
                info.Types = list.Distinct().ToArray();
            }
            else
            {
                LinkAssemblies.Add(name, new LinkTypes
                {
                    Assembly = name,
                    Types = typeNames
                });
            }
        }
    }
}
