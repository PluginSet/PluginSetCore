using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IProfile
    {
        private Dictionary<string, IProfile> _profilePlugins;

        private Dictionary<string, IProfile> ProfilePlugins
        {
            get
            {
                if (_profilePlugins == null)
                {
                    _profilePlugins = new Dictionary<string, IProfile>();
                    foreach (var plugin in GetPlugins<IProfile>())
                    {
                        _profilePlugins.Add(plugin.Name, plugin);
                    }
                }

                return _profilePlugins;
            }
        }
        

#region NotImplementedException

        public void GetUserProfileList(List<string> userIds, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void GetUserProfileList(string jsonUserIds, Action<Result> callback)
        {
            throw new NotImplementedException();
        }
#endregion
        
        public void GetUserProfileListWith(string pluginName, List<string> userIds, Action<Result> callback)
        {
            if (ProfilePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetUserProfileList(userIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetUserProfileListWith(string pluginName, string jsonUserIds, Action<Result> callback)
        {
            if (ProfilePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetUserProfileList(jsonUserIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
    }
}