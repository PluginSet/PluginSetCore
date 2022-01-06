using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildExportAssetBundles: IBuildProcessorTask
    {
        public void Execute(BuildProcessorContext context)
        {
	        var streamingAssetsName = context.TryGet<string>("StreamingAssetsName", null);
	        var streamingAssetsPath = context.TryGet<string>("StreamingAssetsPath", null);
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

			Global.CallCustomOrderMethods<OnBuildBundlesCompletedAttribute, BuildToolsAttribute>(context, streamingAssetsPath, streamingAssetsName, manifest, true);

			AssetDatabase.Refresh();
        }
    }
}