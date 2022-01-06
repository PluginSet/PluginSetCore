using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ENABLE
namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IDeviceInfoPlugin
    {
        private Dictionary<string, IDeviceInfoPlugin> _deviceInfoPlugins;

        private Dictionary<string, IDeviceInfoPlugin> DeviceInfoPlugins
        {
            get
            {
                if (_deviceInfoPlugins == null)
                {
                    _deviceInfoPlugins = new Dictionary<string, IDeviceInfoPlugin>();
                    foreach (var plugin in GetPlugins<IDeviceInfoPlugin>())
                    {
                        _deviceInfoPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _deviceInfoPlugins;
            }
        }
        
        public string[] GetDeviceInfoTypes()
        {
            return DeviceInfoPlugins.Keys.ToArray();
        }

        public string[] GetValidDeviceInfoTypes()
        {
            return DeviceInfoPlugins.Where(kv => kv.Value.IsEnableDeviceInfo).Select(kv => kv.Key).ToArray();
        }

        public bool IsEnableDeviceInfo => true;

        public void GetUDID(Action<string> callback = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetUDIDWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEVICEINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        
        public void GetUDIDWithType(string pluginName, Action<string> callback = null)
        {
            if (DeviceInfoPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetUDID(callback);
            }
        }

        
        
        public void GetModel(Action<string> callback = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetModelWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEVICEINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        
        public void GetModelWithType(string pluginName, Action<string> callback = null)
        {
            if (DeviceInfoPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetModel(callback);
            }
        }

        
        
        public void GetVersion(Action<string> callback = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetVersionWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEVICEINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        
        public void GetVersionWithType(string pluginName, Action<string> callback = null)
        {
            if (DeviceInfoPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetVersion(callback);
            }
        }
        
        

        public void IdfaPermissionState(Action<int> callback = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                IdfaPermissionStateWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEVICEINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        
        public void IdfaPermissionStateWithType(string pluginName, Action<int> callback = null)
        {
            if (DeviceInfoPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.IdfaPermissionState(callback);
            }
        }
        

        public void IsIdfaOn(Action<bool> callback = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                IsIdfaOnWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEVICEINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void IsIdfaOnWithType(string pluginName, Action<bool> callback = null)
        {
            if (DeviceInfoPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.IsIdfaOn(callback);
            }
        }
        
        
        public void GetIdfa(Action<string> callback = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetIdfaWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEVICEINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        
        public void GetIdfaWithType(string pluginName, Action<string> callback = null)
        {
            if (DeviceInfoPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetIdfa(callback);
            }
        }
        
        

        public void ReqIdfaPermission(Action callback = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ReqIdfaPermissionWithType((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEVICEINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }
        
        public void ReqIdfaPermissionWithType(string pluginName, Action callback = null)
        {
            if (DeviceInfoPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ReqIdfaPermission(callback);
            }
        }
    }
}
#endif