using System.IO;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class BuildPrepareBundles : BuildProcessorTask
    {
        public override void Execute(BuildProcessorContext context)
        {
            var manager = ResourcesManager.Instance;
            if (manager == null) return;
            
            var streamingAssetsPath = Path.Combine(Application.dataPath, manager.StreamingAssetsName);
            streamingAssetsPath = Global.GetFullPath(streamingAssetsPath);
            context.Set("StreamingAssetsName", manager.StreamingAssetsName);
            context.Set("StreamingAssetsPath", streamingAssetsPath);
            
			Global.CheckAndDeletePath(streamingAssetsPath);
			Directory.CreateDirectory(streamingAssetsPath);
            
            Global.CallCustomOrderMethods<OnBuildBundlesAttribute, BuildToolsAttribute>(context);
        }
    }
}
