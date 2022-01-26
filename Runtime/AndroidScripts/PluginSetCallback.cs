using System;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace PluginSet.Core
{
    public interface IPluginSetCallback {
        void onSuccess(string result);
        void onFailed(int code, string message);
    }

    public delegate void SuccessCallback(string json);

    public delegate void FailedCallback(int code, string message);

    public class PluginSetCallback:
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaProxy,
#endif
        IPluginSetCallback
    {
        private readonly SuccessCallback OnSuccess;
        private readonly FailedCallback OnFailed;
        
        public PluginSetCallback(SuccessCallback success, FailedCallback failed)
#if !UNITY_EDITOR && UNITY_ANDROID
        :base("com.pluginset.core.IPluginSetCallback")
#endif
        {
            OnSuccess = success;
            OnFailed = failed;
        }

        public void onSuccess(string result)
        {
            OnSuccess?.Invoke(result);
        }

        public void onFailed(int code, string message)
        {
            OnFailed?.Invoke(code, message);
        }
    }
}
