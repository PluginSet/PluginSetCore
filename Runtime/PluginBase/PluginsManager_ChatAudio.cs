using System.Collections.Generic;

namespace PluginSet.Core
{
    public partial class PluginsManager
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
    }
}