using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareImagePlugin
    {
        private Dictionary<string, IShareImagePlugin> _shareImagePlugins;

        private Dictionary<string, IShareImagePlugin> ShareImagePlugins
        {
            get
            {
                if (_shareImagePlugins == null)
                {
                    _shareImagePlugins = new Dictionary<string, IShareImagePlugin>();
                    foreach (var plugin in GetPlugins<IShareImagePlugin>())
                    {
                        _shareImagePlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareImagePlugins;
            }
        }
        
        public string[] GetShareImageTypes()
        {
            return ShareImagePlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareImageTypes()
        {
            return ShareImagePlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }

        public void ShareImage(string imagePath, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareImageWith((string) context.Data, imagePath, success, fail, title, desc, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_IMAGE_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }

        public void ShareImageWith(string pluginName, string imagePath, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            if (ShareImagePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareImage(imagePath, success, fail, title, desc, image);
                return;
            }
            
            fail?.Invoke();
        }
    }
}