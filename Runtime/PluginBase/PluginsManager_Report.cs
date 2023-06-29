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
        
        public void OnUserRegister(string method = "Indefinite")
        {
            foreach (var plugin in ReportPlugins)
            {
                plugin.OnUserRegister(method);
            }
        }
        
        public void OnUserRealName()
        {
            foreach (var plugin in ReportPlugins)
            {
                plugin.OnUserRealName();
            }
        }

        public void OnUserPurchase(bool success, string currency, float price, string paymentMethod, int amount = 1,
            string productId = null, string productName = null, string productType = null)
        {
            foreach (var plugin in ReportPlugins)
            {
                plugin.OnUserPurchase(success, currency, price, paymentMethod, amount, productId, productName, productType);
            }
        }
    }
}