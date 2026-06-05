using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : ICloudDataStoragePlugin
    {
        private Dictionary<string, ICloudDataStoragePlugin> _cloudStoragePlugins;

        private Dictionary<string, ICloudDataStoragePlugin> CloudStoragePlugins
        {
            get
            {
                if (_cloudStoragePlugins == null)
                {
                    _cloudStoragePlugins = new Dictionary<string, ICloudDataStoragePlugin>();
                    foreach (var plugin in GetPlugins<ICloudDataStoragePlugin>())
                    {
                        _cloudStoragePlugins[plugin.Name] = plugin;
                    }
                }

                return _cloudStoragePlugins;
            }
        }

        private ICloudDataStoragePlugin GetFirstReadyCloudStoragePlugin()
        {
            return CloudStoragePlugins.Values.FirstOrDefault();
        }

        public void SetData(string uniqId, string path, string json, string revision, Action<Result> callback = null)
        {
            var plugin = GetFirstReadyCloudStoragePlugin();
            if (plugin == null)
            {
                callback?.Invoke(NotReadyResult());
                return;
            }

            plugin.SetData(uniqId, path, json, revision, callback);
        }

        public void GetData(string uniqId, string path, Action<Result> callback)
        {
            var plugin = GetFirstReadyCloudStoragePlugin();
            if (plugin == null)
            {
                callback?.Invoke(NotReadyResult());
                return;
            }

            plugin.GetData(uniqId, path, callback);
        }

        public void DeleteData(string uniqId, string path, Action<Result> callback = null)
        {
            var plugin = GetFirstReadyCloudStoragePlugin();
            if (plugin == null)
            {
                callback?.Invoke(NotReadyResult());
                return;
            }

            plugin.DeleteData(uniqId, path, callback);
        }

        public void SetDataWith(
            string pluginName,
            string uniqId,
            string path,
            string json,
            string revision,
            Action<Result> callback = null)
        {
            if (CloudStoragePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.SetData(uniqId, path, json, revision, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void GetDataWith(string pluginName, string uniqId, string path, Action<Result> callback)
        {
            if (CloudStoragePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.GetData(uniqId, path, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        public void DeleteDataWith(string pluginName, string uniqId, string path, Action<Result> callback = null)
        {
            if (CloudStoragePlugins.TryGetValue(pluginName, out var plugin))
            {
                plugin.DeleteData(uniqId, path, callback);
                return;
            }

            InvalidCallback(callback, pluginName);
        }

        private static Result NotReadyResult()
        {
            return new Result
            {
                Success = false,
                Code = PluginConstants.FailDefaultCode,
                Error = "Cloud storage not ready",
                PluginName = string.Empty,
            };
        }
    }
}
