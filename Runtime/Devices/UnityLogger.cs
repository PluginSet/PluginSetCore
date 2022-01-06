using System.Runtime.InteropServices;

namespace PluginSet.Core
{
    public class UnityLogger: Logger
    {
        private string _tag;
        private string _tagPrefix;

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _PlatformLog(string log);
#endif

        public override string Tag
        {
            get
            {
                return _tag;
            }
            
            set
            {
                _tag = value;
                _tagPrefix = $"{value}: ";
            }
        }

        protected override void DoLogDebug(string msg, params object[] args)
        {
            UnityEngine.Debug.LogFormat(_tagPrefix + msg, args);
#if UNITY_IOS && !UNITY_EDITOR
            _PlatformLog(string.Format(_tagPrefix + msg,args));
#endif
        }

        protected override void DoLogWarn(string msg, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(_tagPrefix + msg, args);
#if UNITY_IOS && !UNITY_EDITOR
            _PlatformLog(string.Format(_tagPrefix + msg,args));
#endif
        }

        protected override void DoLogError(string msg, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(_tagPrefix + msg, args);
#if UNITY_IOS && !UNITY_EDITOR
            _PlatformLog(string.Format(_tagPrefix + msg,args));
#endif
        }

        protected override void DoLogInfo(string msg, params object[] args)
        {
            UnityEngine.Debug.LogFormat(_tagPrefix + msg, args);
#if UNITY_IOS && !UNITY_EDITOR
            _PlatformLog(string.Format(_tagPrefix + msg,args));
#endif
        }

        protected override void DoLogDebug(string msg)
        {
            UnityEngine.Debug.Log(msg);
#if UNITY_IOS && !UNITY_EDITOR
            _PlatformLog(msg);
#endif
        }

        protected override void DoLogWarn(string msg)
        {
            UnityEngine.Debug.LogWarning(msg);
#if UNITY_IOS && !UNITY_EDITOR
            _PlatformLog(msg);
#endif
        }

        protected override void DoLogError(string msg)
        {
            UnityEngine.Debug.LogError(msg);
#if UNITY_IOS && !UNITY_EDITOR
            _PlatformLog(msg);
#endif
        }

        protected override void DoLogInfo(string msg)
        {
            UnityEngine.Debug.Log(msg);
#if UNITY_IOS && !UNITY_EDITOR
            _PlatformLog(msg);
#endif
        }
    }
}