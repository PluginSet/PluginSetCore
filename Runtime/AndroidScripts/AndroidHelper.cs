#if UNITY_ANDROID

using System;
using UnityEngine;
namespace PluginSet.Core
{
    public static class AndroidHelper
    {
        public const string PERMISSION_RECORD_AUDIO = "android.permission.RECORD_AUDIO";
        public const string PERMISSION_READ_PHONE_STATE = "android.permission.READ_PHONE_STATE";
        
        private static AndroidJavaClass _utils;

        private static AndroidJavaClass Utils
        {
            get
            {
                if (_utils == null)
                    _utils = new AndroidJavaClass("com.pluginset.devices.Utils");
                
                return _utils;
            }
        }
        

        private static AndroidJavaObject _currentActivity;
        
        public static AndroidJavaObject CurrentActivity
        {
            get
            {
                if (_currentActivity == null)
                    _currentActivity = AndroidClasses.UnityPlayer?.GetStatic<AndroidJavaObject>("currentActivity");
                
                return _currentActivity;
            }
        }
        

        private static int _GET_META_DATA = -1;

        public static int GET_META_DATA
        {
            get
            {
                if (_GET_META_DATA < 0 && AndroidClasses.PackageManager != null)
                    _GET_META_DATA = AndroidClasses.PackageManager.GetStatic<int>("GET_META_DATA");

                return _GET_META_DATA;
            }
        }

        private static AndroidJavaObject _applicationInfo;

        public static AndroidJavaObject ApplicationInfo
        {
            get
            {
                if (_applicationInfo == null)
                    _applicationInfo = GetApplicationInfo();

                return _applicationInfo;
            }
        }

        public static string GetPackageName()
        {
            return CurrentActivity?.Call<string>("getPackageName");
        }

        public static AndroidJavaObject GetApplicationInfo()
        {
            AndroidJavaObject pkgMgr = CurrentActivity?.Call<AndroidJavaObject>("getPackageManager");
            return pkgMgr?.Call<AndroidJavaObject>("getApplicationInfo", GetPackageName(), GET_META_DATA);
        }
        
        public static string GetApplicationMetaData(string name, string defaultValue = null)
        {
            try
            {
                var metaData = ApplicationInfo?.Get<AndroidJavaObject>("metaData");
                var result = metaData?.Call<string>("getString", name, defaultValue);
                return result?.TrimStart();
            }
            catch (AndroidJavaException e)
            {
                Console.WriteLine(e);
                return defaultValue;
            }
        }

        public static string GetSystemProperty(string key, string defaultValue = null)
        {
            var sysprop = AndroidClasses.SystemProperties;
            if (sysprop == null)
                return defaultValue;
            
            try
            {
                return sysprop.CallStatic<string>("get", key, defaultValue);
            }
            catch (AndroidJavaException e)
            {
                Console.WriteLine(e);
                return defaultValue;
            }
        }

        public static void Toast(string message, int duration)
        {
            CurrentActivity?.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toast = AndroidClasses.Toast?.CallStatic<AndroidJavaObject>("makeText", CurrentActivity, message, duration);
                toast?.Call("show");
            })); 
        }

        public static void Log(string tag, string message)
        {
            AndroidClasses.Log?.CallStatic<int>("i", tag, message);
        }

        public static void Quit(int code = 0)
        {
            AndroidClasses.System?.CallStatic("exit", code);
        }

        public static void SetClipboardText(string text)
        {
            Utils.CallStatic("CopyToClipboard", CurrentActivity, text);
        }

        public static string GetClipboardText()
        {
            return Utils.CallStatic<string>("ReadFromClipboard", CurrentActivity);
        }

        public static string GetApplicationMetaString(string name, string defaultValue = null)
        {
            return Utils.CallStatic<string>("GetApplicationMetaString", CurrentActivity, name, defaultValue);
        }

        public static int GetApplicationMetaInt(string name, int defaultValue = 0)
        {
            return Utils.CallStatic<int>("GetApplicationMetaInt", CurrentActivity, name, defaultValue);
        }

        public static string GetIMEIAt(int index)
        {
            return Utils.CallStatic<string>("GetIMEIAt", CurrentActivity, index);
        }

        public static string GetIMEI()
        {
            return Utils.CallStatic<string>("GetIMEI", CurrentActivity);
        }
        
        public static string GetDeviceId()
        {
            return Utils.CallStatic<string>("GetDeviceId", CurrentActivity);
        }

        public static int GetBatteryLevel()
        {
            return Utils.CallStatic<int>("GetBatteryLevel", CurrentActivity);
        }

        public static bool IsBatteryCharging()
        {
            return Utils.CallStatic<bool>("IsBatteryCharging", CurrentActivity);
        }
        
        public static long GetVersionCode()
        {
            return Utils.CallStatic<long>("GetVersionCode", CurrentActivity);
        }

        /// <summary>
        /// Vibrate
        /// </summary>
        /// <param name="duration">millionSeconds</param>
        public static void Vibrate(long duration)
        {
            Utils.CallStatic("Vibrate", CurrentActivity, duration);
        }

        public static bool IsPermissionGranted(string permission)
        {
            return Utils.CallStatic<bool>("IsPermissionGranted", CurrentActivity, permission);
        }
        
        public static int CheckPermission(string permission)
        {
            return Utils.CallStatic<int>("CheckPermission", CurrentActivity, permission);
        }

        /// <summary>
        /// RequestPermissions  
        /// </summary>
        /// <param name="permission">use '|' contact permissions if more then one</param>
        /// <param name="onGranted">which permission has been granted</param>
        /// <param name="onDenied">which permission has ben denied</param>
        /// <param name="onDeniedAlways">which permission has ben denied and no ask next time</param>
        /// <returns>true if all permissions granted</returns>
        public static bool RequestPermissions(string permission
            , Action<string> onGranted = null
            , Action<string> onDenied = null
            , Action<string> onDeniedAlways = null)
        {
            object threadLock = new object();
            lock (threadLock)
            {
                PermissionRequestCallback callback = new PermissionRequestCallback(threadLock, onGranted, onDenied, onDeniedAlways, null);
                Utils.CallStatic("RequestPermissions", CurrentActivity, permission, callback);
                if (callback.Result == -1)
                    System.Threading.Monitor.Wait( threadLock );

                return callback.Result == 1;
            }
        }
        
        public static void RequestPermissionsAsync(string permission
            , Action<string> onGranted = null
            , Action<string> onDenied = null
            , Action<string> onDeniedAlways = null
            , Action<bool> onCompleted = null)
        {
            PermissionRequestCallback callback = new PermissionRequestCallback(null, onGranted, onDenied, onDeniedAlways, onCompleted);
            Utils.CallStatic("RequestPermissions", CurrentActivity, permission, callback);
        }

        public static void OpenSettings()
        {
            Utils.CallStatic("OpenSettings", CurrentActivity);
        }

        public static bool OSAvailable(int version)
        {
            return Utils.CallStatic<bool>("OSAvailable", version);
        }

    }
}
#endif