using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : IStore
    {
        private Dictionary<string, IStore> _storePlugin;

        private Dictionary<string, IStore> StorePlugin
        {
            get
            {
                if (_storePlugin == null)
                {
                    _storePlugin = new Dictionary<string, IStore>();
                    foreach (var plugin in GetPlugins<IStore>())
                    {
                        _storePlugin.Add(plugin.Name, plugin);
                    }
                }

                return _storePlugin;
            }
        }
        
        public void Review()
        {
            if (StorePlugin.Count <= 0)
                return;
            
            StorePlugin.First().Value?.Review();
        }

        public void ReviewWith(string pluginName)
        {
            if (StorePlugin.TryGetValue(pluginName, out var plugin))
            {
                plugin.Review();
            }
        }
    }
}
