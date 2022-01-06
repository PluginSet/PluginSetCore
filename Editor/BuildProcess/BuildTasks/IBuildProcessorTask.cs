namespace PluginSet.Core.Editor
{
    public interface IBuildProcessorTask
    {
        void Execute(BuildProcessorContext context);
    }
}