using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IPrivacyAuthorizationCallback
    {
        
        private List<IPrivacyAuthorizationCallback> _paCallbackPlugins;

        private List<IPrivacyAuthorizationCallback> PACallbackPlugins
        {
            get
            {
                if (_paCallbackPlugins == null)
                {
                    _paCallbackPlugins = new List<IPrivacyAuthorizationCallback>();
                    foreach (var plugin in GetPlugins<IPrivacyAuthorizationCallback>())
                    {
                        _paCallbackPlugins.Add(plugin);
                    }
                }

                return _paCallbackPlugins;
            }
        }

        public void OnPrivacyAuthorization()
        {
            foreach (var plugin in PACallbackPlugins)
            {
                plugin.OnPrivacyAuthorization();
            }
        }
    }
}