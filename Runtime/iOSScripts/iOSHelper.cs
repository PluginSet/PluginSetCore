#if UNITY_IOS

using System.Runtime.InteropServices;

namespace PluginSet.Core
{
    public static class iOSHelper
    {
        [DllImport("__Internal")]
        public static extern void _PluginSetPlatformLog(string msg);

        [DllImport("__Internal")]
        public static extern void _PluginSetCopyTextToClipboard(string text);

        [DllImport("__Internal")]
        public static extern string _PluginSetReadTextToClipboard();

        [DllImport("__Internal")]
        public static extern void _PluginSetVibrate(int level);

        [DllImport("__Internal")]
        public static extern string _PluginSetReadInfoPlist(string name, string defaultValue);

        [DllImport("__Internal")]
        public static extern string _PluginSetGetVersionCode();

        [DllImport("__Internal")]
        public static extern string _PluginSetGetOrSaveKeyChain(string services, string key, string value);

        [DllImport("__Internal")]
        public static extern void _PluginSetSaveKeyChain(string services, string key, string value);

        [DllImport("__Internal")]
        public static extern void _PluginSetDeleteAllKeyChainValue(string services);

        [DllImport("__Internal")]
        public static extern void _PluginSetDeleteKeyChain(string services, string key);

        [DllImport("__Internal")]
        public static extern int _PluginSetCheckAdvertisingTrackingPermission();

        [DllImport("__Internal")]
        public static extern bool _PluginSetRequestAdvertisingTracking();

        [DllImport("__Internal")]
        public static extern int _PluginSetCheckMicrophonePermission();

        [DllImport("__Internal")]
        public static extern bool _PluginSetRequestMicrophoneAuth();

        [DllImport("__Internal")]
        public static extern bool _PluginSetEnableOpenSettings();

        [DllImport("__Internal")]
        public static extern void _PluginSetOpenSettings();

        [DllImport("__Internal")]
        public static extern bool _PluginSetOSAvailable(int version);

#if ENABLE_PUSH_MODULE
        [DllImport("__Internal")]
        public static extern void _PluginSetNotificationStorePendingOpenJson(string json);

        [DllImport("__Internal")]
        public static extern bool _PluginSetNotificationRequestPermission();

        [DllImport("__Internal")]
        public static extern void _PluginSetNotificationSetBadgeNumber(int count);

        [DllImport("__Internal")]
        public static extern int _PluginSetNotificationGetBadgeNumber();

        [DllImport("__Internal")]
        public static extern void _PluginSetNotificationRemoveAllDelivered();

        [DllImport("__Internal")]
        public static extern System.IntPtr _PluginSetNotificationConsumeInitialJson();

        [DllImport("__Internal")]
        public static extern void _PluginSetNotificationFreeCString(System.IntPtr value);

        [DllImport("__Internal")]
        public static extern void _PluginSetNotificationPostLocal(
            string identifier,
            string channelId,
            string title,
            string body,
            int delaySeconds,
            string dataJson,
            string actionId);

        [DllImport("__Internal")]
        public static extern void _PluginSetNotificationCancelLocal(string identifier);

        [DllImport("__Internal")]
        public static extern void _PluginSetNotificationCancelAllPendingLocal();
#endif
    }
}
#endif