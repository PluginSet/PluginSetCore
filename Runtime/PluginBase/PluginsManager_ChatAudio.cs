using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IChatAudio
    {
        private Dictionary<string, IChatAudio> _chatAudio;

        private Dictionary<string, IChatAudio> ChatAudios
        {
            get
            {
                if (_chatAudio == null)
                {
                    _chatAudio = new Dictionary<string, IChatAudio>();
                    foreach (var plugin in GetPlugins<IChatAudio>())
                    {
                        _chatAudio.Add(plugin.Name, plugin);
                    }
                }

                return _chatAudio;
            }
        }

#region NotImplementedException

        public void AddRemoteStateChangeCallback(Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public void RemoveRemoteStateChangeCallback(Action<Result> callback)
        {
            throw new NotImplementedException();
        }

        public bool IsRemoteMute(string userId)
        {
            throw new NotImplementedException();
        }

        public void MuteRemoteAudio(string userId, bool enable)
        {
            throw new System.NotImplementedException();
        }

        public void MuteAllRemoteAudio(bool enable)
        {
            throw new System.NotImplementedException();
        }

        public int GetAudioCaptureVolume()
        {
            throw new System.NotImplementedException();
        }

        public void SetAudioCaptureVolume(int volume)
        {
            throw new System.NotImplementedException();
        }

        public int GetAudioPlayVolume()
        {
            throw new System.NotImplementedException();
        }

        public void SetAudioPlayVolume(int volume)
        {
            throw new System.NotImplementedException();
        }
#endregion

        public void AddRemoteStateChangeCallbackWith(string pluginName, Action<Result> callback)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                plugin.AddRemoteStateChangeCallback(callback);
                return;
            }
            
            InvalidCallback(callback, pluginName);
        }

        public void RemoveRemoteStateChangeCallbackWith(string pluginName, Action<Result> callback)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                plugin.RemoveRemoteStateChangeCallback(callback);
                return;
            }
            
            InvalidCallback(callback, pluginName);
        }
        
        public bool IsRemoteMuteWith(string pluginName, string userId)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                return plugin.IsRemoteMute(userId);
            }

            return false;
        }

        public void MuteRemoteAudioWith(string pluginName, string userId, bool enable)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                plugin.MuteRemoteAudio(userId, enable);
            }
        }

        public void MuteAllRemoteAudioWith(string pluginName, bool enable)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                plugin.MuteAllRemoteAudio(enable);
            }
        }

        public int GetAudioCaptureVolumeWith(string pluginName)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                return plugin.GetAudioCaptureVolume();
            }

            return 0;
        }

        public void SetAudioCaptureVolumeWith(string pluginName, int volume)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                plugin.SetAudioCaptureVolume(volume);
            }
        }

        public int GetAudioPlayVolumeWith(string pluginName)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                return plugin.GetAudioPlayVolume();
            }

            return 0;
        }

        public void SetAudioPlayVolumeWith(string pluginName, int volume)
        {
            if (ChatAudios.TryGetValue(pluginName, out var plugin))
            {
                plugin.SetAudioPlayVolume(volume);
            }
        }
    }
}