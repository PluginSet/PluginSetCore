using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public partial class PluginsManager: IAnalytics
    {
        private Dictionary<string, IAnalytics> _analyticsPlugins;

        private Dictionary<string, IAnalytics> AnalyticsPlugins
        {
            get
            {
                if (_analyticsPlugins == null)
                {
                    _analyticsPlugins = new Dictionary<string, IAnalytics>();
                    foreach (var plugin in GetPlugins<IAnalytics>())
                    {
                        _analyticsPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _analyticsPlugins;
            }
        }

        public void CustomEvent(string customEventName, Dictionary<string, object> eventData = null)
        {
            foreach (var plugin in AnalyticsPlugins.Values)
            {
                plugin.CustomEvent(customEventName, eventData);
            }
        }
        
        public void CustomEventWith(string pluginName, string customEventName, Dictionary<string, object> eventData = null)
        {
            if (AnalyticsPlugins.TryGetValue(pluginName, out var plugin))
                plugin.CustomEvent(customEventName, eventData);
        }
    }
}