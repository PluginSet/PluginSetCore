using System.IO;
using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildCopyBundles: BuildProcessorTask
    {
	    public override void Execute(BuildProcessorContext context)
	    {
		    var streamingAssetsPath = context.TryGet<string>("StreamingAssetsPath", null);
		    if (string.IsNullOrEmpty(streamingAssetsPath))
				return;
			
			if (!Directory.Exists(streamingAssetsPath))
				return;
			
			string targetPath = context.BuildPath;
			if (string.IsNullOrEmpty(targetPath))
				return;
			
			targetPath = Path.Combine(targetPath, "Patches");
            Global.CopyFilesTo(targetPath, streamingAssetsPath, "*");
			context.SetBuildResult("patchesPath", Path.GetFullPath(targetPath));

			AssetDatabase.Refresh();
        }
    }
}