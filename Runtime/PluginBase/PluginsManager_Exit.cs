using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : IExitPlugin
    {
        private Dictionary<string, IExitPlugin> _exitPlugins;

        private Dictionary<string, IExitPlugin> ExitPlugins
        {
            get
            {
                if (_exitPlugins == null)
                {
                    _exitPlugins = new Dictionary<string, IExitPlugin>();
                    foreach (var plugin in GetPlugins<IExitPlugin>())
                    {
                        _exitPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _exitPlugins;
            }
        }

        public bool HasExitDialog => ExitPlugins.Values.Any(plugin => plugin.HasExitDialog);

        public string GetExitDialogPlugin()
        {
            foreach (var plugin in ExitPlugins.Values)
            {
                if (plugin.HasExitDialog)
                    return plugin.Name;
            }

            return null;
        }
        
        public void ExitApplication()
        {
            var pluginName = GetExitDialogPlugin();
            if (string.IsNullOrEmpty(pluginName))
                return;
            
            ExitApplicationWith(pluginName);
        }

        public void ExitApplicationWith(string pluginName)
        {
            if (ExitPlugins.TryGetValue(pluginName, out var plugin))
                plugin.ExitApplication();
        }
    }
}
