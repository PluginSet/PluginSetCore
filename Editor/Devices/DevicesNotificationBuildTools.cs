using PluginSet.Core.Editor;
using UnityEditor;

namespace PluginSet.Core.Editor.Devices
{
    [BuildTools]
    public static class DevicesNotificationBuildTools
    {
        /// <summary>
        /// 通知模块（本地 + 系统通知栏）所需权限；远程推送额外权限由 Firebase/Apple 构建项追加。
        /// </summary>
        [AndroidProjectModify]
        public static void OnAndroidNotificationModuleModify(BuildProcessorContext context, AndroidProjectManager projectManager)
        {
            if (context.BuildTarget != BuildTarget.Android)
                return;

            if (!context.BuildChannels.IncludePushModule)
                return;

            var manifest = projectManager.LibraryManifest;
            manifest.AddUsePermission("android.permission.POST_NOTIFICATIONS", "PluginSetNotification");
            manifest.AddUsePermission("android.permission.SCHEDULE_EXACT_ALARM", "PluginSetNotification");
            manifest.AddUsePermission("android.permission.WAKE_LOCK", "PluginSetNotification");

            projectManager.AddApplicationPlugin(
                "DevicesNotification",
                "com.pluginset.devices.DevicesNotificationApplicationPlugin");
            projectManager.AddActivityPlugin(
                "DevicesNotification",
                "com.pluginset.devices.DevicesNotificationActivityPlugin");

            const string receiverPath = "/manifest/application/receiver";
            var receiver = manifest.createElementWithPath(receiverPath);
            receiver.SetAttribute("name", AndroidConst.NS_URI, "com.pluginset.devices.DevicesLocalNotificationReceiver");
            receiver.SetAttribute("exported", AndroidConst.NS_URI, "false");
        }
    }
}
