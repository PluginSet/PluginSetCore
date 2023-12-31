using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class BuildPreparePatches: BuildProcessorTask
    {
        public override void Execute(BuildProcessorContext context)
        {
            var streamingAssetsPath = context.StreamingAssetsPath;
            if (string.IsNullOrEmpty(streamingAssetsPath))
	            return;
            
            context.SetBuildingUpdatePatches();

	        var modFiles = new List<string>();
	        var addFiles = new List<string>();
		        
			var patchData = context.PatchFiles;
			if (patchData != null)
			{
				if (patchData.ModFiles != null)
					modFiles.AddRange(patchData.ModFiles);
				
				if (patchData.AddFiles != null)
					addFiles.AddRange(patchData.AddFiles);
			}

			context.Set("UpdatePatchModFiles", modFiles);
			context.Set("UpdatePatchAddFiles", addFiles);
			
			context.SetBuildResult("UpdatePatchModFiles", string.Join(",", modFiles));
			context.SetBuildResult("UpdatePatchAddFiles", string.Join(",", addFiles));
				
			Global.CheckAndDeletePath(streamingAssetsPath);
			Directory.CreateDirectory(streamingAssetsPath);
	        
            Global.CallCustomOrderMethods<OnBuildPatchesAttribute, BuildToolsAttribute>(context);
        }
    }
}