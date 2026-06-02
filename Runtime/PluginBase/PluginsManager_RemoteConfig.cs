using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : IRemoteConfigPlugin
    {
        private Dictionary<string, IRemoteConfigPlugin> _remoteConfigPlugins;
        private string _remoteConfigTypeName;

        private Dictionary<string, IRemoteConfigPlugin> RemoteConfigPlugins
        {
            get
            {
                if (_remoteConfigPlugins == null)
                {
                    _remoteConfigPlugins = new Dictionary<string, IRemoteConfigPlugin>();
                    foreach (var plugin in GetPlugins<IRemoteConfigPlugin>())
                    {
                        _remoteConfigPlugins[plugin.Name] = plugin;
                    }
                }

                return _remoteConfigPlugins;
            }
        }

        private IRemoteConfigPlugin GetFirstValidRemoteConfigPlugin()
        {
            return RemoteConfigPlugins.Values.FirstOrDefault();
        }

        private bool CheckRemoteConfigType<T>() where T : class, new()
        {
            var typeName = typeof(T).AssemblyQualifiedName;
            if (string.IsNullOrEmpty(_remoteConfigTypeName))
            {
                _remoteConfigTypeName = typeName;
                return true;
            }

            if (_remoteConfigTypeName == typeName)
                return true;

            Logger.Error(
                "RemoteConfig type mismatch. Expected: {0}, Current: {1}",
                _remoteConfigTypeName,
                typeName);
            return false;
        }

        public void SetDefault<T>(T config) where T : class, new()
        {
            if (!CheckRemoteConfigType<T>())
                return;

            var plugin = GetFirstValidRemoteConfigPlugin();
            if (plugin == null)
            {
                Logger.Warn("RemoteConfig plugin not found, skip SetDefault");
                return;
            }

            plugin.SetDefault(config ?? new T());
        }

        public T GetConfig<T>() where T : class, new()
        {
            if (!CheckRemoteConfigType<T>())
                return new T();

            var plugin = GetFirstValidRemoteConfigPlugin();
            if (plugin == null)
            {
                Logger.Warn("RemoteConfig plugin not found, return default config");
                return new T();
            }

            return plugin.GetConfig<T>() ?? new T();
        }
    }
}
