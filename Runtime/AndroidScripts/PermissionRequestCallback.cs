#if UNITY_ANDROID
using System;
using System.Threading;
using UnityEngine;

namespace PluginSet.Core
{
    public interface IPermissionRequestCallback {
        void onGranted(string permission);
        void onDenied(string permission);
        void onDeniedAlways(string permission);
        void onCompleted(bool allGranted);
    }

    public class PermissionRequestCallback : AndroidJavaProxy, IPermissionRequestCallback
    {
		private object _threadLock;
        private Action<string> _onGranted;
        private Action<string> _onDenied;
        private Action<string> _onDeniedAlways;

        public int Result { get; private set; }

        public PermissionRequestCallback(
            object threadLock,
            Action<string> onGranted,
            Action<string> onDenied,
            Action<string> onDeniedAlways)
            : base("com.pluginset.devices.IPermissionRequestCallback")
        {
            Result = -1;
            
            _threadLock = threadLock;
            _onGranted = onGranted;
            _onDenied = onDenied;
            _onDeniedAlways = onDeniedAlways ?? onDenied;
        }

        public void onGranted(string permission)
        {
            _onGranted?.Invoke(permission);
        }

        public void onDenied(string permission)
        {
            _onDenied?.Invoke(permission);
        }

        public void onDeniedAlways(string permission)
        {
            _onDeniedAlways?.Invoke(permission);
        }

        public void onCompleted(bool allGranted)
        {
            Result = allGranted ? 1 : 0;
            
            lock (_threadLock)
            {
                Monitor.Pulse(_threadLock);
            }
        }
    }
}
#endif