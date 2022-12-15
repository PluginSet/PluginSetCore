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
            var projectChanged = CheckProjectChanged(context);
            if (projectChanged || context.CheckNeedRebuildAssetBundles())
            {
                task = handler.AddNextTask(new BuildPrepareBundles(), task);
                task = handler.AddNextTask(new BuildExportAssetBundles(), task);
                buildAssets = true;
            }
            
            if (projectChanged || context.ForceExportProject || CheckNeedExportProject(context))
            {
                Global.CheckAndDeletePath(projectPath);
                
                handler.AddNextTask(new BuildExportProjectOrApk(), task);
            }
            else if (buildAssets)
            {
                handler.AddNextTask(new BuildCopyStreamAssets(), task);
            }
        }

        private static bool CheckProjectChanged(BuildProcessorContext context)
        {
            string tag = context.BuildExportProjectTag();
            string md5FileName = Path.Combine(context.ProjectPath, "channelMd5.txt");
            if (File.Exists(md5FileName))
            {
                if (File.ReadAllText(md5FileName).Equals(tag))
                    return false;

                File.Delete(md5FileName);
            }

            context.Set("md5FileName", md5FileName);
            context.Set("md5Context", tag);
            return true;
        }

        private static bool CheckNeedExportProject(BuildProcessorContext context)
        {
            if (!Directory.Exists(context.ProjectPath))
                return true;
            
            return false;
        }
    }
}
