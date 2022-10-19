using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : ILoginPlugin
    {
        private Dictionary<string, ILoginPlugin> _loginPlugins;

        private Dictionary<string, ILoginPlugin> LoginPlugins
        {
            get
            {
                if (_loginPlugins == null)
                {
                    _loginPlugins = new Dictionary<string, ILoginPlugin>();
                    foreach (var plugin in GetPlugins<ILoginPlugin>())
                    {
                        _loginPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _loginPlugins;
            }
        }

        public bool IsEnableLogin => GetValidLoginTypes().Length > 0;

        /// <summary>
        /// 任一支持登录的SDK是登录状态，返回true
        /// </summary>
        public bool IsLoggedIn => LoginPlugins.Any(kv => kv.Value.IsLoggedIn);

        public string[] GetLoginTypes()
        {
            return LoginPlugins.Keys.ToArray();
        }

        public string[] GetValidLoginTypes()
        {
            return LoginPlugins.Where(kv => kv.Value.IsEnableLogin).Select(kv => kv.Key).ToArray();
        }

        public bool LoginEnable(string pluginName)
        {
            if (LoginPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableLogin;

            return false;
        }
        
        public bool IsEnableLoginWith(string pluginName)
        {
            if (LoginPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnableLogin;

            return false;
        }

        public void Login(Action<Result> callback = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                LoginWith((string) context.Data, callback);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_LOGIN_TYPE, context))
            {
                callback?.Invoke(new Result
                {
                    Success = false,
                    Error = "Login api need point to a plugin with name",
                    PluginName = string.Empty,
                });

                PluginsEventContext.Return(context);
            }
        }

        public void LoginWith(string pluginName, Action<Result> callback = null)
        {
            if (LoginPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableLogin)
                {
                    plugin.Login(callback);
                    return;
                }
            }

            InvalidCallback(callback, pluginName);
        }

        public void Logout(Action<Result> callback = null)
        {
            foreach (var plugin in LoginPlugins.Values)
            {
                if (plugin.IsLoggedIn)
                {
                    plugin.Logout(callback);
                }
            }
        }

        public void LogoutWith(string pluginName, Action<Result> callback = null)
        {
            if (LoginPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnableLogin)
                {
                    plugin.Logout(callback);
                    return;
                }
            }

            InvalidCallback(callback, pluginName);
        }

        public string GetUserLoginData()
        {
            var list = new List<string>();
            foreach (var plugin in LoginPlugins.Values)
            {
                if (plugin.IsLoggedIn)
                {
                    list.Add($"\"{plugin.Name}\":{plugin.GetUserLoginData()}");
                }
            }
            
            return $"{{{string.Join(",", list)}}}";
        }

        public string GetUserLoginDataWith(string pluginName)
        {
            if (LoginPlugins.TryGetValue(pluginName, out var plugin))
            {
                return plugin.GetUserLoginData();
            }

            return "{}";
        }
    }
}
