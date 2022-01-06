
namespace PluginSet.Core.Editor
{
    public class BuildInitialize: IBuildProcessorTask
    {
        public void Execute(BuildProcessorContext context)
        {
            // 初始化资源
            Global.CallCustomOrderMethods<OnFrameworkInitAttribute>(context);
        }
    }
}