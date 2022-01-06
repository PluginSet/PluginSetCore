using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareImageUrlPlugin
    {
        private Dictionary<string, IShareImageUrlPlugin> _shareImageUrlPlugins;

        private Dictionary<string, IShareImageUrlPlugin> ShareImageUrlPlugins
        {
            get
            {
                if (_shareImageUrlPlugins == null)
                {
                    _shareImageUrlPlugins = new Dictionary<string, IShareImageUrlPlugin>();
                    foreach (var plugin in GetPlugins<IShareImageUrlPlugin>())
                    {
                        _shareImageUrlPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareImageUrlPlugins;
            }
        }
        
        public string[] GetShareImageUrlTypes()
        {
            return ShareImageUrlPlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareImageUrlTypes()
        {
            return ShareImageUrlPlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }

        public void ShareImageUrl(string imageUrl, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareImageUrlWith((string) context.Data, imageUrl, success, fail, title, desc, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_IMAGE_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }
        
        public void ShareImageUrlWith(string pluginName, string imageUrl, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            if (ShareImageUrlPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareImageUrl(imageUrl, success, fail, title, desc, image);
                return;
            }
            
            fail?.Invoke();
        }
    }
}