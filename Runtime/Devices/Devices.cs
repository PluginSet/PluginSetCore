using System;
using UnityEngine;

namespace PluginSet.Core
{
    
    public static class Devices
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("Devices");
        
        /// <summary>
        /// return meta data with name on Android platform
        /// return empty string on Editor
        /// </summary>
        /// <param name="name">key name</param>
        /// <param name="defaultValue">default return value</param>
        /// <returns></returns>
        public static string GetAppData(string name, string defaultValue = null)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidHelper.GetApplicationMetaString(name, defaultValue);
#elif UNITY_IOS && !UNITY_EDITOR
            return iOSHelper._ReadInfoPlist(name, defaultValue);
#else
            return defaultValue;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level">Apple 13 [0-4], Apple 10 [0-2]</param>
        public static void Vibrate(int level)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            long duration = (level) * 200L + 100L;
            AndroidHelper.Vibrate(duration);
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHelper._Vibrate(level);
#else
            Handheld.Vibrate();
#endif
        }

        /// <summary>
        ///  GetSafeArea
        /// </summary>
        /// <returns>safeArea in current device</returns>
        public static Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        public static bool IsAdvertisingTrackingGranted()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidHelper.IsPermissionGranted(AndroidHelper.PERMISSION_READ_PHONE_STATE);
#elif UNITY_IOS && !UNITY_EDITOR
            return iOSHelper._IsAdvertisingTrackingGranted();
#else
            return true;
#endif
        }

        public static bool RequestAdvertisingTracking()
        {
            if (!IsAdvertisingTrackingGranted())
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return AndroidHelper.RequestPermissions(AndroidHelper.PERMISSION_READ_PHONE_STATE);
#elif UNITY_IOS && !UNITY_EDITOR
                return iOSHelper._RequestAdvertisingTracking();
#endif
            }

            return true;
        }
        

        public static string GetDeviceUniqueId()
        {
            if (!RequestAdvertisingTracking())
                Logger.Warn("GetDeviceUniqueId has not permission to get device id");
            
#if UNITY_ANDROID && !UNITY_EDITOR
            var imei = AndroidHelper.GetIMEI();
            if (!string.IsNullOrEmpty(imei))
                return imei;
            return SystemInfo.deviceUniqueIdentifier;
#elif UNITY_IOS && !UNITY_EDITOR
            var idfa = UnityEngine.iOS.Device.advertisingIdentifier;
            if (idfa.Equals("00000000-0000-0000-0000-000000000000") || string.IsNullOrEmpty(idfa))
            {
                idfa = SystemInfo.deviceUniqueIdentifier;
                string services = iOSHelper._ReadInfoPlist("KeyChainServices", "com.pluginset.core");
                if (string.IsNullOrEmpty(services))
                    services = "com.pluginset.core";
                idfa = iOSHelper._GetOrSaveKeyChain(services, "SystemInfo.deviceUniqueIdentifier", idfa);
            }

            return idfa;
#else
            return SystemInfo.deviceUniqueIdentifier;
#endif
        }

        public static void SetClipboardText(string text)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidHelper.SetClipboardText(text);
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHelper._CopyTextToClipboard(text);
#else
            GUIUtility.systemCopyBuffer = text;
#endif
        }

        public static string GetClipboardText()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidHelper.GetClipboardText();
#elif UNITY_IOS && !UNITY_EDITOR
            return iOSHelper._ReadTextToClipboard();
#else
            return GUIUtility.systemCopyBuffer;
#endif
        }

        public static int GetBatteryLevel()
        {
            return (int) SystemInfo.batteryLevel;
        }

        public static bool IsBatteryCharging()
        {
            return SystemInfo.batteryStatus == BatteryStatus.Charging;
        }

        public static int GetVersionCode()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return (int)AndroidHelper.GetVersionCode();
#elif UNITY_ANDROID && UNITY_EDITOR
            return UnityEditor.PlayerSettings.Android.bundleVersionCode;
#elif UNITY_IOS && !UNITY_EDITOR
            return int.Parse(iOSHelper._GetVersionCode());
#elif UNITY_IOS && UNITY_EDITOR
            return int.Parse(UnityEditor.PlayerSettings.iOS.buildNumber);
#else
            return 0;
#endif
        }

        public static void LogI(string msg)
        {
#if !UNITY_EDITOR
#if UNITY_IOS
            iOSHelper._PlatformLog(msg);
#elif UNITY_ANDROID
            AndroidHelper.Log("PluginSet", msg);
#endif
#endif
        }
    }
}
