namespace PluginSet.Core
{
    internal class PluginEventDispatcher: EventDispatcher<PluginsEventContext>
    {
        private static PluginEventDispatcher _globalDispatcher;

        internal static PluginEventDispatcher GlobalDispatcher
        {
            get
            {
                if (_globalDispatcher == null)
                    _globalDispatcher = new PluginEventDispatcher();

                return _globalDispatcher;
            }
        }
    }
}