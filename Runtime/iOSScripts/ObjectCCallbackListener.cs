#if UNITY_EDITOR || UNITY_IOS
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PluginSet.Core
{
    public class ObjectCCallbackListener: MonoBehaviour
    {
        [Serializable]
        private struct CallBackData
        {
            public string callback;
            public bool success;
            public int code;
            public string result;
        }
        
        private const string ListenerObjectName = "UnityAppleListener";

        private static ObjectCCallbackListener _instance;
        
        public static void InitListener()
        {
            var gameObject = new GameObject(ListenerObjectName);
            DontDestroyOnLoad(gameObject);
            _instance = gameObject.AddComponent<ObjectCCallbackListener>();
        }

        public static string RegisterCallback(IPluginSetCallback callback)
        {
            return _instance.AddCallback(callback);
        }

        private ulong _callbackIndex = 0;
        private Dictionary<string, IPluginSetCallback> _callbacks = new Dictionary<string, IPluginSetCallback>();

        private string AddCallback(IPluginSetCallback callback)
        {
            var index = _callbackIndex++;
            var key = $"callback_{index}";
            _callbacks.Add(key, callback);
            return key;
        }

        private void OnUnityAppleCallback(string json)
        {
            var data = JsonUtility.FromJson<CallBackData>(json);
            if (string.IsNullOrEmpty(data.callback))
                return;

            if (_callbacks.TryGetValue(data.callback, out var callback))
            {
                if (data.success)
                    callback?.onSuccess(data.result);
                else
                    callback?.onFailed(data.code, data.result);

                _callbacks.Remove(data.callback);
            }
        }
    }
}
#endif