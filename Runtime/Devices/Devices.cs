using System;
using UnityEngine;

namespace PluginSet.Core
{
    public enum DevicePermission
    {
        Denied = 0,
        Granted = 1,
        ShouldAsk = 2,
    }
    
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

        private static bool RecordRequestPermissions(string key)
        {
            var record = PlayerPrefs.GetInt(key, 0);
            if (record == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }
            return record == 1;
        }
        
        public static DevicePermission CheckAdvertisingTrackingPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return (DevicePermission)AndroidHelper.CheckPermission(AndroidHelper.PERMISSION_READ_PHONE_STATE);
#elif UNITY_IOS && !UNITY_EDITOR
            return (DevicePermission)iOSHelper._CheckAdvertisingTrackingPermission();
#else
            return DevicePermission.Granted;
#endif
        }

        public static bool IsAdvertisingTrackingGranted()
        {
            return CheckAdvertisingTrackingPermission() == DevicePermission.Granted;
        }

        public static bool RequestAdvertisingTracking()
        {
            var permission = CheckAdvertisingTrackingPermission();
            if (permission == DevicePermission.ShouldAsk ||
                (permission == DevicePermission.Denied && !RecordRequestPermissions(nameof(RequestAdvertisingTracking))))
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return AndroidHelper.RequestPermissions(AndroidHelper.PERMISSION_READ_PHONE_STATE);
#elif UNITY_IOS && !UNITY_EDITOR
                return iOSHelper._RequestAdvertisingTracking();
#endif
            }

            return permission == DevicePermission.Granted;
        }
        
        public static DevicePermission CheckMicrophonePermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return (DevicePermission)AndroidHelper.CheckPermission(AndroidHelper.PERMISSION_RECORD_AUDIO);
#elif UNITY_IOS && !UNITY_EDITOR
            return (DevicePermission)iOSHelper._CheckMicrophonePermission();
#else
            return DevicePermission.Granted;
#endif
        }

        public static bool IsMicrophoneEnable()
        {
            return CheckMicrophonePermission() == DevicePermission.Granted;
        }

        public static bool RequestMicrophoneAuth()
        {
            var permission = CheckMicrophonePermission();
            if (permission == DevicePermission.ShouldAsk ||
                (permission == DevicePermission.Denied && !RecordRequestPermissions(nameof(RequestMicrophoneAuth))))
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return AndroidHelper.RequestPermissions(AndroidHelper.PERMISSION_RECORD_AUDIO);
#elif UNITY_IOS && !UNITY_EDITOR
                return iOSHelper._RequestMicrophoneAuth();
#endif
            }

            return permission == DevicePermission.Granted;
        }

        public static string GetDeviceUniqueId()
        {
            var deviceId = string.Empty;
            if (RequestAdvertisingTracking())
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                deviceId = AndroidHelper.GetIMEI();
#elif UNITY_IOS && !UNITY_EDITOR
                deviceId = UnityEngine.iOS.Device.advertisingIdentifier;
                if (deviceId.Equals("00000000-0000-0000-0000-000000000000"))
                    deviceId = string.Empty;
#endif
            }
            else
            {
                Logger.Warn("GetDeviceUniqueId has not permission to get device id");
            }

            if (string.IsNullOrEmpty(deviceId))
                deviceId = SystemInfo.deviceUniqueIdentifier;
                
#if UNITY_IOS && !UNITY_EDITOR
            string services = iOSHelper._ReadInfoPlist("KeyChainServices", "com.pluginset.core");
            if (string.IsNullOrEmpty(services))
                services = "com.pluginset.core";
            deviceId = iOSHelper._GetOrSaveKeyChain(services, "SystemInfo.deviceUniqueIdentifier", deviceId);
#endif
            return deviceId;
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
            return (int) (SystemInfo.batteryLevel * 100);
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
        
        public static bool EnableOpenSettings()
        {
#if !UNITY_EDITOR && UNITY_IOS
            return iOSHelper._EnableOpenSettings();
#else
            return true;
#endif
        }

        public static void OpenSettings()
        {
#if !UNITY_EDITOR && UNITY_IOS
            iOSHelper._OpenSettings();
#elif !UNITY_EDITOR && UNITY_ANDROID
            AndroidHelper.OpenSettings();
#endif
        }

        public static bool OSAvailable(int version)
        {
#if !UNITY_EDITOR && UNITY_IOS
            return iOSHelper._OSAvailable(version);
#elif !UNITY_EDITOR && UNITY_ANDROID
            return AndroidHelper.OSAvailable(version);
#else
            return true;
#endif
        }
    }
}
