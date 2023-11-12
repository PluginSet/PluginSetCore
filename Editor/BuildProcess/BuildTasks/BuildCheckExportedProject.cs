using System.IO;

namespace PluginSet.Core.Editor
{
    public class BuildCheckExportedProject : BuildProcessorTask
    {
        public override void Execute(BuildProcessorContext context)
        {
            BuildProcessorTask task = this;
            var handler = this.Handler;
            string projectPath = context.ProjectPath;

            var buildAssets = false;
            if (context.CheckNeedRebuildAssetBundles())
            {
                task = handler.AddNextTask(new BuildPrepareBundles(), task);
                task = handler.AddNextTask(new BuildExportAssetBundles(), task);
                buildAssets = true;
            }
            
            if (context.ForceExportProject || CheckNeedExportProject(context))
            {
                Global.CheckAndDeletePath(projectPath);
                
                handler.AddNextTask(new BuildExportProjectOrApk(), task);
            }
            else if (buildAssets)
            {
                handler.AddNextTask(new BuildCopyStreamAssets(), task);
            }
        }

        private static bool CheckNeedExportProject(BuildProcessorContext context)
        {
            if (!Directory.Exists(context.ProjectPath))
                return true;
            
            return false;
        }
    }
}
