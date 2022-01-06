using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IReport
    {
        private List<IReport> _reportPlugins;

        private List<IReport> ReportPlugins
        {
            get
            {
                if (_reportPlugins == null)
                {
                    _reportPlugins = new List<IReport>();
                    foreach (var plugin in GetPlugins<IReport>())
                    {
                        _reportPlugins.Add(plugin);
                    }
                }

                return _reportPlugins;
            }
        }
        
        public void OnUserRegister()
        {
            foreach (var plugin in _reportPlugins)
            {
                plugin.OnUserRegister();
            }
        }
        
        public void OnUserRealName()
        {
            foreach (var plugin in _reportPlugins)
            {
                plugin.OnUserRealName();
            }
        }
    }
}