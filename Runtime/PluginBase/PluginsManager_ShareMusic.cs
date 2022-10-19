using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareMusicPlugin
    {
        private Dictionary<string, IShareMusicPlugin> _shareMusicPlugins;

        private Dictionary<string, IShareMusicPlugin> ShareMusicPlugins
        {
            get
            {
                if (_shareMusicPlugins == null)
                {
                    _shareMusicPlugins = new Dictionary<string, IShareMusicPlugin>();
                    foreach (var plugin in GetPlugins<IShareMusicPlugin>())
                    {
                        _shareMusicPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareMusicPlugins;
            }
        }
        
        public string[] GetShareMusicTypes()
        {
            return ShareMusicPlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareMusicTypes()
        {
            return ShareMusicPlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }
        
        public bool IsEnableShareMusicWith(string pluginName)
        {
            if (ShareMusicPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShare;

            return false;
        }

        public void ShareMusic(string musicFile, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareMusicWith((string) context.Data, musicFile, success, fail, title, desc, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_MUSIC_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }

        public void ShareMusicWith(string pluginName, string musicFile, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            if (ShareMusicPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareMusic(musicFile, success, fail, title, desc, image);
                return;
            }
            
            fail?.Invoke();
        }
    }
}