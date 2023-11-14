using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public partial class PluginsManager : IInterstitialAdPlugin
    {
        private Dictionary<string, IInterstitialAdPlugin> _interstitialAdPlugins;

        private Dictionary<string, IInterstitialAdPlugin> InterstitialAdPlugins
        {
            get
            {
                if (_interstitialAdPlugins == null)
                {
                    _interstitialAdPlugins = new Dictionary<string, IInterstitialAdPlugin>();
                    foreach (var plugin in GetPlugins<IInterstitialAdPlugin>())
                    {
                        _interstitialAdPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _interstitialAdPlugins;
            }
        }

        public bool IsEnableShowInterstitialAd => GetValidInterstitialTypes().Length > 0;
        
        public bool IsReadyToShowInterstitialAd => InterstitialAdPlugins.Any(kv => kv.Value.IsReadyToShowInterstitialAd);

        public string[] GetInterstitialTypes()
        {
            return InterstitialAdPlugins.Keys.ToArray();
        }

        public string[] GetValidInterstitialTypes()
        {
            return InterstitialAdPlugins.Where(kv => kv.Value.IsEnableShowInterstitialAd).Select(kv => kv.Key).ToArray();
        }
        
        public string GetReadyInterstitialPlugin()
        {
            foreach (var plugin in InterstitialAdPlugins.Values)
            {
                if (plugin.IsReadyToShowInterstitialAd)
                    return plugin.Name;
            }

            return null;
        }
        
        public bool IsReadyShowInterstitialAd(string pluginName)
        {
            if (InterstitialAdPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsReadyToShowInterstitialAd;

            return false;
        }

        public bool EnableShowInterstitial(string pluginName)
        {
            if (InterstitialAdPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShowInterstitialAd;

            return false;
        }

        public void LoadInterstitialAd(Action success = null, Action<int> fail = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                LoadInterstitialAdWith((string) context.Data, success, fail);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_INTERSTITIAL_AD_TYPE, context))
            {
                fail?.Invoke(PluginConstants.InvalidCode);
                PluginsEventContext.Return(context);
            }
        }

        public void LoadInterstitialAdWith(string pluginName, Action success = null, Action<int> fail = null)
        {
            if (InterstitialAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowInterstitialAd)
                {
                    plugin.LoadInterstitialAd(success, fail);
                    return;
                }
            }

            fail?.Invoke(PluginConstants.InvalidCode);
        }

        public void ShowInterstitialAd(Action<bool, int> dismiss = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShowInterstitialAdWith((string) context.Data, dismiss);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_INTERSTITIAL_AD_TYPE, context))
            {
                dismiss?.Invoke(false, PluginConstants.InvalidCode);
                PluginsEventContext.Return(context);
            }
        }

        public void ShowInterstitialAdWith(string pluginName, Action<bool, int> dismiss = null)
        {
            if (InterstitialAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowInterstitialAd)
                {
                    plugin.ShowInterstitialAd(dismiss);
                    return;
                }
            }
            
            dismiss?.Invoke(false, PluginConstants.InvalidCode);
        }
    }
}