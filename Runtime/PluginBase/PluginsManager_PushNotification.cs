#if ENABLE_PUSH_MODULE
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : IRemotePushNotificationPlugin
    {
        private Dictionary<string, IRemotePushNotificationPlugin> _remotePushPlugins;

        private Dictionary<string, IRemotePushNotificationPlugin> RemotePushPlugins
        {
            get
            {
                if (_remotePushPlugins == null)
                {
                    _remotePushPlugins = new Dictionary<string, IRemotePushNotificationPlugin>();
                    foreach (var plugin in GetPlugins<IRemotePushNotificationPlugin>())
                    {
                        _remotePushPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _remotePushPlugins;
            }
        }

        public bool IsEnablePush => GetValidPushTypes().Length > 0;

        public bool HasPushToken => GetValidPushTypes().Any(n => RemotePushPlugins[n].HasPushToken);

        public string PushToken
        {
            get
            {
                var plugin = GetFirstValidRemotePushPlugin();
                return plugin?.PushToken ?? string.Empty;
            }
        }

        public string[] GetPushTypes()
        {
            return RemotePushPlugins.Keys.ToArray();
        }

        public string[] GetValidPushTypes()
        {
            return RemotePushPlugins.Where(kv => kv.Value.IsEnablePush).Select(kv => kv.Key).ToArray();
        }

        public bool EnablePush(string pluginName)
        {
            if (RemotePushPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnablePush;

            return false;
        }

        public bool HasPushTokenWith(string pluginName)
        {
            if (RemotePushPlugins.TryGetValue(pluginName, out var plugin) && plugin.IsEnablePush)
                return plugin.HasPushToken;

            return false;
        }

        public string GetPushTokenWith(string pluginName)
        {
            if (RemotePushPlugins.TryGetValue(pluginName, out var plugin) && plugin.IsEnablePush)
                return plugin.PushToken ?? string.Empty;

            return string.Empty;
        }

        public void SubscribeToTopic(string topic)
        {
            var plugin = GetFirstValidRemotePushPlugin();
            plugin?.SubscribeToTopic(topic);
        }

        public void SubscribeToTopicWith(string pluginName, string topic)
        {
            if (RemotePushPlugins.TryGetValue(pluginName, out var plugin) && plugin.IsEnablePush)
                plugin.SubscribeToTopic(topic);
        }

        public void UnsubscribeFromTopic(string topic)
        {
            var plugin = GetFirstValidRemotePushPlugin();
            plugin?.UnsubscribeFromTopic(topic);
        }

        public void UnsubscribeFromTopicWith(string pluginName, string topic)
        {
            if (RemotePushPlugins.TryGetValue(pluginName, out var plugin) && plugin.IsEnablePush)
                plugin.UnsubscribeFromTopic(topic);
        }

        private IRemotePushNotificationPlugin GetFirstValidRemotePushPlugin()
        {
            foreach (var kv in RemotePushPlugins)
            {
                if (kv.Value.IsEnablePush)
                    return kv.Value;
            }

            return null;
        }
    }
}
#endif
