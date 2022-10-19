using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareMusicUrlPlugin
    {
        private Dictionary<string, IShareMusicUrlPlugin> _shareMusicUrlPlugins;

        private Dictionary<string, IShareMusicUrlPlugin> ShareMusicUrlPlugins
        {
            get
            {
                if (_shareMusicUrlPlugins == null)
                {
                    _shareMusicUrlPlugins = new Dictionary<string, IShareMusicUrlPlugin>();
                    foreach (var plugin in GetPlugins<IShareMusicUrlPlugin>())
                    {
                        _shareMusicUrlPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareMusicUrlPlugins;
            }
        }
        
        public string[] GetShareMusicUrlTypes()
        {
            return ShareMusicUrlPlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareMusicUrlTypes()
        {
            return ShareMusicUrlPlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }
        
        public bool IsEnableShareMusicUrlWith(string pluginName)
        {
            if (ShareMusicUrlPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShare;

            return false;
        }

        public void ShareMusicUrl(string musicUrl, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareMusicUrlWith((string) context.Data, musicUrl, success, fail, title, desc, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_MUSIC_URL_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }

        public void ShareMusicUrlWith(string pluginName, string musicUrl, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            if (ShareMusicUrlPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareMusicUrl(musicUrl, success, fail, title, desc, image);
                return;
            }
            
            fail?.Invoke();
        }
    }
}