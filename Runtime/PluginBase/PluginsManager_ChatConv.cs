using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IChatConv
    {
        private Dictionary<string, IChatConv> _chatConvs;

        private Dictionary<string, IChatConv> ChatConvs
        {
            get
            {
                if (_chatConvs == null)
                {
                    _chatConvs = new Dictionary<string, IChatConv>();
                    foreach (var plugin in GetPlugins<IChatConv>())
                    {
                        _chatConvs.Add(plugin.Name, plugin);
                    }
                }

                return _chatConvs;
            }
        }

#region NotImplementedException

        public void AddNewConvCallback(Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void RemoveNewConvCallback(Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void DeleteConv(string convId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void GetConvList(Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public string SendConvTextMessage(string userId, string content, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public string SendConvSoundMessage(string userId, string soundPath, int duration, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public string SendConvCustomMessage(string userId, string customType, string content, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void CancelSendConvMessage(string userId, string messageId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void RevokeConvMessage(string userId, string messageId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteConvMessage(string userId, string messageId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void GetConvMessageList(string userId, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void GetConvMessageList(string userId, uint maxCount, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void GetConvMessageList(string userId, uint maxCount, string lastMessageId, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void ClearConvHistoryMessage(string userId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void ReportReadConvMessage(string convId, string messageId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        #endregion
        
        
        public void AddNewConvCallbackWith(string pluginName, Action<Result> callback)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.AddNewConvCallback(callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void RemoveNewConvCallbackWith(string pluginName, Action<Result> callback)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.RemoveNewConvCallback(callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
        
        public void DeleteConvWith(string pluginName, string convId, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteConv(convId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetConvListWith(string pluginName, Action<Result> callback)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetConvList(callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public string SendConvTextMessageWith(string pluginName, string userId, string content, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                return plugin.SendConvTextMessage(userId, content, callback);
            }

            InvalidCallback(callback, pluginName);
            return null;
        }
        
        public string SendConvSoundMessageWith(string pluginName, string userId, string soundPath, int duration, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                return plugin.SendConvSoundMessage(userId, soundPath, duration, callback);
            }

            InvalidCallback(callback, pluginName);
            return null;
        }

        public string SendConvCustomMessageWith(string pluginName, string userId, string customType, string content, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                return plugin.SendConvCustomMessage(userId, customType, content, callback);
            }

            InvalidCallback(callback, pluginName);
            return null;
        }

        public void CancelSendConvMessageWith(string pluginName, string userId, string messageId, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.CancelSendConvMessage(userId, messageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void RevokeConvMessageWith(string pluginName, string userId, string messageId, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.RevokeConvMessage(userId, messageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteConvMessageWith(string pluginName, string userId, string messageId, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteConvMessage(userId, messageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetConvMessageListWith(string pluginName, string userId, Action<Result> callback)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetConvMessageList(userId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetConvMessageListWith(string pluginName, string userId, uint maxCount, Action<Result> callback)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetConvMessageList(userId, maxCount, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetConvMessageListWith(string pluginName, string userId, uint maxCount, string lastMessageId, Action<Result> callback)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetConvMessageList(userId, maxCount, lastMessageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void ClearConvHistoryMessageWith(string pluginName, string userId, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.ClearConvHistoryMessage(userId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
        
        public void ReportReadConvMessageWith(string pluginName, string convId, string messageId, Action<Result> callback = null)
        {
            if (ChatConvs.TryGetValue(pluginName, out var plugin))
            {
                plugin.ReportReadConvMessage(convId, messageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
    }
}