#if UNITY_IOS

using System.Runtime.InteropServices;

namespace PluginSet.Core
{
    public static class iOSHelper
    {
        [DllImport("__Internal")]
        public static extern void _PlatformLog(string msg);

        [DllImport("__Internal")]
        public static extern void _CopyTextToClipboard(string text);

        [DllImport("__Internal")]
        public static extern string _ReadTextToClipboard();

        [DllImport("__Internal")]
        public static extern void _Vibrate(int level);

        [DllImport("__Internal")]
        public static extern string _ReadInfoPlist(string name, string defaultValue);

        [DllImport("__Internal")]
        public static extern string _GetVersionCode();

        [DllImport("__Internal")]
        public static extern string _GetOrSaveKeyChain(string services, string key, string value);

        [DllImport("__Internal")]
        public static extern void _SaveKeyChain(string services, string key, string value);

        [DllImport("__Internal")]
        public static extern void _DeleteAllKeyChainValue(string services);

        [DllImport("__Internal")]
        public static extern void _DeleteKeyChain(string services, string key);

        [DllImport("__Internal")]
        public static extern int _CheckAdvertisingTrackingPermission();

        [DllImport("__Internal")]
        public static extern bool _RequestAdvertisingTracking();

        [DllImport("__Internal")]
        public static extern int _CheckMicrophonePermission();

        [DllImport("__Internal")]
        public static extern bool _RequestMicrophoneAuth();

        [DllImport("__Internal")]
        public static extern bool _EnableOpenSettings();

        [DllImport("__Internal")]
        public static extern void _OpenSettings();

        [DllImport("__Internal")]
        public static extern bool _OSAvailable(int version);
    }
}
#endif