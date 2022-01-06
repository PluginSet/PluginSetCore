using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public partial class PluginsManager : IMomentPlugin
    {
        private Dictionary<string, IMomentPlugin> _momentPlugins;
        private Dictionary<string, IMomentPlugin> MomentPlugins
        {
            get
            {
                if (_momentPlugins == null)
                {
                    _momentPlugins = new Dictionary<string, IMomentPlugin>();
                    foreach (var plugin in GetPlugins<IMomentPlugin>())
                    {
                        _momentPlugins.Add(plugin.Name, plugin);
                    }
                }
                return _momentPlugins;
            }
        }

        public bool IsEnableShowMoment => GetValidMomentTypes().Length > 0;

        public bool IsReadyToShowMoment => MomentPlugins.Any(kv => kv.Value.IsReadyToShowMoment);

        public string[] GetValidMomentTypes()
        {
            return MomentPlugins.Where(kv => kv.Value.IsEnableShowMoment).Select(kv => kv.Key).ToArray();
        }

        public string GetReadyMomentPlugin()
        {
            foreach (var plugin in MomentPlugins.Values)
                if (plugin.IsReadyToShowMoment)
                    return plugin.Name;

            return null;
        }

        public bool IsReadyShowMoment(string pluginName)
        {
            if (MomentPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsReadyToShowMoment;

            return false;
        }

        public bool EnableShowMoment(string pluginName)
        {
            if (MomentPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableShowMoment;

            return false;
        }

        public void MomentOpen()
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                MomentOpenWith((string)context.Data);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_MOMENT_OPEN_TYPE, context))
            {
                var plugin = GetReadyMomentPlugin();
                if (!string.IsNullOrEmpty(plugin))
                    MomentOpenWith(plugin);
                PluginsEventContext.Return(context);
            }
        }

        public void MomentOpenWith(string pluginName)
        {
            if (MomentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowMoment)
                {
                    plugin.MomentOpen();
                }
            }
        }

        public void MomentFetch()
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                MomentFetchWith((string)context.Data);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_MOMENT_FETCH_TYPE, context))
            {
                var plugin = GetReadyMomentPlugin();
                if (!string.IsNullOrEmpty(plugin))
                    MomentFetchWith(plugin);
                PluginsEventContext.Return(context);
            }
        }

        public void MomentFetchWith(string pluginName)
        {
            if (MomentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowMoment)
                {
                    plugin.MomentFetch();
                }
            }
        }

        public void MomentOpenPage(string page)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                MomentOpenPageWith((string)context.Data, page);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_MOMENT_OPEN_PAGE_TYPE, context))
            {
                var plugin = GetReadyMomentPlugin();
                if (!string.IsNullOrEmpty(plugin))
                    MomentOpenPageWith(plugin, page);
                PluginsEventContext.Return(context);
            }
        }

        public void MomentOpenPageWith(string pluginName, string page)
        {
            if (MomentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowMoment)
                {
                    plugin.MomentOpenPage(page);
                }
            }
        }

        public void MomentTryCloseMomentConfirmWindow(string tips, string content)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                MomentTryCloseMomentConfirmWindowWith((string)context.Data, tips, content);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_MOMENT_TRY_CLOSE_MOMENT, context))
            {
                var plugin = GetReadyMomentPlugin();
                if (!string.IsNullOrEmpty(plugin))
                    MomentTryCloseMomentConfirmWindowWith(plugin, tips, content);
                PluginsEventContext.Return(context);
            }
        }

        public void MomentTryCloseMomentConfirmWindowWith(string pluginName, string tips, string content)
        {
            if (MomentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowMoment)
                {
                    plugin.MomentTryCloseMomentConfirmWindow(tips, content);
                }
            }
        }

        public void MomentClose()
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                MomentCloseWith((string)context.Data);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_MOMENT_CLOSE_MOMENT, context))
            {
                var plugin = GetReadyMomentPlugin();
                if (!string.IsNullOrEmpty(plugin))
                    MomentCloseWith(plugin);
                PluginsEventContext.Return(context);
            }
        }

        public void MomentCloseWith(string pluginName)
        {
            if (MomentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableShowMoment)
                {
                    plugin.MomentClose();
                }
            }
        }
    }
}