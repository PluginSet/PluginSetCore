using System;
using System.Runtime.InteropServices;
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
        
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void syncfs();
        [DllImport("__Internal")]
        private static extern void requestAudio();
#endif
        
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
#elif UNITY_WEBGL
            // do nothing
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

        private static bool RecordRequestPermissions(string key, bool doRecord = true)
        {
            var record = PlayerPrefs.GetInt(key, 0);
            if (doRecord && record == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }
            return record == 1;
        }

        private static DevicePermission CheckAdvertisingTrackingPermissionInternal(bool doRecord)
        {
            var permission = DevicePermission.Granted;
#if UNITY_ANDROID && !UNITY_EDITOR
            permission = (DevicePermission)AndroidHelper.CheckPermission(AndroidHelper.PERMISSION_READ_PHONE_STATE);
#elif UNITY_IOS && !UNITY_EDITOR
            permission = (DevicePermission)iOSHelper._CheckAdvertisingTrackingPermission();
#endif
            if (permission == DevicePermission.Denied && !RecordRequestPermissions(nameof(RequestAdvertisingTracking), doRecord))
            {
                permission = DevicePermission.ShouldAsk;
            }

            return permission;
        }
        
        public static DevicePermission CheckAdvertisingTrackingPermission()
        {
            return CheckAdvertisingTrackingPermissionInternal(false);
        }

        public static bool IsAdvertisingTrackingGranted()
        {
            return CheckAdvertisingTrackingPermission() == DevicePermission.Granted;
        }

        public static bool RequestAdvertisingTracking()
        {
            var permission = CheckAdvertisingTrackingPermissionInternal(true);
            if (permission == DevicePermission.ShouldAsk)
            {
                RecordRequestPermissions(nameof(RequestAdvertisingTracking), true);
#if UNITY_ANDROID && !UNITY_EDITOR
                return AndroidHelper.RequestPermissions(AndroidHelper.PERMISSION_READ_PHONE_STATE);
#elif UNITY_IOS && !UNITY_EDITOR
                return iOSHelper._RequestAdvertisingTracking();
#endif
            }

            return permission == DevicePermission.Granted;
        }

        private static DevicePermission CheckMicrophonePermissionInternal(bool doRecord)
        {
            var permission = DevicePermission.Granted;
#if UNITY_ANDROID && !UNITY_EDITOR
            return (DevicePermission)AndroidHelper.CheckPermission(AndroidHelper.PERMISSION_RECORD_AUDIO);
#elif UNITY_IOS && !UNITY_EDITOR
            return (DevicePermission)iOSHelper._CheckMicrophonePermission();
#endif
            if (permission == DevicePermission.Denied &&
                !RecordRequestPermissions(nameof(RequestMicrophoneAuth), doRecord))
            {
                permission = DevicePermission.ShouldAsk;
            }

            return permission;
        }
        
        public static DevicePermission CheckMicrophonePermission()
        {
            return CheckMicrophonePermissionInternal(false);
        }

        public static bool IsMicrophoneEnable()
        {
            return CheckMicrophonePermission() == DevicePermission.Granted;
        }

        public static bool RequestMicrophoneAuth()
        {
            var permission = CheckMicrophonePermissionInternal(true);
            if (permission == DevicePermission.ShouldAsk)
            {
                RecordRequestPermissions(nameof(RequestMicrophoneAuth), true);
#if UNITY_ANDROID && !UNITY_EDITOR
                return AndroidHelper.RequestPermissions(AndroidHelper.PERMISSION_RECORD_AUDIO);
#elif UNITY_IOS && !UNITY_EDITOR
                return iOSHelper._RequestMicrophoneAuth();
#endif
            }

            return permission == DevicePermission.Granted;
        }

        public static string GetUnityUniqueId()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
#if UNITY_IOS && !UNITY_EDITOR
            string services = iOSHelper._ReadInfoPlist("KeyChainServices", "com.pluginset.core");
            if (string.IsNullOrEmpty(services))
                services = "com.pluginset.core";
            deviceId = iOSHelper._GetOrSaveKeyChain(services, "SystemInfo.deviceUniqueIdentifier", deviceId);
#endif
            return deviceId;
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
                deviceId = GetUnityUniqueId();
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
#elif UNITY_ANDROID
            return true;
#else
            return false;
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

        public static void RequestAudio()
        {
#if UNITY_WEBGL
            requestAudio();
#endif
        }

        public static void SyncFileSystem()
        {
#if UNITY_WEBGL
            syncfs();
#endif
        }
    }
}
