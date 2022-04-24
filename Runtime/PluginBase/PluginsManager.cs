using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace PluginSet.Core
{
    // 只对PluginSet开头的Assembly中的类有效
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginRegister : Attribute
    {
    }

    public sealed partial class PluginsManager : PluginBase
    {
        private static readonly Logger Logger = LoggerManager.GetLogger("PluginsManager");

        private static List<Type> PluginTypes;

        static PluginsManager()
        {
            var registerType = typeof(PluginRegister);
            PluginTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                where assembly.FullName.StartsWith("PluginSet")
                from type in assembly.GetTypes()
                where type.IsDefined(registerType, false)
                select type).ToList();
        }

        private static PluginsManager _instance;
        public static PluginsManager Instance => StartPlugins();

        public static readonly SimpleDataSet GlobalData = new SimpleDataSet();

        public static PluginsManager StartPlugins()
        {
            if (_instance != null) return _instance;
            var obj = new GameObject();
            obj.SetActive(false);
            _instance = obj.AddComponent<PluginsManager>();
            _instance.InitPlugins();
            obj.SetActive(true);
            return _instance;
        }

        public static void RegisterPlugin<T>() where T : PluginBase
        {
            var type = typeof(T);
            if (!PluginTypes.Contains(type))
                PluginTypes.Add(type);
        }

        public override string Name => "PluginsManager";

        public event Action OnPluginsStartOver;

        public event Func<IEnumerator> OnPluginsReStart;

        public bool IsStarted => _isStarted;

        private bool _isStarting;
        private bool _needRestart;
        private bool _isStarted;

        private int _mainThreadId;

        private IStartPlugin[] _startPlugins;

        private IPluginBase[] _plugins;

        private void InitPlugins()
        {
            _instance = this;
            _isStarted = false;
            var o = gameObject;
            o.name = Name;
            DontDestroyOnLoad(o);

            MainThread.Init();
            
#if UNITY_IOS
            ObjectCCallbackListener.InitListener();
#endif

            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Application.logMessageReceivedThreaded += OnLoggerErrorHandle;

            var allPluginTypes = PluginTypes;
            var count = allPluginTypes.Count;
            foreach (var type in allPluginTypes)
            {
                Logger.Debug("PluginsManager collect plugin named {0} in {1} <<<<<<<<<<< ", type.Name, count);
            }

            var pluginList = new List<IPluginBase>(count);
            foreach (var type in allPluginTypes)
            {
                Logger.Debug("PluginsManager pre add plugin {0} <<<<<<<<<<< ", type.Name);
                var cmp = o.AddComponent(type) as PluginBase;
                if (cmp == null)
                    continue;

                Logger.Debug("PluginsManager Add plugin {0} >>>>>>>>>>> ", cmp.Name);
                cmp.SetPluginsMangerInstance(this);
                pluginList.Add(cmp);
            }

            _plugins = pluginList.ToArray();
        }

        public IEnumerable<T> GetPlugins<T>() where T : IPluginBase
        {
            return from plugin in _plugins
                where plugin is T
                select (T) plugin;
        }

        // 重启游戏
        public void Restart()
        {
            Logger.Debug("准备重启");
            if (_isStarting)
            {
                Logger.Debug($"重启游戏失败 {_isStarting} {_needRestart}");
                _needRestart = true;
                return;
            }

            _needRestart = true;
            StartCoroutine(RestartIEnumerator());
        }

        public void ReportError(string error, string stack = "", LogType type = LogType.Error)
        {
            ReportError(error, stack, "Report", type);
        }

        private void ReportError(string error, string stack, string threadName, LogType type)
        {
            SendNotification(PluginConstants.NOTIFY_REPORT, new ReportError
            {
                Condition = error,
                StackTrack = stack,
                ThreadName = threadName,
                LogType = type
            });
        }

        protected override void Init(PluginSetConfig config)
        {
            AddEventListener(PluginConstants.NOTIFY_RESTART, Restart);
        }

        private void Start()
        {
            StartCoroutine(StartAll());
        }

        private IEnumerator StartAll()
        {
            _isStarting = true;

            if (_startPlugins == null)
            {
                var plugins = GetPlugins<IStartPlugin>().ToList();
                plugins.Sort((a, b) =>
                {
                    var ao = a.StartOrder;
                    var bo = b.StartOrder;
                    return ao > bo ? 1 : ao < bo ? -1 : 0;
                });
                _startPlugins = plugins.ToArray();
            }

            // 开始所有插件的启动
            for (int i = 0; i < _startPlugins.Length; i++)
            {
                var plugin = _startPlugins[i];
                Logger.Debug("Start Plugin >>>>>>>>>>>>> start {0}", plugin.Name);
                yield return plugin.StartPlugin();
                Logger.Debug("Start Plugin >>>>>>>>>>>>> over needRestart = {0}", _needRestart);
                if (_needRestart)
                    break;
            }

            // 初始化结束，是否需要重启
            _isStarting = false;
            if (_needRestart)
            {
                yield return RestartIEnumerator();
            }
            else
            {
                OnPluginsStartOver?.Invoke();
                _isStarted = true;
            }
        }

        private void DisposeAll(bool isAppQuit = false)
        {
            if (_startPlugins == null || _startPlugins.Length <= 0)
                return;

            foreach (var plugin in _startPlugins)
            {
                if (plugin.IsRunning)
                    plugin.DisposePlugin(isAppQuit);
            }
        }

        private IEnumerator RestartIEnumerator()
        {
            if (_isStarting)
                yield break;
            
            Logger.Debug("开始重启");
            _isStarting = true;
            yield return null;
            DisposeAll(false);

            yield return OnPluginsReStart?.Invoke();
            yield return Resources.UnloadUnusedAssets();

            GC.Collect();

            // 重新启动
            _needRestart = false;
            yield return StartAll();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            Application.logMessageReceivedThreaded -= OnLoggerErrorHandle;

            dispatcher.RemoveEventListeners();
            //这个只会在游戏关闭的时候触发 游戏关闭的时候没必要在主动销毁一遍了 崩溃率贼高
            DisposeAll(true);
            _startPlugins = null;
#endif
        }

        private void OnLoggerErrorHandle(string condition, string stackTrack, LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    if (threadId == _mainThreadId)
                    {
                        ReportError(condition, stackTrack, "MainThread", type);
                    }
                    else
                    {
                        MainThread.Run(delegate
                        {
                            ReportError(condition, stackTrack, "SubThread" + threadId, type);
                        });
                    }

                    break;
                // 如果需要处理，在项目代码中增加相关监听
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SendNotification(PluginConstants.NOTIFY_APPLICATION_ENTER_BACKGROUND);
            }
            else
            {
                SendNotification(PluginConstants.NOTIFY_APPLICATION_ENTER_FOREGROUND);
            }
        }
    }
}
