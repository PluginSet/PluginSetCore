using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareWebPagePlugin
    {
        private Dictionary<string, IShareWebPagePlugin> _shareWebPagePlugins;

        private Dictionary<string, IShareWebPagePlugin> ShareWebPagePlugins
        {
            get
            {
                if (_shareWebPagePlugins == null)
                {
                    _shareWebPagePlugins = new Dictionary<string, IShareWebPagePlugin>();
                    foreach (var plugin in GetPlugins<IShareWebPagePlugin>())
                    {
                        _shareWebPagePlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareWebPagePlugins;
            }
        }
        
        public string[] GetShareWebPageTypes()
        {
            return ShareWebPagePlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareWebPageTypes()
        {
            return ShareWebPagePlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }

        public void ShareWebPage(string webUrl, Action success = null, Action fail = null
            , string title = null, string desc = null,string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareWebPageWith((string) context.Data, webUrl, success, fail, title, desc, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_WEBPAGE_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }

        public void ShareWebPageWith(string pluginName, string webUrl, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null)
        {
            if (ShareWebPagePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareWebPage(webUrl, success, fail, title, desc, image);
                return;
            }
            
            fail?.Invoke();
        }
    }
}