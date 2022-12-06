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
			
			targetPath = Path.Combine(targetPath, "Patches");
//			Global.MoveAllFilesToPath(streamingAssetsPath, targetPath);
            Global.CopyFilesTo(targetPath, streamingAssetsPath, "*");

			AssetDatabase.Refresh();
        }
    }
}