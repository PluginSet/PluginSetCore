using System;
using System.Collections.Generic;
using System.Linq;

#if ENABLE
namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IYiDunPlugin
    {
        private Dictionary<string, IYiDunPlugin> _yiDunInstallPlugins;

        private Dictionary<string, IYiDunPlugin> YiDunInstallPlugins
        {
            get
            {
                if (_yiDunInstallPlugins == null)
                {
                    _yiDunInstallPlugins = new Dictionary<string, IYiDunPlugin>();
                    foreach (var plugin in GetPlugins<IYiDunPlugin>())
                    {
                        _yiDunInstallPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _yiDunInstallPlugins;
            }
        }
        
        public string[] GetValidYiDunTypes()
        {
            return YiDunInstallPlugins.Where(kv => kv.Value.IsYiDunEnable).Select(kv => kv.Key).ToArray();
        }

        public bool IsYiDunEnable => GetValidYiDunTypes().Length > 0;

        public void SetConfigData(string channel, string trackid)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                SetConfigDataWithType((string) context.Data, channel, trackid);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_YIDUN_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void AddExtraData(string key, string value)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                AddExtraDataWithType((string) context.Data, key, value);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_YIDUN_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void InitWatchMan(string productNumber, Action<int, string> onInit)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                InitWatchManWithType((string) context.Data, productNumber, onInit);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_YIDUN_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetToken(Action<int, string, string> onGetToken)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetTokenWithType((string) context.Data, onGetToken);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_YIDUN_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        
        
        public void SetConfigDataWithType(string pluginName, string channel, string trackid)
        {
            if (YiDunInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.SetConfigData(channel, trackid);
            }
        }
        
        public void AddExtraDataWithType(string pluginName, string key, string value)
        {
            if (YiDunInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.AddExtraData(key, value);
            }
        }
        
        public void InitWatchManWithType(string pluginName, string productNumber, Action<int, string> onInit)
        {
            if (YiDunInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.InitWatchMan(productNumber, onInit);
            }
        }

        public void GetTokenWithType(string pluginName, Action<int, string, string> onGetToken)
        {
            if (YiDunInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetToken(onGetToken);
            }
        }
    }
}
#endif