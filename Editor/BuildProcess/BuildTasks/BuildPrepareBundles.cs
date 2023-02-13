using System.IO;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class BuildPrepareBundles : BuildProcessorTask
    {
        public override void Execute(BuildProcessorContext context)
        {
            var streamingAssetsPath = context.StreamingAssetsPath;
            if (string.IsNullOrEmpty(streamingAssetsPath))
                return;
            
			Global.CheckAndDeletePath(streamingAssetsPath);
			Directory.CreateDirectory(streamingAssetsPath);
            
            Global.CallCustomOrderMethods<OnBuildBundlesAttribute, BuildToolsAttribute>(context);
        }
    }
}
