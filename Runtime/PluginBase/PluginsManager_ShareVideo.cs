using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareVideoPlugin
    {
        private Dictionary<string, IShareVideoPlugin> _shareVideoPlugins;

        private Dictionary<string, IShareVideoPlugin> ShareVideoPlugins
        {
            get
            {
                if (_shareVideoPlugins == null)
                {
                    _shareVideoPlugins = new Dictionary<string, IShareVideoPlugin>();
                    foreach (var plugin in GetPlugins<IShareVideoPlugin>())
                    {
                        _shareVideoPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareVideoPlugins;
            }
        }
        
        public string[] GetShareVideoTypes()
        {
            return ShareVideoPlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareVideoTypes()
        {
            return ShareVideoPlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }
        
        public bool IsEnableShareVideoWith(string pluginName)
        {
            if (ShareVideoPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShare;

            return false;
        }

        public void ShareVideo(string videoFile, Action success = null, Action fail = null, string extra = null, string title = null,
            string desc = null, string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareVideoWith((string) context.Data, videoFile, success, fail, extra, title, desc, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_VIDEO_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }

        public void ShareVideoWith(string pluginName, string videoFile, Action success = null, Action fail = null,
            string extra = null
            , string title = null, string desc = null, string image = null)
        {
            if (ShareVideoPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareVideo(videoFile, success, fail, extra, title, desc, image);
                return;
            }
            
            fail?.Invoke();
        }
    }
}