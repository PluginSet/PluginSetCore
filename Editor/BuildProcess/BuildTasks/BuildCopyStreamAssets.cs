using System;
using System.IO;
using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildCopyStreamAssets: BuildProcessorTask
    {
        public override void Execute(BuildProcessorContext context)
        {
            var streamingAssetsPath = context.StreamingAssetsPath;
		    if (string.IsNullOrEmpty(streamingAssetsPath))
				return;
			
			if (!Directory.Exists(streamingAssetsPath))
				return;
            
            string projectPath = context.ProjectPath;
            string targetPath;
            if (context.BuildTarget == BuildTarget.Android)
            {
                if (!Directory.Exists(Path.Combine(projectPath, "unityLibrary")) && Directory.Exists(Path.Combine(projectPath, "tuanjieLibrary")))
                    targetPath = Path.Combine(projectPath, "tuanjieLibrary", "src", "main", "assets");
                else
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