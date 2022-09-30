using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IChatBase
    {
        private Dictionary<string, IChatBase> _chatBasePlugins;

        private Dictionary<string, IChatBase> ChatBasePlugins
        {
            get
            {
                if (_chatBasePlugins == null)
                {
                    _chatBasePlugins = new Dictionary<string, IChatBase>();
                    foreach (var plugin in GetPlugins<IChatBase>())
                    {
                        _chatBasePlugins.Add(plugin.Name, plugin);
                    }
                }

                return _chatBasePlugins;
            }
        }


        public void LoginChat(string json, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void LogoutChat(Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void SetMessageLocalData(string messageId, int? intData = null, string stringData = null, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }
        
        public void AddNewMessageReceiveCallback(Action<Result> callback)
        {
            foreach (var plugin in ChatBasePlugins.Values)
            {
                plugin.AddNewMessageReceiveCallback(callback);
            }
        }

        public void RemoveNewMessageReceiveCallback(Action<Result> callback)
        {
            foreach (var plugin in ChatBasePlugins.Values)
            {
                plugin.RemoveNewMessageReceiveCallback(callback);
            }
        }

        public void LoginChatWith(string pluginName, string json, Action<Result> callback = null)
        {
            if (ChatBasePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.LoginChat(json, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void LogoutChatWith(string pluginName, Action<Result> callback = null)
        {
            if (ChatBasePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.LogoutChat(callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
        
        public void AddNewMessageReceiveCallbackWith(string pluginName, Action<Result> callback)
        {
            if (ChatBasePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.AddNewMessageReceiveCallback(callback);
                return;
            }

//            InvalidCallback(callback, pluginName);
        }

        public void RemoveNewMessageReceiveCallbackWith(string pluginName, Action<Result> callback)
        {
            if (ChatBasePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.RemoveNewMessageReceiveCallback(callback);
                return;
            }

//            InvalidCallback(callback, pluginName);
        }
        
        public void SetMessageLocalDataWith(string pluginName, string messageId, int? intData = null, string stringData = null, Action<Result> callback = null)
        {
            if (ChatBasePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.SetMessageLocalData(messageId, intData, stringData, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
    }
}