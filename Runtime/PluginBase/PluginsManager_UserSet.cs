using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager : IUserSet
    {
        private string _userId = string.Empty;
        private Dictionary<string, object> _userInfo = new Dictionary<string, object>();

        private Dictionary<string, IUserSet> _userSetPlugins;

        private Dictionary<string, IUserSet> UserSetPlugins
        {
            get
            {
                if (_userSetPlugins == null)
                {
                    _userSetPlugins = new Dictionary<string, IUserSet>();
                    foreach (var plugin in GetPlugins<IUserSet>())
                    {
                        _userSetPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _userSetPlugins;
            }
        }

        private bool SetUserInternal(string userId)
        {
            if (_userId.Equals(userId))
                return false;

            _userId = userId;
            _userInfo.Clear();
            SendNotification(PluginConstants.NOTIFY_USER_ID, _userId);

            return true;
        }

        public void SetUser(string userId)
        {
            SetUserInternal(userId);
        }

        public void SetUserInfo(string key, object value)
        {
            if (_userInfo.TryGetValue(key, out var oldValue))
            {
                if (oldValue.Equals(value))
                    return;
            }

            _userInfo[key] = value;
            SendNotification(PluginConstants.NOTIFY_USER_INFO, _userInfo);
        }

        public void SetUserInfo(Dictionary<string, object> pairs)
        {
            var dirty = false;
            foreach (var kv in pairs)
            {
                if (_userInfo.TryGetValue(kv.Key, out var oldValue))
                    if (oldValue.Equals(kv.Value))
                        continue;

                _userInfo[kv.Key] = kv.Value;
                dirty = true;
            }

            if (dirty)
                SendNotification(PluginConstants.NOTIFY_USER_INFO, _userInfo);
        }

        public void ClearUserInfo()
        {
            _userInfo.Clear();
            SendNotification(PluginConstants.NOTIFY_CLEAR_USER_INFO);
        }

        public void SetUserWith(string pluginName, string userId)
        {
            if (UserSetPlugins.TryGetValue(pluginName, out var plugin))
                plugin.SetUser(userId);
        }

        public void SetUserInfoWith(string pluginName, string key, object value)
        {
            if (UserSetPlugins.TryGetValue(pluginName, out var plugin))
                plugin.SetUserInfo(key, value);
        }

        public void SetUserInfoWith(string pluginName, Dictionary<string, object> pairs)
        {
            if (UserSetPlugins.TryGetValue(pluginName, out var plugin))
                plugin.SetUserInfo(pairs);
        }

        public void ClearUserInfoWith(string pluginName)
        {
            if (UserSetPlugins.TryGetValue(pluginName, out var plugin))
                plugin.ClearUserInfo();
        }

        public void FlushUserInfo()
        {
            foreach (var plugin in UserSetPlugins.Values)
            {
                plugin.FlushUserInfo();
            }
        }
        
        
        public void FlushUserInfoWith(string pluginName)
        {
            if (UserSetPlugins.TryGetValue(pluginName, out var plugin))
                plugin.FlushUserInfo();
        }
    }
}
