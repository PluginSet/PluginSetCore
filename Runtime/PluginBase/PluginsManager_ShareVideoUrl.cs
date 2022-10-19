using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareVideoUrlPlugin
    {
        private Dictionary<string, IShareVideoUrlPlugin> _shareVideoUrlPlugins;

        private Dictionary<string, IShareVideoUrlPlugin> ShareVideoUrlPlugins
        {
            get
            {
                if (_shareVideoUrlPlugins == null)
                {
                    _shareVideoUrlPlugins = new Dictionary<string, IShareVideoUrlPlugin>();
                    foreach (var plugin in GetPlugins<IShareVideoUrlPlugin>())
                    {
                        _shareVideoUrlPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareVideoUrlPlugins;
            }
        }
        
        public string[] GetShareVideoUrlTypes()
        {
            return ShareVideoUrlPlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareVideoUrlTypes()
        {
            return ShareVideoUrlPlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }
        
        public bool IsEnableShareVideoUrlWith(string pluginName)
        {
            if (ShareVideoUrlPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShare;

            return false;
        }

        public void ShareVideoUrl(string videoUrl, Action success = null, Action fail = null, string extra = null, string title = null,
            string desc = null, string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareVideoUrlWith((string) context.Data, videoUrl, success, fail, extra, title, desc, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_VIDEO_URL_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }

        public void ShareVideoUrlWith(string pluginName, string videoUrl, Action success = null, Action fail = null,
            string extra = null
            , string title = null, string desc = null, string image = null)
        {
            if (ShareVideoUrlPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareVideoUrl(videoUrl, success, fail, extra, title, desc, image);
                return;
            }
            
            fail?.Invoke();
        }
    }
}