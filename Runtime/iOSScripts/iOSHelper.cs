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
        public static extern bool _IsAdvertisingTrackingGranted();

        [DllImport("__Internal")]
        public static extern bool _RequestAdvertisingTracking();

        [DllImport("__Internal")]
        public static extern bool _IsMicrophoneGranted();
        [DllImport("__Internal")]
        public static extern bool _RequestMicrophoneAuth();
    }
}
#endif