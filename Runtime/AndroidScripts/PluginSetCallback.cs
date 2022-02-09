using System;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace PluginSet.Core
{
    public interface IPluginSetCallback {
        void onSuccess(string result);
        void onFailed(int code, string message);
    }
    
    public class PluginSetCallbackBase:
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaProxy,
#endif
        IPluginSetCallback
    {
        public PluginSetCallbackBase()
#if !UNITY_EDITOR && UNITY_ANDROID
        :base("com.pluginset.core.IPluginSetCallback")
#endif
        {
        }

        public virtual void onSuccess(string result)
        {
        }

        public virtual void onFailed(int code, string message)
        {
        }
    }

    public delegate void SuccessCallback(string json);

    public delegate void FailedCallback(int code, string message);

    public class PluginSetCallback: PluginSetCallbackBase
    {
        private readonly SuccessCallback OnSuccess;
        private readonly FailedCallback OnFailed;
        
        public PluginSetCallback(SuccessCallback success, FailedCallback failed)
        {
            OnSuccess = success;
            OnFailed = failed;
        }

        public override void onSuccess(string result)
        {
            OnSuccess?.Invoke(result);
        }

        public override void onFailed(int code, string message)
        {
            OnFailed?.Invoke(code, message);
        }
    }
}
