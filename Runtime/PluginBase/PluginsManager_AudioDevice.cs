using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IAudioDevice
    {
        private Dictionary<string, IAudioDevice> _audioDevice;

        private Dictionary<string, IAudioDevice> AudioDevices
        {
            get
            {
                if (_audioDevice == null)
                {
                    _audioDevice = new Dictionary<string, IAudioDevice>();
                    foreach (var plugin in GetPlugins<IAudioDevice>())
                    {
                        _audioDevice.Add(plugin.Name, plugin);
                    }
                }

                return _audioDevice;
            }
        }

        public void StartLocalAudio(string json, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void StopLocalAudio()
        {
            foreach (var plugin in AudioDevices.Values)
            {
                plugin.StopLocalAudio();
            }
        }

        public void PauseLocalAudio()
        {
            foreach (var plugin in AudioDevices.Values)
            {
                plugin.PauseLocalAudio();
            }
        }

        public void ResumeLocalAudio()
        {
            foreach (var plugin in AudioDevices.Values)
            {
                plugin.ResumeLocalAudio();
            }
        }

        public void StartLocalAudioWith(string pluginName, string json, Action<Result> callback = null)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                plugin.StartLocalAudio(json, callback);
            }
        }

        public void StopLocalAudioWith(string pluginName)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                plugin.StopLocalAudio();
            }
        }
    }
}