using System.IO;

namespace PluginSet.Core.Editor
{
    public class BuildCheckExportedProject : IBuildProcessorTask
    {
        public void Execute(BuildProcessorContext context)
        {
            IBuildProcessorTask task = this;
            var handler = context.Get<BuildTaskHandler>("handler");
            string projectPath = context.ProjectPath;
            
            var projectChanged = CheckProjectChanged(context);
            var buildAssets = false;
            if (projectChanged || context.ForceBuildBundles || CheckNeedExportAssetBundles(context))
            {
                // 为了测试加快打包 可以不用每次都打包bundles
                task = handler.AddNextTask(new BuildPrepareBundles(), task);
                task = handler.AddNextTask(new BuildExportAssetBundles(), task);
                task = handler.AddNextTask(new BuildCopyBundles(), task);
                buildAssets = true;
            }
            
            if (projectChanged || context.ForceExportProject || CheckNeedExportProject(context))
            {
                Global.CheckAndDeletePath(projectPath);
                
                task = handler.AddNextTask(new BuildExportProjectOrApk(), task);
                if (!context.ExportProject) return;
            }
            else if (buildAssets)
            {
                task = handler.AddNextTask(new BuildCopyStreamAssets(), task);
            }

            
            task = handler.AddNextTask(new BuildModifyAndroidProject(projectPath), task);
            task = handler.AddNextTask(new BuildModifyIOSProject(projectPath), task);

            Unuse(task);
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


        private static bool CheckNeedExportAssetBundles(BuildProcessorContext context)
        {
	        var streamingAssetsName = context.TryGet<string>("StreamingAssetsName", null);
	        var streamingAssetsPath = context.TryGet<string>("StreamingAssetsPath", null);
            if (string.IsNullOrEmpty(streamingAssetsPath))
                return false;

            if (!File.Exists(Path.Combine(streamingAssetsPath, streamingAssetsName.ToLower())))
                return true;
            
            return false;
        }

        private static void Unuse(IBuildProcessorTask _)
        {
        }
    }
}
