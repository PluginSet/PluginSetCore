using System;
using System.Collections;
using UnityEngine;

namespace PluginSet.Core
{
    public class CoroutineHelper: MonoBehaviour
    {
        private static CoroutineHelper _instance;

        internal static CoroutineHelper CreateInstance(string name)
        {
            var obj = new GameObject(name);
            DontDestroyOnLoad(obj);
            return obj.AddComponent<CoroutineHelper>();
        }

        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = CreateInstance("Coroutine-instance");

                return _instance;
            }
        }

        public Coroutine StartCoroutine(IEnumerator routine, Action callback)
        {
            return StartCoroutine(RoutineCallback(routine, callback));
        }
        

        public static IEnumerator RoutineCallback(IEnumerator routine, Action callback)
        {
            yield return routine;
            callback?.Invoke();
        }

        public static IEnumerator ContactRoutine(params IEnumerator[] routines)
        {
            foreach (var routine in routines)
            {
                yield return routine;
            }
        }
    }
}