using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public partial class PluginsManager : IOpenAdPlugin
    {
        private Dictionary<string, IOpenAdPlugin> _openAdPlugins;

        private Dictionary<string, IOpenAdPlugin> OpenAdPlugins
        {
            get
            {
                if (_openAdPlugins == null)
                {
                    _openAdPlugins = new Dictionary<string, IOpenAdPlugin>();
                    foreach (var plugin in GetPlugins<IOpenAdPlugin>())
                    {
                        _openAdPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _openAdPlugins;
            }
        }

        public bool IsEnableShowOpenAd => GetValidOpenAdTypes().Length > 0;

        public bool IsReadyToShowOpenAd => OpenAdPlugins.Any(kv => kv.Value.IsReadyToShowOpenAd);

        public string[] GetOpenAdTypes()
        {
            return OpenAdPlugins.Keys.ToArray();
        }

        public string[] GetValidOpenAdTypes()
        {
            return OpenAdPlugins.Where(kv => kv.Value.IsEnableShowOpenAd).Select(kv => kv.Key).ToArray();
        }
        
        public string GetReadyOpenAdPlugin()
        {
            foreach (var plugin in OpenAdPlugins.Values)
            {
                if (plugin.IsReadyToShowOpenAd)
                    return plugin.Name;
            }

            return null;
        }

        public bool IsReadyShowOpenAd(string pluginName)
        {
            if (OpenAdPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsReadyToShowOpenAd;

            return false;
        }

        public bool EnableShowOpenAd(string pluginName)
        {
            if (OpenAdPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShowOpenAd;

            return false;
        }

        public void LoadOpenAd(Action success = null, Action<int> fail = null, Dictionary<string, object> extParams = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                LoadOpenAdWith((string) context.Data, success, fail, extParams);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_OPEN_AD_TYPE, context))
            {
                fail?.Invoke(PluginConstants.InvalidCode);
                PluginsEventContext.Return(context);
            }
        }

        public void LoadOpenAdWith(string pluginName, Action success = null, Action<int> fail = null, Dictionary<string, object> extParams = null)
        {
            if (OpenAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowOpenAd)
                {
                    plugin.LoadOpenAd(success, fail, extParams);
                    return;
                }
            }

            fail?.Invoke(PluginConstants.InvalidCode);
        }

        public void ShowOpenAd(Action<bool, int> dismiss = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShowOpenAdWith((string) context.Data, dismiss);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_OPEN_AD_TYPE, context))
            {
                var plugin = GetReadyOpenAdPlugin();
                if (string.IsNullOrEmpty(plugin))
                    dismiss?.Invoke(false, PluginConstants.InvalidCode);
                else
                    ShowOpenAdWith(plugin, dismiss);
                PluginsEventContext.Return(context);
            }
        }

        public void ShowOpenAdWith(string pluginName, Action<bool, int> dismiss = null)
        {
            if (OpenAdPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowOpenAd)
                {
                    plugin.ShowOpenAd(dismiss);
                    return;
                }
            }

            dismiss?.Invoke(false, PluginConstants.InvalidCode);
        }
    }
}
