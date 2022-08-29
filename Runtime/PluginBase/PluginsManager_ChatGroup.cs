using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IChatGroup
    {
        private Dictionary<string, IChatGroup> _chatGroups;

        private Dictionary<string, IChatGroup> ChatGroups
        {
            get
            {
                if (_chatGroups == null)
                {
                    _chatGroups = new Dictionary<string, IChatGroup>();
                    foreach (var plugin in GetPlugins<IChatGroup>())
                    {
                        _chatGroups.Add(plugin.Name, plugin);
                    }
                }

                return _chatGroups;
            }
        }

#region NotImplementedException
        public void CreateGroup(string groupId, string jsonParams, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteGroup(string groupId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteGroupConv(string groupId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void JoinGroup(string groupId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void JoinGroup(string groupId, string hello, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void QuitGroup(string groupId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public string SendGroupTextMessage(string groupId, string content, string extra = null, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public string SendGroupSoundMessage(string groupId, string soundPath, int duration, string extra = null, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public string SendGroupCustomMessage(string groupId, string customType, string content, string extra = null, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void CancelSendGroupMessage(string groupId, string messageId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void RevokeGroupMessage(string groupId, string messageId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteGroupMessage(string groupId, string messageId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void GetGroupMessageList(string groupId, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void GetGroupMessageList(string groupId, uint maxCount, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void GetGroupMessageList(string groupId, uint maxCount, string lastMessageId, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void ClearGroupHistoryMessage(string groupId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void ReportReadGroupMessage(string convId, string messageId, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        #endregion
        
        public void CreateGroupWith(string pluginName, string groupId, string jsonParams, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.CreateGroup(groupId, jsonParams, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteGroupWith(string pluginName, string groupId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteGroup(groupId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteGroupConvWith(string pluginName, string groupId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteGroupConv(groupId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void JoinGroupWith(string pluginName, string groupId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.JoinGroup(groupId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void JoinGroupWith(string pluginName, string groupId, string hello, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.JoinGroup(groupId, hello, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void QuitGroupWith(string pluginName, string groupId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.QuitGroup(groupId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public string SendGroupTextMessageWith(string pluginName, string groupId, string content, string extra = null, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                return plugin.SendGroupTextMessage(groupId, content, extra, callback);
            }

            InvalidCallback(callback, pluginName);
            return null;
        }
        
        public string SendGroupSoundMessageWith(string pluginName, string groupId, string soundPath, int duration, string extra = null, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                return plugin.SendGroupSoundMessage(groupId, soundPath, duration, extra, callback);
            }

            InvalidCallback(callback, pluginName);
            return null;
        }

        public string SendGroupCustomMessageWith(string pluginName, string groupId, string customType, string content, string extra = null, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                return plugin.SendGroupCustomMessage(groupId, customType, content, extra, callback);
            }

            InvalidCallback(callback, pluginName);
            return null;
        }

        public void CancelSendGroupMessageWith(string pluginName, string groupId, string messageId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.CancelSendGroupMessage(groupId, messageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void RevokeGroupMessageWith(string pluginName, string groupId, string messageId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.RevokeGroupMessage(groupId, messageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteGroupMessageWith(string pluginName, string groupId, string messageId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteGroupMessage(groupId, messageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetGroupMessageListWith(string pluginName, string groupId, Action<Result> callback)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetGroupMessageList(groupId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetGroupMessageListWith(string pluginName, string groupId, uint maxCount, Action<Result> callback)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetGroupMessageList(groupId, maxCount, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetGroupMessageListWith(string pluginName, string groupId, uint maxCount, string lastMessageId, Action<Result> callback)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetGroupMessageList(groupId, maxCount, lastMessageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void ClearGroupHistoryMessageWith(string pluginName, string groupId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.ClearGroupHistoryMessage(groupId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
        
        public void ReportReadGroupMessageWith(string pluginName, string convId, string messageId, Action<Result> callback = null)
        {
            if (ChatGroups.TryGetValue(pluginName, out var plugin))
            {
                plugin.ReportReadGroupMessage(convId, messageId, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
    }
}