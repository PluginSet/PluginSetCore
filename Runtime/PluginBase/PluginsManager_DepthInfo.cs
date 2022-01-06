using System;
using System.Collections.Generic;
using System.Linq;

#if ENABLE
namespace PluginSet.Core
{
    public sealed partial class PluginsManager : IDepthInfoPlugin
    {
        private Dictionary<string, IDepthInfoPlugin> _depthInfoInstallPlugins;

        private Dictionary<string, IDepthInfoPlugin> DepthInfoInstallPlugins
        {
            get
            {
                if (_depthInfoInstallPlugins == null)
                {
                    _depthInfoInstallPlugins = new Dictionary<string, IDepthInfoPlugin>();
                    foreach (var plugin in GetPlugins<IDepthInfoPlugin>())
                    {
                        _depthInfoInstallPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _depthInfoInstallPlugins;
            }
        }
        
        public string[] GetValidDepthInfoTypes()
        {
            return DepthInfoInstallPlugins.Where(kv => kv.Value.IsDepthInfoEnable).Select(kv => kv.Key).ToArray();
        }
        
        public bool IsDepthInfoEnable => GetValidDepthInfoTypes().Length > 0;
        
        public void IsEmulator(Action<int> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                IsEmulatorWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetDeviceInfomation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetDeviceInfomationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetDisplayInformation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetDisplayInformationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetAllAppsPackageName(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetAllAppsPackageNameWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetAppInfoByPackageName(string packageName, Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetAppInfoByPackageNameWithType((string) context.Data, packageName, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetBatteryInformation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetBatteryInformationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetCameraInformation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetCameraInformationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetCPUInformation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetCPUInformationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetLocationInformation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetLocationInformationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetMemoryInformation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetMemoryInformationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetNetworkInformation(bool useIPv4, Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetNetworkInformationWithType((string) context.Data, useIPv4, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetSensorInformation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetSensorInformationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetSystemInformation(Action<string> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetSystemInformationWithType((string) context.Data, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void TriggerBrightnessMode()
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                TriggerBrightnessModeWithType((string) context.Data);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void GetPermission(string permissionName, Action<int> onResult)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                GetPermissionWithType((string) context.Data, permissionName, onResult);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        public void RequestPermissions(string permission, Action<string> onGranted = null, Action<string> onDenied = null,
            Action<string> onDeniedAlways = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                RequestPermissionsWithType((string) context.Data, permission, onGranted, onDenied, onDeniedAlways);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_DEPTHINFO_TYPE, context))
            {
                PluginsEventContext.Return(context);
            }
        }

        //==========================================================================================
        public void IsEmulatorWithType(string pluginName, Action<int> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.IsEmulator(onResult);
            }
        }

        public void GetDeviceInfomationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetDeviceInfomation(onResult);
            }
        }

        public void GetDisplayInformationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetDisplayInformation(onResult);
            }
        }

        public void GetAllAppsPackageNameWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetAllAppsPackageName(onResult);
            }
        }

        public void GetAppInfoByPackageNameWithType(string pluginName, string packageName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetAppInfoByPackageName(packageName, onResult);
            }
        }

        public void GetBatteryInformationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetBatteryInformation(onResult);
            }
        }

        public void GetCameraInformationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetCameraInformation(onResult);
            }
        }

        public void GetCPUInformationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetCPUInformation(onResult);
            }
        }

        public void GetLocationInformationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetLocationInformation(onResult);
            }
        }

        public void GetMemoryInformationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetMemoryInformation(onResult);
            }
        }

        public void GetNetworkInformationWithType(string pluginName, bool useIPv4, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetNetworkInformation(useIPv4, onResult);
            }
        }

        public void GetSensorInformationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetSensorInformation(onResult);
            }
        }

        public void GetSystemInformationWithType(string pluginName, Action<string> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetSystemInformation(onResult);
            }
        }

        public void TriggerBrightnessModeWithType(string pluginName)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.TriggerBrightnessMode();
            }
        }

        public void GetPermissionWithType(string pluginName, string permissionName, Action<int> onResult)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetPermission(permissionName, onResult);
            }
        }

        public void RequestPermissionsWithType(string pluginName, string permission, Action<string> onGranted = null, Action<string> onDenied = null,
            Action<string> onDeniedAlways = null)
        {
            if (DepthInfoInstallPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.RequestPermissions(permission, onGranted, onDenied, onDeniedAlways);
            }
        }
    }
}
#endif