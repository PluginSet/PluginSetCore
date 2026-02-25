using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core.Editor
{
    public struct AudioPatchFileInfo
    {
        public string File;
        public string Name;
        public string Bundle;
    }
    
    public static class BuildGlobal
    {
        public static void BuildPatchesStart(this BuildProcessorContext context)
        {
            context.Set("isBuildPatches", true);
        }

        public static void BuildPatchesEnded(this BuildProcessorContext context)
        {
            context.Set("isBuildPatches", false);
        }

        public static bool IsBuildingPatches(this BuildProcessorContext context)
        {
            return context.TryGet<bool>("isBuildPatches", false);
        }

        public static bool IsSupportAudioPatch(this BuildProcessorContext context)
        {
            return context.TryGet<bool>("IsSupportAudioPatch", false);
        }

        public static void SetSupportAudioPatch(this BuildProcessorContext context, bool value)
        {
            context.Set<bool>("IsSupportAudioPatch", value);
        }

        public static void SetBuildingUpdatePatches(this BuildProcessorContext context)
        {
	        context.Set("IsBuildUpdatePatch", true);
        }
        
        public static bool IsBuildingUpdatePatches(this BuildProcessorContext context)
        {
            return context.TryGet("IsBuildUpdatePatch", false);
        }

        public static void SetBuildPaths(this BuildProcessorContext context, List<string> buildPaths)
        {
            context.Set("buildPaths", buildPaths);
        }

        public static List<string> GetBuildPaths(this BuildProcessorContext context, List<string> defaultResult)
        {
            return context.TryGet<List<string>>("buildPaths", defaultResult);
        }

        public static void ExtendStreamingFiles(this BuildProcessorContext context, params string[] files)
        {
            var list = context.TryGet<List<string>>("streamingExtendFiles", null);
            if (list == null)
            {
                list = new List<string>();
                context.Set("streamingExtendFiles", list);
            }

            list.AddRange(files);
        }
        
        public static List<string> GetStreamingExtendFiles(this BuildProcessorContext context)
        {
            return context.TryGet<List<string>>("streamingExtendFiles", null);
        }

        public static void AddMusicPatchFiles(this BuildProcessorContext context, string patchName, string name, string path, string bundleName = "")
        {
            var dict = context.TryGet<Dictionary<string, List<AudioPatchFileInfo>>>("audioPatch_music", null);
            if (dict == null)
            {
                dict = new Dictionary<string, List<AudioPatchFileInfo>>();
                context.Set("audioPatch_music", dict);
            }

            if (!dict.TryGetValue(patchName, out var list))
            {
                list = new List<AudioPatchFileInfo>();
                dict.Add(patchName, list);
            }

            if (list.Any(info => info.Name == name))
                throw new Exception($"Same music audio file named {name} in {patchName}, at {path}");

            list.Add(new AudioPatchFileInfo()
            {
                Name = name,
                File = path,
                Bundle = bundleName
            });
        }

        public static Dictionary<string, List<AudioPatchFileInfo>> GetMusicPatchFiles(this BuildProcessorContext context)
        {
            return context.TryGet("audioPatch_music", new Dictionary<string, List<AudioPatchFileInfo>>());
        }

        public static void AddSoundPatchFiles(this BuildProcessorContext context, string patchName, string name, string path, string bundleName = "")
        {
            var dict = context.TryGet<Dictionary<string, List<AudioPatchFileInfo>>>("audioPatch_sound", null);
            if (dict == null)
            {
                dict = new Dictionary<string, List<AudioPatchFileInfo>>();
                context.Set("audioPatch_sound", dict);
            }

            if (!dict.TryGetValue(patchName, out var list))
            {
                list = new List<AudioPatchFileInfo>();
                dict.Add(patchName, list);
            }

            if (list.Any(info => info.Name == name))
                throw new Exception($"Same sound audio file named {name} in {patchName}, at {path}");

            list.Add(new AudioPatchFileInfo()
            {
                Name = name,
                File = path,
                Bundle = bundleName,
            });
        }

        public static Dictionary<string, List<AudioPatchFileInfo>> GetSoundPatchFiles(this BuildProcessorContext context)
        {
            return context.TryGet("audioPatch_sound", new Dictionary<string, List<AudioPatchFileInfo>>());
        }

        public static void SetBuildResult(this BuildProcessorContext context, string key, object value)
        {
            var dict = context.TryGet<Dictionary<string, object>>("buildResultJson", null);
            if (dict == null)
            {
                dict = new Dictionary<string, object>();
                context.Set("buildResultJson", dict);
            }
            
            dict[key] = value;
        }
        
        public static void RemoveBuildResult(this BuildProcessorContext context, string key)
        {
            var dict = context.TryGet<Dictionary<string, object>>("buildResultJson", null);
            if (dict == null)
                return;

            if (dict.ContainsKey(key))
                dict.Remove(key);
        }

        public static List<string> CollectBuildPaths(this BuildProcessorContext context, List<string> originPaths)
        {
            if (!IsBuildingPatches(context))
                return originPaths;

            var buildPaths = GetBuildPaths(context, null);
            if (buildPaths == null || buildPaths.Count <= 0)
                return null;

            return Global.CollectSameItemsAndRemove(originPaths, buildPaths);
        }

        public static bool CheckNeedRebuildAssetBundles(this BuildProcessorContext context)
        {
            if (context.ForceBuildBundles)
                return true;
                
            foreach (var method in Global.GetMethods<CheckRebuildAssetBundlesAttribute, BuildToolsAttribute>())
            {
                if (method.Invoke(null, new object[] {context}).Equals(true))
                {
                    return true;
                }
            }
            
            return false;
        }
    }
}