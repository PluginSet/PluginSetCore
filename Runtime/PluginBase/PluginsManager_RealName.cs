using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public partial class PluginsManager: IRealNamePlugin
    {
        private Dictionary<string, IRealNamePlugin> _realNamePlugins;

        private Dictionary<string, IRealNamePlugin> RealNamePlugins
        {
            get
            {
                if (_realNamePlugins == null)
                {
                    _realNamePlugins = new Dictionary<string, IRealNamePlugin>();
                    foreach (var plugin in GetPlugins<IRealNamePlugin>())
                    {
                        _realNamePlugins.Add(plugin.Name, plugin);
                    }
                }

                return _realNamePlugins;
            }
        }

        public bool IsEnableVerifyRealName => RealNamePlugins.Values.Any(plugin => plugin.IsEnableVerifyRealName);

        public bool IsPluginEnableVerifyRealName(string pluginName)
        {
            if (RealNamePlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableVerifyRealName;

            return false;
        }
        
        [Obsolete("Please use VerifyRealNameWith instead")]
        public void VerifyRealName(Action<Result> callback = null)
        {
            throw new Exception("Please use VerifyRealNameWith instead");
        }
        
        public void VerifyRealNameWith(string pluginName, Action<Result> callback = null)
        {
            if (RealNamePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.VerifyRealName(callback);
                return;
            }
            
            InvalidCallback(callback, pluginName);
        }
    }
}