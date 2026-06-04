using System;
using System.Collections.Generic;
using UnityEngine;

namespace PluginSet.Core
{
    /// <summary>
    /// 在下一帧执行回调的通用调度器；同一 <see cref="Action"/> 引用在同一帧内只会入队一次。
    /// </summary>
    public class CallLater : MonoBehaviour
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("CallLater");

        private static CallLater _instance;

        private readonly HashSet<Action> _executeSet = new HashSet<Action>();
        private readonly HashSet<Action> _waitSet = new HashSet<Action>();
        private readonly object _lock = new object();
        private bool _isDispose;

        public static CallLater Init()
        {
            return Instance;
        }

        public static void Call(Action action)
        {
            if (action == null)
                return;

            Instance.Enqueue(action);
        }

        private static CallLater Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                var gameObject = new GameObject("CallLater");
                DontDestroyOnLoad(gameObject);
                _instance = gameObject.AddComponent<CallLater>();
                return _instance;
            }
        }

        private void Enqueue(Action action)
        {
            if (_isDispose)
                return;

#if !UNITY_WEBGL
            lock (_lock)
            {
#endif
                if (_waitSet.Contains(action) || _executeSet.Contains(action))
                    return;

                _waitSet.Add(action);
#if !UNITY_WEBGL
            }
#endif
        }

        private void Update()
        {
            Action[] runList = null;

#if !UNITY_WEBGL
            lock (_lock)
            {
#endif
                if (_executeSet.Count > 0)
                {
                    runList = new Action[_executeSet.Count];
                    _executeSet.CopyTo(runList);
                    _executeSet.Clear();
                }
#if !UNITY_WEBGL
            }
#endif

            if (runList == null)
                return;

            for (int i = 0; i < runList.Length; i++)
            {
                try
                {
                    runList[i]?.Invoke();
                }
                catch (Exception e)
                {
                    Logger.Error($"CallLater invoke error: {e.Message}, {e}");
                }
            }
        }

        private void LateUpdate()
        {
#if !UNITY_WEBGL
            lock (_lock)
            {
#endif
                _executeSet.UnionWith(_waitSet);
                _waitSet.Clear();
#if !UNITY_WEBGL
            }
#endif
        }

        private void OnDestroy()
        {
            _isDispose = true;
#if !UNITY_WEBGL
            lock (_lock)
            {
#endif
                _executeSet.Clear();
                _waitSet.Clear();
#if !UNITY_WEBGL
            }
#endif
        }
    }
}
