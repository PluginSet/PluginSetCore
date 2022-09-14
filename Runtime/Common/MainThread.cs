using System;
using System.Collections.Generic;
using UnityEngine;

namespace PluginSet.Core
{
    public class MainThread: MonoBehaviour
    {
        private static Logger Logger = LoggerManager.GetLogger("MainThread");
        
        private static MainThread _instance;

        private static MainThread CreateInstance()
        {
            var gameObject = new GameObject("MainThread");
            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<MainThread>();
        }

        private static MainThread Instance
        {
            get
            {
                if (_instance == null)
                    _instance = CreateInstance();

                return _instance;
            }
        }

        public static MainThread Init()
        {
            return Instance;
        }

        public static void Run(Action action)
        {
            if (action == null)
                return;
            
            Instance.AddAction(action);
        }

        private Queue<Action> _actions = new Queue<Action>();
        private bool _isDispose = false;

        private void AddAction(Action action)
        {
            if (_isDispose) return;
            
            lock (_actions)
            {
                _actions.Enqueue(action);
            }
        }

        private void Update()
        {
            Action next;
            
            while (true)
            {
                lock (_actions)
                {
                    if (_actions.Count <= 0)
                        return;

                    next = _actions.Dequeue();
                }

                try
                {
                    next.Invoke();
                }
                catch (Exception e)
                {
                    Logger.Error($"Action invoke error {e.Message}, {e}");
                }
            }
        }

        private void OnDestroy()
        {
            _isDispose = true;
            lock (_actions)
            {
                _actions.Clear();
            }
        }
    }
}