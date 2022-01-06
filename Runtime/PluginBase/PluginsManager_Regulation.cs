using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : IRegulationPlugin
    {
        private Dictionary<string, IRegulationPlugin> _regulationPlugins;

        private Dictionary<string, IRegulationPlugin> RegulationPlugins
        {
            get
            {
                if (_regulationPlugins == null)
                {
                    _regulationPlugins = new Dictionary<string, IRegulationPlugin>();
                    foreach (var plugin in GetPlugins<IRegulationPlugin>())
                    {
                        _regulationPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _regulationPlugins;
            }
        }

        public bool AllowPrivateTraffic => RegulationPlugins.Count == 0 || RegulationPlugins.All(kv => kv.Value.AllowPrivateTraffic);

        public bool ShowTosOnLoginScreen => RegulationPlugins.Any(kv => kv.Value.ShowTosOnLoginScreen);

        public bool AllowGuestLogin => RegulationPlugins.Count == 0 || RegulationPlugins.All(kv => kv.Value.AllowPrivateTraffic);
    }
}
