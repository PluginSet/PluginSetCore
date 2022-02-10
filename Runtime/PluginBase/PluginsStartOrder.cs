namespace PluginSet.Core
{
    public static class PluginsStartOrder
    {
        public const int AppStart = int.MinValue;
        
        public const int ResourceManager = -1000000;

        public const int SdkDefault = -100;

        public const int Default = 0;

        public const int VirtualMachine = 1000000;
    }
}