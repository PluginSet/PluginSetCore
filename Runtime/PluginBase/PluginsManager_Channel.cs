using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{

    public sealed partial class PluginsManager : IChannel
    {
        public int ChannelPriority => 0;
        
        private Dictionary<string, IChannel> _channelPlugins;
        private List<IChannel> _channelList;

        private void _InitChannelPlugins()
        {
            _channelPlugins = new Dictionary<string, IChannel>();
            foreach (var plugin in GetPlugins<IChannel>())
            {
                _channelPlugins.Add(plugin.Name, plugin);
            }

            _channelList = _channelPlugins.Values.ToList();
            _channelList.Sort((a, b) => b.ChannelPriority - a.ChannelPriority);
        }

        private Dictionary<string, IChannel> ChannelPlugins
        {
            get
            {
                if (_channelPlugins == null)
                    _InitChannelPlugins();

                return _channelPlugins;
            }
        }

        public List<IChannel> ChannelList
        {
            get
            {
                if (_channelList == null)
                    _InitChannelPlugins();
                
                return _channelList;
            }
        }

        public string GetChannelName()
        {
            if (ChannelList.Count <= 0)
            {
                Logger.Warn("No plugin for GetChannelName ");
                return null;
            }

            for (int i = 0; i < ChannelList.Count; i++)
            {
                var plugin = ChannelList[i];
                string channelName = plugin.GetChannelName();
                if (!string.IsNullOrEmpty(channelName))
                    return channelName;
            }

            return null;
        }

        public string GetChannelNameWith(string pluginName)
        {
            if (ChannelPlugins.TryGetValue(pluginName, out var plugin))
            {
                return plugin.GetChannelName();
            }

            Logger.Warn("Cannot GetChannelName with plugin " + pluginName);
            return null;
        }

        public int GetChannelId()
        {
            if (ChannelList.Count <= 0)
            {
                Logger.Warn("No plugin for GetChannelId ");
                return -1;
            }

            for (int i = 0; i < ChannelList.Count; i++)
            {
                var plugin = ChannelList[i];
                int channelId = plugin.GetChannelId();
                if (channelId >= 0)
                    return channelId;
            }

            return -1;
        }
        
        public int GetChannelIdWith(string pluginName)
        {
            if (ChannelPlugins.TryGetValue(pluginName, out var plugin))
            {
                return plugin.GetChannelId();
            }

            Logger.Warn("Cannot GetChannelId with plugin " + pluginName);
            return -1;
        }
        
    }
}