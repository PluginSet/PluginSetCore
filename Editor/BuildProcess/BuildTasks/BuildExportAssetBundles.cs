using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildExportAssetBundles: BuildProcessorTask
    {
        public override void Execute(BuildProcessorContext context)
        {
	        var streamingAssetsName = context.StreamingAssetsName;
	        var streamingAssetsPath = context.StreamingAssetsPath;
	        if (string.IsNullOrEmpty(streamingAssetsName) || string.IsNullOrEmpty(streamingAssetsPath))
		        return;

			var manifest = context.BuildAssetBundle(streamingAssetsPath);
			if (manifest == null)
			{
				Global.CallCustomOrderMethods<OnBuildBundlesCompletedAttribute, BuildToolsAttribute>(context, streamingAssetsPath, streamingAssetsName, null, false);
				if (context.BuildNothing())
					return;
				throw new BuildException("BuildAssetBundles: Nothing build!");
			}

			context.Set("AssetBundleManifest", manifest);

			Global.CallCustomOrderMethods<OnBuildBundlesCompletedAttribute, BuildToolsAttribute>(context, streamingAssetsPath, streamingAssetsName, manifest, context.IsBuildingUpdatePatches());

			AssetDatabase.Refresh();
        }
    }
}