using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public partial class PluginsManager : IRewardAdPlugin
    {
        private Dictionary<string, IRewardAdPlugin> _rewardAdPlugins;

        private Dictionary<string, IRewardAdPlugin> RewardAdPlugins
        {
            get
            {
                if (_rewardAdPlugins == null)
                {
                    _rewardAdPlugins = new Dictionary<string, IRewardAdPlugin>();
                    foreach (var plugin in GetPlugins<IRewardAdPlugin>())
                    {
                        _rewardAdPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _rewardAdPlugins;
            }
        }

        public bool IsEnableShowRewardAd => GetValidRewardAdTypes().Length > 0;

        public bool IsReadyToShowRewardAd => RewardAdPlugins.Any(kv => kv.Value.IsReadyToShowRewardAd);

        public string[] GetRewardAdTypes()
        {
            return RewardAdPlugins.Keys.ToArray();
        }

        public string[] GetValidRewardAdTypes()
        {
            return RewardAdPlugins.Where(kv => kv.Value.IsEnableShowRewardAd).Select(kv => kv.Key).ToArray();
        }
        
        public string GetReadyRewardAdPlugin()
        {
            foreach (var plugin in RewardAdPlugins.Values)
            {
                if (plugin.IsReadyToShowRewardAd)
                    return plugin.Name;
            }

            return null;
        }

        public bool IsReadyShowRewardAd(string pluginName)
        {
            if (RewardAdPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsReadyToShowRewardAd;

            return false;
        }

        public bool EnableShowRewardAd(string pluginName)
        {
            if (RewardAdPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShowRewardAd;

            return false;
        }

        public void LoadRewardAd(Action success = null, Action<int> fail = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                LoadRewardAdWith((string) context.Data, success, fail);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_REWARD_AD_TYPE, context))
            {
                fail?.Invoke(PluginConstants.InvalidCode);
                PluginsEventContext.Return(context);
            }
        }

        public void LoadRewardAdWith(string pluginName, Action success = null, Action<int> fail = null)
        {
            if (RewardAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowRewardAd)
                {
                    plugin.LoadRewardAd(success, fail);
                    return;
                }
            }

            fail?.Invoke(PluginConstants.InvalidCode);
        }

        public void ShowRewardAd(Action<bool, int> dismiss = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShowRewardAdWith((string) context.Data, dismiss);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_REWARD_AD_TYPE, context))
            {
                var plugin = GetReadyRewardAdPlugin();
                if (string.IsNullOrEmpty(plugin))
                    dismiss?.Invoke(false, PluginConstants.InvalidCode);
                else
                    ShowRewardAdWith(plugin, dismiss);
                PluginsEventContext.Return(context);
            }
        }

        [Obsolete("Use GetLoadedRewardAdInfoWith instead")]
        public AdInfo GetLoadedRewardAdInfo()
        {
            throw new NotImplementedException();
        }

        public AdInfo GetLoadedRewardAdInfoWith(string pluginName)
        {
            if (RewardAdPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.GetLoadedRewardAdInfo();

            return AdInfo.Null;
        }

        public void ShowRewardAdWith(string pluginName, Action<bool, int> dismiss = null)
        {
            if (RewardAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowRewardAd)
                {
                    plugin.ShowRewardAd(dismiss);
                    return;
                }
            }

            dismiss?.Invoke(false, PluginConstants.InvalidCode);
        }
    }
}
