using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildCopyBundles: IBuildProcessorTask
    {
	    public void Execute(BuildProcessorContext context)
	    {
		    var streamingAssetsPath = context.TryGet<string>("StreamingAssetsPath", null);
		    if (string.IsNullOrEmpty(streamingAssetsPath))
				return;
			
			string targetPath = context.BuildPath;
			if (string.IsNullOrEmpty(targetPath))
				return;
			
			Global.MoveAllFilesToPath(streamingAssetsPath, targetPath);

			AssetDatabase.Refresh();
        }
    }
}