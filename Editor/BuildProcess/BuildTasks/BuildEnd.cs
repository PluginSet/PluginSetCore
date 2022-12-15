using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildEnd : BuildProcessorTask
    {
        public override void Execute(BuildProcessorContext context)
        {
            var md5FileName = context.TryGet<string>("md5FileName", null);
            var md5Context = context.TryGet<string>("md5Context", null);
            if (!string.IsNullOrEmpty(md5FileName) && !string.IsNullOrEmpty(md5Context))
                File.WriteAllText(md5FileName, md5Context);

            var outJson = context.TryGet<Dictionary<string, object>>("buildResultJson", null);
            if (outJson != null && !string.IsNullOrEmpty(context.BuildPath))
            {
                outJson["channel"] = context.Channel;
                outJson["version"] = context.VersionName;
                outJson["build"] = context.Build;
                var outJsonFile = Path.Combine(context.BuildPath, "buildResult.json");
                File.WriteAllText(outJsonFile, MiniJSON.Json.Serialize(outJson));
            }

            // clear temp paths
            foreach (var path in context.TemplatePaths)
            {
                Global.CheckAndDeletePath(path);
            }

            AssetDatabase.Refresh();
        }
    }
}
