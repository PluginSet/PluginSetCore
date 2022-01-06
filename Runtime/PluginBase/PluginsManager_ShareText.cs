using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareTextPlugin
    {
        public bool IsEnableShare => true;

        private Dictionary<string, IShareTextPlugin> _shareTextPlugins;

        private Dictionary<string, IShareTextPlugin> ShareTextPlugins
        {
            get
            {
                if (_shareTextPlugins == null)
                {
                    _shareTextPlugins = new Dictionary<string, IShareTextPlugin>();
                    foreach (var plugin in GetPlugins<IShareTextPlugin>())
                    {
                        _shareTextPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareTextPlugins;
            }
        }
        
        public string[] GetShareTextTypes()
        {
            return ShareTextPlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareTextTypes()
        {
            return ShareTextPlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }

        public void ShareText(string text, Action success = null, Action fail = null
            , string title = null, string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareTextWith((string) context.Data, text, success, fail, title, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_TEXT_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }

        public void ShareTextWith(string pluginName, string text, Action success = null, Action fail = null
            , string title = null, string image = null)
        {
            if (ShareTextPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareText(text, success, fail, title, image);
                return;
            }
            
            fail?.Invoke();
        }

    }
}