using System.IO;
using PluginSetCore.Editor.WebGL;

namespace PluginSet.Core.Editor
{
    public class BuildModifyWebGLProject: BuildProcessorTask
    {
        private string ProjectPath;
        public BuildModifyWebGLProject(string projectPath)
        {
            ProjectPath = projectPath;
        }
            
        
        public override void Execute(BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<WebGLBuildParams>();
            var templatePath = buildParams.ExtraTemplates;
            if (!string.IsNullOrEmpty(templatePath) && Directory.Exists(templatePath))
            {
                foreach (var file in Directory.GetFiles(templatePath, "*.*", SearchOption.AllDirectories))
                {
                    var path = Global.GetSubPath(templatePath, file);
                    if (path.StartsWith("."))
                        continue;
                    if (Path.GetFileNameWithoutExtension(path).StartsWith("."))
                        continue;
                    
                    Global.CopyFile(file, Path.Combine(ProjectPath, path), null);
                }
            }
            
            Global.CallCustomOrderMethods<WebGLProjectModifyAttribute, BuildToolsAttribute>(context, ProjectPath);
        }
    }
}