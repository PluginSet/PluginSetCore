using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager: IShareMiniProgramPlugin
    {
        private Dictionary<string, IShareMiniProgramPlugin> _shareMiniProgramPlugins;

        private Dictionary<string, IShareMiniProgramPlugin> ShareMiniProgramPlugins
        {
            get
            {
                if (_shareMiniProgramPlugins == null)
                {
                    _shareMiniProgramPlugins = new Dictionary<string, IShareMiniProgramPlugin>();
                    foreach (var plugin in GetPlugins<IShareMiniProgramPlugin>())
                    {
                        _shareMiniProgramPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _shareMiniProgramPlugins;
            }
        }
        
        public string[] GetShareMiniProgramTypes()
        {
            return ShareMiniProgramPlugins.Keys.ToArray();
        }
        
        public string[] GetValidShareMiniProgramTypes()
        {
            return ShareMiniProgramPlugins.Where(kv => kv.Value.IsEnableShare).Select(kv => kv.Key).ToArray();
        }
        
        public bool IsEnableShareMiniProgramWith(string pluginName)
        {
            if (ShareMiniProgramPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShare;

            return false;
        }

        public void ShareMiniProgram(string url, string path, string appId, Action success = null, Action fail = null,
            string title = null, string desc = null, string image = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                ShareMiniProgramWith((string) context.Data, url, path, appId, success, fail, title, desc, image);
                PluginsEventContext.Return(context);
            };
            
            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_SHARE_MINIPROGRAM_TYPE, context))
            {
                fail?.Invoke();
                PluginsEventContext.Return(context);
            }
        }

        public void ShareMiniProgramWith(string pluginName, string url, string path, string appId,
            Action success = null, Action fail = null,
            string title = null, string desc = null, string image = null)
        {
            if (ShareMiniProgramPlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.ShareMiniProgram(url, path, appId, success, fail, title, desc, image);
                return;
            }
            
            fail?.Invoke();
        }
    }
}