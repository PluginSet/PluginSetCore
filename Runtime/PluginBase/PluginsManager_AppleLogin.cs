using System;
using System.Collections.Generic;
using System.Linq;

#if ENABLE
namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IAppleLoginPlugin
    {
        private Dictionary<string, IAppleLoginPlugin> _appleLoginPlugins;

        private Dictionary<string, IAppleLoginPlugin> AppleLoginPlugins
        {
            get
            {
                if (_appleLoginPlugins == null)
                {
                    _appleLoginPlugins = new Dictionary<string, IAppleLoginPlugin>();
                    foreach (var plugin in GetPlugins<IAppleLoginPlugin>())
                    {
                        _appleLoginPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _appleLoginPlugins;
            }
        }

        public bool IsEnableAppleLogin => GetValidLoginTypes().Length > 0;

        public string[] GetAppleLoginTypes()
        {
            return AppleLoginPlugins.Keys.ToArray();
        }

        public string[] GetValidAppleLoginTypes()
        {
            return AppleLoginPlugins.Where(kv => kv.Value.IsEnableAppleLogin).Select(kv => kv.Key).ToArray();
        }


        public void DoAppleLogin(Action<AppleLoginResult> callback = null, string info = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                DoAppleLoginWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_LOGIN_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void DoAppleLoginWithType(string pluginName, Action<AppleLoginResult> callback = null)
        {
            if (AppleLoginPlugins.TryGetValue(pluginName, out var plugin))
            {
                    plugin.DoAppleLogin(callback);
                    return;
            }
            
            callback?.Invoke(new AppleLoginResult
            {
                Success = false,
                ErrorCode = 0,
                JsonInfo = "DoAppleLoginWithType",
            });
        }
    }
}
#endif