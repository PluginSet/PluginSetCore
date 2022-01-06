using System;
using System.Collections.Generic;
using System.Linq;

#if ENABLE
namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IOpenInstallPlugin
    {
        private Dictionary<string, IOpenInstallPlugin> _openInstallPlugins;

        private Dictionary<string, IOpenInstallPlugin> OpenInstallPlugins
        {
            get
            {
                if (_openInstallPlugins == null)
                {
                    _openInstallPlugins = new Dictionary<string, IOpenInstallPlugin>();
                    foreach (var plugin in GetPlugins<IOpenInstallPlugin>())
                    {
                        _openInstallPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _openInstallPlugins;
            }
        }

        public bool IsEnableOpenInstall => GetValidLoginTypes().Length > 0;
        
        public string[] GetOpenInstallTypes()
        {
            return OpenInstallPlugins.Keys.ToArray();
        }

        public string[] GetValidOpenInstallTypes()
        {
            return OpenInstallPlugins.Where(kv => kv.Value.IsEnableOpenInstall).Select(kv => kv.Key).ToArray();
        }

        public void GetInstallParms(Action<OpenInstallResult> callback = null, string info = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetInstallParmsWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_LOGIN_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetInstallParmsWithType(string pluginName, Action<OpenInstallResult> callback = null)
        {
            if (OpenInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                // if (plugin.IsEnableOpenInstall)
                // {
                    plugin.GetInstallParms(callback);
                    return;
                // }
            }
            
            callback?.Invoke(new OpenInstallResult
            {
                Success = false,
                ErrorCode = 0,
                PluginName = pluginName,
            });
        }

        public void OpenInstallReportRegister()
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                OpenInstallReportRegisterWithType((string) context.Data);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_LOGIN_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void OpenInstallReportRegisterWithType(string pluginName)
        {
            if (OpenInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.OpenInstallReportRegister();
                return;
            }
        }
    }
}
#endif