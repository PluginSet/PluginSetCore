#if UNITY_ANDROID
using UnityEngine;

namespace PluginSet.Core
{
    public static class AndroidClasses
    {
        private static AndroidJavaClass _unityPlayer;
        
        public static AndroidJavaClass UnityPlayer
        {
            get
            {
                if (_unityPlayer == null)
                    _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                
                return _unityPlayer;
            }
        }

        private static AndroidJavaClass _toast;

        public static AndroidJavaClass Toast
        {
            get
            {
                if (_toast == null)
                    _toast = new AndroidJavaClass("android.widget.Toast");

                return _toast;
            }
        }
        
        private static AndroidJavaClass _log;

        public static AndroidJavaClass Log
        {
            get
            {
                if (_log == null)
                    _log = new AndroidJavaClass("android.util.Log");

                return _log;
            }
        }
        
        private static AndroidJavaClass _packageManager;

        public static AndroidJavaClass PackageManager
        {
            get
            {
                if (_packageManager == null)
                    _packageManager = new AndroidJavaClass("android.content.pm.PackageManager");

                return _packageManager;
            }
        }
        

        private static AndroidJavaClass _system;

        public static AndroidJavaClass System
        {
            get
            {
                if (_system == null)
                    _system = new AndroidJavaClass("java.lang.System");

                return _system;
            }
        }

        private static AndroidJavaClass _systemProperties;

        public static AndroidJavaClass SystemProperties
        {
            get
            {
                if (_systemProperties == null)
                    _systemProperties = new AndroidJavaClass("android.os.SystemProperties");

                return _systemProperties;
            }
        }

    }
}
#endif