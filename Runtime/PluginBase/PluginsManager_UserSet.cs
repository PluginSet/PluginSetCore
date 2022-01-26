using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager : IUserSet
    {
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

//        public void SetUserInfo(Dictionary<string, object> pairs)
        public void SetUserInfo(bool isNewUser, string userId, Dictionary<string, object> pairs = null)
        {
            foreach (var plugin in UserSetPlugins.Values)
            {
                plugin.SetUserInfo(isNewUser, userId, pairs);
            }
        }

        public void ClearUserInfo()
        {
            foreach (var plugin in UserSetPlugins.Values)
            {
                plugin.ClearUserInfo();
            }
        }

        public void SetUserInfoWith(string pluginName, bool isNewUser, string userId, Dictionary<string, object> pairs = null)
        {
            if (UserSetPlugins.TryGetValue(pluginName, out var plugin))
                plugin.SetUserInfo(isNewUser, userId, pairs);
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
