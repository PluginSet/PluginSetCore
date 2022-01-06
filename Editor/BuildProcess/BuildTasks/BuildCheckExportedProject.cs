using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildCheckExportedProject : IBuildProcessorTask
    {
        public void Execute(BuildProcessorContext context)
        {
            IBuildProcessorTask task = this;
            var handler = context.Get<BuildTaskHandler>("handler");
            string projectPath = context.ProjectPath;
            
            if (context.ForceExportProject || CheckNeedExportProject(context))
            {
                Global.CheckAndDeletePath(projectPath);
                if (context.ForceBuildBundles || CheckNeedExportAssetBundles(context))
                {
                    // 为了测试加快打包 可以不用每次都打包bundles
                    task = handler.AddNextTask(new BuildPrepareBundles(), task);
                    task = handler.AddNextTask(new BuildExportAssetBundles(), task);
                }
                
                task = handler.AddNextTask(new BuildExportProjectOrApk(), task);
                if (!context.ExportProject) return;
            }

            
            task = handler.AddNextTask(new BuildModifyAndroidProject(projectPath), task);
            task = handler.AddNextTask(new BuildModifyIOSProject(projectPath), task);
            task = handler.AddNextTask(new BuildGenerateApp(), task);

            Unuse(task);
        }

        private static bool CheckNeedExportProject(BuildProcessorContext context)
        {
            string md5 = GetBuildChannelsMd5(context.BuildChannels);
            string md5FileName = Path.Combine(context.ProjectPath, "channelMd5.txt");
            if (File.Exists(md5FileName))
            {
                if (File.ReadAllText(md5FileName).Equals(md5))
                    return false;

                File.Delete(md5FileName);
            }

            context.Set("md5FileName", md5FileName);
            context.Set("md5Context", md5);
            return true;
        }

        private static bool CheckNeedExportAssetBundles(BuildProcessorContext context)
        {
            return true; // TODO
        }

        private static string GetBuildChannelsMd5(BuildChannels asset)
        {
            var md5 = MD5.Create();
            var path = AssetDatabase.GetAssetPath(asset);
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(path));
            var buffer = new StringBuilder();
            var len = bytes.Length;
            for (int i = 0; i < len; i++)
            {
                buffer.Append(bytes[i]);
            }

            return buffer.ToString();
        }

        private static void Unuse(IBuildProcessorTask _)
        {
        }
    }
}
