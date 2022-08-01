using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IFriendShip
    {
        private Dictionary<string, IFriendShip> _friendShips;

        private Dictionary<string, IFriendShip> FriendShips
        {
            get
            {
                if (_friendShips == null)
                {
                    _friendShips = new Dictionary<string, IFriendShip>();
                    foreach (var plugin in GetPlugins<IFriendShip>())
                    {
                        _friendShips.Add(plugin.Name, plugin);
                    }
                }

                return _friendShips;
            }
        }

#region NotImplementedException
        public void ModifySelfUserProfile(string json, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void ModifyFriendProfile(string userId, string json, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void GetFriendProfileList(Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void GetFriendsInfo(List<string> userIds, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void GetFriendsInfo(string jsonUserIds, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void SearchFriends(string json, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void AddFriend(string userId, string json, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteFriend(List<string> userIds, bool both, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteFriend(string jsonUserIds, bool both, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void CheckFriendType(List<string> userIds, bool both, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void CheckFriendType(string jsonUserIds, bool both, Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void AddToBlackList(List<string> userIds, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void AddToBlackList(string jsonUserIds, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void GetBlackList(Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void DeleteFromBlackList(List<string> userIds, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteFromBlackList(string jsonUserIds, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void GetRequestList(Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void DeleteRequest(List<string> requestIds, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteRequest(string jsonRequestIds, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void HandleAgreeRequest(string userId, string json, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void HandleAgreeAndAddRequest(string userId, string json, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void HandleRejectRequest(string userId, string json, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }
#endregion

        public void ModifySelfUserProfileWith(string pluginName, string json, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.ModifySelfUserProfile(json, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void ModifyFriendProfileWith(string pluginName, string userId, string json, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.ModifyFriendProfile(userId, json, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetFriendProfileListWith(string pluginName, Action<Result> callback)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetFriendProfileList(callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
        
        public void GetFriendsInfoWith(string pluginName, List<string> userIds, Action<Result> callback)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetFriendsInfo(userIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
        
        public void SearchFriendsWith(string pluginName, string json, Action<Result> callback)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.SearchFriends(json, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetFriendsInfoWith(string pluginName, string jsonUserIds, Action<Result> callback)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetFriendsInfo(jsonUserIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void AddFriendWith(string pluginName, string userId, string json, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.AddFriend(userId, json, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteFriendWith(string pluginName, List<string> userIds, bool both, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteFriend(userIds, both, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteFriendWith(string pluginName, string jsonUserIds, bool both, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteFriend(jsonUserIds, both, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void CheckFriendTypeWith(string pluginName, List<string> userIds, bool both, Action<Result> callback)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.CheckFriendType(userIds, both, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void CheckFriendTypeWith(string pluginName, string jsonUserIds, bool both, Action<Result> callback)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.CheckFriendType(jsonUserIds, both, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void AddToBlackListWith(string pluginName, List<string> userIds, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.AddToBlackList(userIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void AddToBlackListWith(string pluginName, string jsonUserIds, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.AddToBlackList(jsonUserIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetBlackListWith(string pluginName, Action<Result> callback)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetBlackList(callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteFromBlackListWith(string pluginName, List<string> userIds, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteFromBlackList(userIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteFromBlackListWith(string pluginName, string jsonUserIds, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteFromBlackList(jsonUserIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetRequestListWith(string pluginName, Action<Result> callback)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetRequestList(callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteRequestWith(string pluginName, List<string> requestIds, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteRequest(requestIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteRequestWith(string pluginName, string jsonRequestIds, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteRequest(jsonRequestIds, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void HandleAgreeRequestWith(string pluginName, string userId, string json, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.HandleAgreeRequest(userId, json, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void HandleAgreeAndAddRequestWith(string pluginName, string userId, string json, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.HandleAgreeAndAddRequest(userId, json, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void HandleRejectRequestWith(string pluginName, string userId, string json, Action<Result> callback = null)
        {
            if (FriendShips.TryGetValue(pluginName, out var plugin))
            {
                plugin.HandleRejectRequest(userId, json, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }
    }
}