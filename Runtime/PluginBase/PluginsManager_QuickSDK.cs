using System;
using System.Collections.Generic;
using System.Linq;

#if ENABLE
namespace PluginSet.Core
{
    public sealed partial class PluginsManager : IQuickSDKPlugin
    {
        private Dictionary<string, IQuickSDKPlugin> _quickSDKPlugins;

        private Dictionary<string, IQuickSDKPlugin> QuickSDKPlugins
        {
            get
            {
                if (_quickSDKPlugins == null)
                {
                    _quickSDKPlugins = new Dictionary<string, IQuickSDKPlugin>();
                    foreach (var plugin in GetPlugins<IQuickSDKPlugin>())
                    {
                        _quickSDKPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _quickSDKPlugins;
            }
        }
        
        public string[] GetValidQuickSDKTypes()
        {
            return QuickSDKPlugins.Where(kv => kv.Value.IsQuickSDKEnable).Select(kv => kv.Key).ToArray();
        }
        
        public bool IsQuickSDKEnable => GetValidQuickSDKTypes().Length > 0;

        public void InitQuickSDK(string productCode, string productKey, Action onInitSuccessCallBack = null,
            Action<string, string> onInitFailedCallBack = null, Action<bool> onRequestPermissionCallBack = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                InitQuickSDKWithType((string) context.Data, productCode, productKey, onInitSuccessCallBack, onInitFailedCallBack, onRequestPermissionCallBack);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void CreateQuickSDK()
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                CreateQuickSDKWithType((string) context.Data);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void PayQuickSDK(string orderInfo, string gameRoleInfo, Action<string, string, string> onPaySuccessCallBack = null, Action<string> onPayCancelCallBack = null,
            Action<string, string, string> onPayFailedCallBack = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                PayQuickSDKWithType((string) context.Data, orderInfo, gameRoleInfo, onPaySuccessCallBack, onPayCancelCallBack, onPayFailedCallBack);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void LoginQuickSDK(Action<string> onLoginSuccessCallBack = null, Action<string, string> onLoginFailedCallBack = null,
            Action onLoginCancelCallBack = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                LoginQuickSDKWithType((string) context.Data, onLoginSuccessCallBack, onLoginFailedCallBack, onLoginCancelCallBack);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void LogoutQuickSDK(Action onLogoutSuccessCallBack = null, Action<string, string> onLogoutFailedCallBack = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                LogoutQuickSDKWithType((string) context.Data, onLogoutSuccessCallBack, onLogoutFailedCallBack);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void ExitQuickSDK(Action onExitSuccessCallBack = null, Action<string, string> onExitFailedCallBack = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ExitQuickSDKWithType((string) context.Data, onExitSuccessCallBack, onExitFailedCallBack);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void UploadGameRoleInfoQuickSDK(string gameRoleInfo, bool createRole)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                UploadGameRoleInfoQuickSDKWithType((string) context.Data, gameRoleInfo, createRole);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void SetSwitchAccountCallBackQuickSDK(Action<string> onSwitchAccountSuccessCallBack = null, Action<string, string> onSwitchAccountFailedCallBack = null,
            Action onSwitchAccountCancelCallBack = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                SetSwitchAccountCallBackQuickSDKWithType((string) context.Data, onSwitchAccountSuccessCallBack, onSwitchAccountFailedCallBack, onSwitchAccountCancelCallBack);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        

        public void SetLogoutCallBackQuickSDK(Action onLogoutSuccessCallBack = null, Action<string, string> onLogoutFailedCallBack = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                SetLogoutCallBackQuickSDKWithType((string) context.Data, onLogoutSuccessCallBack, onLogoutFailedCallBack);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void VerifyRealNameQuickSDK(Action<string> onVerifyRealNameSuccessCallBack = null, Action onVerifyRealNameFailedCallBack = null, Action<bool> onVerifyRealNamSupportCallBack = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                VerifyRealNameQuickSDKWithType((string) context.Data, onVerifyRealNameSuccessCallBack, onVerifyRealNameFailedCallBack, onVerifyRealNamSupportCallBack);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_QUICKSDK_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        //==========================================================================================
        public void InitQuickSDKWithType(string pluginName, string productCode, string productKey, Action onInitSuccessCallBack = null,
            Action<string, string> onInitFailedCallBack = null, Action<bool> onRequestPermissionCallBack = null)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.InitQuickSDK(productCode, productKey, onInitSuccessCallBack, onInitFailedCallBack, onRequestPermissionCallBack);
            }
        }

        public void CreateQuickSDKWithType(string pluginName)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.CreateQuickSDK();
            }
        }
        
        public void PayQuickSDKWithType(string pluginName, string orderInfo, string gameRoleInfo, Action<string, string, string> onPaySuccessCallBack = null, Action<string> onPayCancelCallBack = null,
            Action<string, string, string> onPayFailedCallBack = null)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.PayQuickSDK(orderInfo, gameRoleInfo, onPaySuccessCallBack, onPayCancelCallBack, onPayFailedCallBack);
            }
        }

        public void LoginQuickSDKWithType(string pluginName, Action<string> onLoginSuccessCallBack = null,
            Action<string, string> onLoginFailedCallBack = null,
            Action onLoginCancelCallBack = null)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.LoginQuickSDK(onLoginSuccessCallBack, onLoginFailedCallBack, onLoginCancelCallBack);
            }
        }
        
        public void LogoutQuickSDKWithType(string pluginName, Action onLogoutSuccessCallBack = null, Action<string, string> onLogoutFailedCallBack = null)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.LogoutQuickSDK(onLogoutSuccessCallBack, onLogoutFailedCallBack);
            }
        }

        public void ExitQuickSDKWithType(string pluginName, Action onExitSuccessCallBack = null, Action<string, string> onExitFailedCallBack = null)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ExitQuickSDK(onExitSuccessCallBack, onExitFailedCallBack);
            }
        }

        public void UploadGameRoleInfoQuickSDKWithType(string pluginName, string gameRoleInfo, bool createRole)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.UploadGameRoleInfoQuickSDK(gameRoleInfo, createRole);
            }
        }

        public void SetSwitchAccountCallBackQuickSDKWithType(string pluginName, Action<string> onSwitchAccountSuccessCallBack = null,
            Action<string, string> onSwitchAccountFailedCallBack = null,
            Action onSwitchAccountCancelCallBack = null)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.SetSwitchAccountCallBackQuickSDK(onSwitchAccountSuccessCallBack, onSwitchAccountFailedCallBack, onSwitchAccountCancelCallBack);
            }
        }

        public void SetLogoutCallBackQuickSDKWithType(string pluginName, Action onLogoutSuccessCallBack = null,
            Action<string, string> onLogoutFailedCallBack = null)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.SetLogoutCallBackQuickSDK(onLogoutSuccessCallBack, onLogoutFailedCallBack);
            }
        }

        public void VerifyRealNameQuickSDKWithType(string pluginName, Action<string> onVerifyRealNameSuccessCallBack = null,
            Action onVerifyRealNameFailedCallBack = null, Action<bool> onVerifyRealNamSupportCallBack = null)
        {
            if (QuickSDKPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.VerifyRealNameQuickSDK(onVerifyRealNameSuccessCallBack, onVerifyRealNameFailedCallBack, onVerifyRealNamSupportCallBack);
            }
        }
    }
}
#endif