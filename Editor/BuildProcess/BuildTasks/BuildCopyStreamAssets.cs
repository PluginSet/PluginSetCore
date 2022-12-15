using System;
using System.IO;
using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildCopyStreamAssets: BuildProcessorTask
    {
        public override void Execute(BuildProcessorContext context)
        {
	        var streamingAssetsPath = context.TryGet<string>("StreamingAssetsPath", null);
		    if (string.IsNullOrEmpty(streamingAssetsPath))
				return;
			
			if (!Directory.Exists(streamingAssetsPath))
				return;
            
            string projectPath = context.ProjectPath;
            string targetPath;
            if (context.BuildTarget == BuildTarget.Android)
            {
                targetPath = Path.Combine(projectPath, "unityLibrary", "src", "main", "assets");
            }
            else if (context.BuildTarget == BuildTarget.iOS)
            {
                targetPath = Path.Combine(projectPath, "Data", "Raw");
            }
            else
            {
                throw new Exception("Not Supported BuildTarget");
            }
            
            Global.CopyFilesTo(targetPath, streamingAssetsPath, "*");
        }
    }
}