using System.IO;
using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildEnd : IBuildProcessorTask
    {
        public void Execute(BuildProcessorContext context)
        {
            var md5FileName = context.TryGet<string>("md5FileName", null);
            var md5Context = context.TryGet<string>("md5Context", null);
            if (!string.IsNullOrEmpty(md5FileName) && !string.IsNullOrEmpty(md5Context))
                File.WriteAllText(md5FileName, md5Context);

            // clear temp paths
            foreach (var path in context.TemplatePaths)
            {
                Global.CheckAndDeletePath(path);
            }

            AssetDatabase.Refresh();
        }
    }
}
