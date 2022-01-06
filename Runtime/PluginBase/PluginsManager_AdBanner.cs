using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public partial class PluginsManager : IBannerAdPlugin
    {
        private Dictionary<string, IBannerAdPlugin> _bannerAdPlugin;

        private Dictionary<string, IBannerAdPlugin> BannerAdPlugins
        {
            get
            {
                if (_bannerAdPlugin == null)
                {
                    _bannerAdPlugin = new Dictionary<string, IBannerAdPlugin>();
                    foreach (var plugin in GetPlugins<IBannerAdPlugin>())
                    {
                        _bannerAdPlugin.Add(plugin.Name, plugin);
                    }
                }

                return _bannerAdPlugin;
            }
        }

        public bool IsEnableShowBanner => GetValidBannerTypes().Length > 0;
        
        public string[] GetBannerTypes()
        {
            return BannerAdPlugins.Keys.ToArray();
        }

        public string[] GetValidBannerTypes()
        {
            return BannerAdPlugins.Where(kv => kv.Value.IsEnableShowBanner).Select(kv => kv.Key).ToArray();
        }

        public bool EnableShowBanner(string pluginName)
        {
            if (BannerAdPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShowBanner;

            return false;
        }
        
        public void ShowBannerAd(string adId, BannerPosition position = BannerPosition.BottomCenter, Dictionary<string, object> extensions = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShowBannerAdWith((string) context.Data, adId, position, extensions);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_BANNER_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void ShowBannerAdWith(string pluginName, string adId,
            BannerPosition position = BannerPosition.BottomCenter, Dictionary<string, object> extensions = null)
        {
            if (BannerAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowBanner)
                {
                    plugin.ShowBannerAd(adId, position, extensions);
                }
            }
        }
        

        public void HideBannerAd(string adId)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                HideBannerAdWith((string) context.Data, adId);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_BANNER_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        
        public void HideBannerAdWith(string pluginName, string adId)
        {
            if (BannerAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowBanner)
                {
                    plugin.HideBannerAd(adId);
                }
            }
        }

        public void HideAllBanners()
        {
            foreach (var plugin in BannerAdPlugins.Values)
            {
                if (plugin.IsEnableShowBanner)
                    plugin.HideAllBanners();
            }
        }
        
        public void HideAllBannersWith(string pluginName)
        {
            if (BannerAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowBanner)
                {
                    plugin.HideAllBanners();
                }
            }
        
        }
    }
}