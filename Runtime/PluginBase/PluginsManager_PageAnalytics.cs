using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IPageAnalytics
    {
        private Dictionary<string, IPageAnalytics> _pageAnalyticsPlugins;
        
        private Dictionary<string, IPageAnalytics> PageAnalyticsPlugins
        {
            get
            {
                if (_pageAnalyticsPlugins == null)
                {
                    _pageAnalyticsPlugins = new Dictionary<string, IPageAnalytics>();
                    foreach (var plugin in GetPlugins<IPageAnalytics>())
                    {
                        _pageAnalyticsPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _pageAnalyticsPlugins;
            }
        }

        public void PageStart(string pageName, string pageClassOverride)
        {
            foreach (var plugin in PageAnalyticsPlugins.Values)
            {
                plugin.PageStart(pageName, pageClassOverride);
            }
        }

        public void PageEnd(string pageName)
        {
            foreach (var plugin in PageAnalyticsPlugins.Values)
            {
                plugin.PageEnd(pageName);
            }
        }
        
        public void PageStartWith(string pluginName, string pageName, string pageClassOverride)
        {
            if (PageAnalyticsPlugins.TryGetValue(pluginName, out var plugin))
                plugin.PageStart(pageName, pageClassOverride);
        }
        
        public void PageEndWith(string pluginName, string pageName)
        {
            if (PageAnalyticsPlugins.TryGetValue(pluginName, out var plugin))
                plugin.PageEnd(pageName);
        }
    }
}