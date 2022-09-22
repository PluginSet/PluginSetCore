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
        private Action<bool> _onCompleted;

        public int Result { get; private set; }

        public PermissionRequestCallback(
            object threadLock,
            Action<string> onGranted,
            Action<string> onDenied,
            Action<string> onDeniedAlways,
            Action<bool> onCompleted)
            : base("com.pluginset.devices.IPermissionRequestCallback")
        {
            Result = -1;
            
            _threadLock = threadLock;
            _onGranted = onGranted;
            _onDenied = onDenied;
            _onDeniedAlways = onDeniedAlways ?? onDenied;
            _onCompleted = onCompleted;
        }

        public void onGranted(string permission)
        {
            MainThread.Run(delegate
            {
                _onGranted?.Invoke(permission);
            });
        }

        public void onDenied(string permission)
        {
            MainThread.Run(delegate
            {
                _onDenied?.Invoke(permission);
            });
        }

        public void onDeniedAlways(string permission)
        {
            MainThread.Run(delegate
            {
                _onDeniedAlways?.Invoke(permission);
            });
        }

        public void onCompleted(bool allGranted)
        {
            Result = allGranted ? 1 : 0;

            if (_threadLock != null)
            {
                lock (_threadLock)
                {
                    Monitor.Pulse(_threadLock);
                }
            }
            
            MainThread.Run(delegate
            {
                _onCompleted?.Invoke(allGranted);
            });
        }
    }
}
#endif