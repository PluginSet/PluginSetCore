using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor.Devices
{
    [InitializeOnLoad]
    public static class DevicesPluginFilters
    {
        private static bool FilterPushModule(string fileName, BuildProcessorContext context)
        {
            if (context.BuildChannels.IncludePushModule)
                return false;

            Debug.Log("Filter Devices push plugin: excluded " + fileName);
            return true;
        }

        static DevicesPluginFilters()
        {
            PluginFilter.RegisterFilter("com.pluginset.core/Plugins/Android/DevicesNotification", FilterPushModule);
            PluginFilter.RegisterFilter("com.pluginset.core/Plugins/Android/DevicesLocalNotificationReceiver", FilterPushModule);
            PluginFilter.RegisterFilter("com.pluginset.core/Plugins/iOS/DevicesNotification", FilterPushModule);
        }
    }
}
