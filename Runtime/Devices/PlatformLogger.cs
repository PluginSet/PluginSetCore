#if !UNITY_EDITOR
#define USE_PLATFORM_LOG
#endif

using System.Runtime.InteropServices;

#if UNITY_ANDROID
namespace PluginSet.Core
{
    public class PlatformLogger: UnityLogger
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _PlatformLog(string log);
#endif
        
        private void DoLog(bool showStack, string msg, params object[] args)
        {
#if USE_PLATFORM_LOG
            DoLog(showStack, string.Format(msg, args));
#endif
        }
        
        private void DoLog(bool showStack, string msg)
        {
#if USE_PLATFORM_LOG
            if (showStack)
            {
                msg += "\n" + UnityEngine.StackTraceUtility.ExtractStackTrace();
            }
#if UNITY_ANDROID
            AndroidHelper.Log("Unity", msg);
#elif UNITY_IOS
            _PlatformLog(msg);
#endif
#endif
        }
        
        protected override void DoLogDebug(string msg, params object[] args)
        {
            base.DoLogDebug(msg, args);
            DoLog(false, msg, args);
        }

        protected override void DoLogWarn(string msg, params object[] args)
        {
            base.DoLogWarn(msg, args);
            DoLog(true, msg, args);
        }

        protected override void DoLogError(string msg, params object[] args)
        {
            base.DoLogError(msg, args);
            DoLog(true, msg, args);
        }

        protected override void DoLogInfo(string msg, params object[] args)
        {
            base.DoLogInfo(msg, args);
            DoLog(false, msg, args);
        }

        protected override void DoLogDebug(string msg)
        {
            base.DoLogDebug(msg);
            DoLog(false, msg);
        }

        protected override void DoLogWarn(string msg)
        {
            base.DoLogWarn(msg);
            DoLog(true, msg);
        }

        protected override void DoLogError(string msg)
        {
            base.DoLogError(msg);
            DoLog(true, msg);
        }

        protected override void DoLogInfo(string msg)
        {
            base.DoLogInfo(msg);
            DoLog(false, msg);
        }
    }
}
#endif