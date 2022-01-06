using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : ISystemPlugin
    {
        private Dictionary<string, ISystemPlugin> _systemPlugins;

        private Dictionary<string, ISystemPlugin> SystemPlugins
        {
            get
            {
                if (_systemPlugins == null)
                {
                    _systemPlugins = new Dictionary<string, ISystemPlugin>();
                    foreach (var plugin in GetPlugins<ISystemPlugin>())
                    {
                        _systemPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _systemPlugins;
            }
        }

        public bool HasOnAppQuit => SystemPlugins.Count > 0;

        public void OnAppQuit(Action<Result> callback = null)
        {
            foreach (var kv in SystemPlugins)
            {
                kv.Value.OnAppQuit(callback);
            }
        }
    }
}
