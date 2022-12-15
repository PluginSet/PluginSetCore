using System.Collections.Generic;

namespace PluginSet.Core.Editor
{
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