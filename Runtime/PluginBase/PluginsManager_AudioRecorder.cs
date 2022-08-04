using System;
using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager: IAudioRecorder
    {
        private Dictionary<string, IAudioRecorder> _audioRecorders;

        private Dictionary<string, IAudioRecorder> AudioRecorders
        {
            get
            {
                if (_audioRecorders == null)
                {
                    _audioRecorders = new Dictionary<string, IAudioRecorder>();
                    foreach (var plugin in GetPlugins<IAudioRecorder>())
                    {
                        _audioRecorders.Add(plugin.Name, plugin);
                    }
                }

                return _audioRecorders;
            }
        }
        
        public void StartRecording(string savePath, Action<Result> callback = null)
        {
            throw new NotImplementedException();
        }

        public void StopRecording()
        {
            foreach (var plugin in AudioRecorders.Values)
            {
                plugin.StopRecording();
            }
        }

        public void CancelRecording()
        {
            foreach (var plugin in AudioRecorders.Values)
            {
                plugin.CancelRecording();
            }
        }

        public void PlayRecordFile(string recordPath, string json = null)
        {
            throw new NotImplementedException();
        }

        public void StopPlayRecord()
        {
            foreach (var plugin in AudioRecorders.Values)
            {
                plugin.StopPlayRecord();
            }
        }

        public void SetMicVolume(int volume)
        {
            throw new NotImplementedException();
        }

        public void StartRecordingWith(string pluginName, string savePath, Action<Result> callback = null)
        {
            if (AudioRecorders.TryGetValue(pluginName, out var plugin))
            {
                plugin.StartRecording(savePath, callback);
                return;
            }
            
            InvalidCallback(callback, pluginName, savePath);
        }

        public void StopRecordingWith(string pluginName)
        {
            if (AudioRecorders.TryGetValue(pluginName, out var plugin))
            {
                plugin.StopRecording();
            }
        }
        
        public void CancelRecordingWith(string pluginName)
        {
            if (AudioRecorders.TryGetValue(pluginName, out var plugin))
            {
                plugin.CancelRecording();
            }
        }
        
        public void PlayRecordFileWith(string pluginName, string recordPath, string json = null)
        {
            if (AudioRecorders.TryGetValue(pluginName, out var plugin))
            {
                plugin.PlayRecordFile(recordPath, json);
            }
        }
        
        public void StopPlayRecordWith(string pluginName)
        {
            if (AudioRecorders.TryGetValue(pluginName, out var plugin))
            {
                plugin.StopPlayRecord();
            }
        }
        
        public void SetMicVolumeWith(string pluginName, int volume)
        {
            if (AudioRecorders.TryGetValue(pluginName, out var plugin))
            {
                plugin.SetMicVolume(volume);
            }
        }
    }
}