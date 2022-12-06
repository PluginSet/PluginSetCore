using System.IO;
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
			
//			Global.MoveAllFilesToPath(streamingAssetsPath, targetPath);
		    if (!context.IsBuildingPatches())
			    targetPath = Path.Combine(targetPath, "..", "patches"); // TODO
            Global.CopyFilesTo(targetPath, streamingAssetsPath, "*");

			AssetDatabase.Refresh();
        }
    }
}