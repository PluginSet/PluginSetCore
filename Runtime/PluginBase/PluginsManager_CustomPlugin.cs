using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: ICustomPlugin
    {
        private Dictionary<string, ICustomPlugin> _customPlugins;

        private Dictionary<string, ICustomPlugin> CustomPlugins
        {
            get
            {
                if (_customPlugins == null)
                {
                    _customPlugins = new Dictionary<string, ICustomPlugin>();
                    foreach (var plugin in GetPlugins<ICustomPlugin>())
                    {
                        _customPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _customPlugins;
            }
        }

        [Obsolete("CustomCall cannot execute without plugin name, please call CustomCallWith instead", true)]
        public void CustomCall(string func, Action<Result> callback = null, string json = null)
        {
            throw new Exception("CustomCall cannot execute without plugin name, please call CustomCallWith instead");
        }
        
        public void CustomCallWith(string pluginName, string func, Action<Result> callback = null, string json = null)
        {
            if (CustomPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.CustomCall(func, callback, json);
                return;
            }
            
            InvalidCallback(callback, pluginName);
        }
    }
}