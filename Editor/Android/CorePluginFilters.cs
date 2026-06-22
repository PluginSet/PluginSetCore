using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    [InitializeOnLoad]
    public static class CorePluginFilters
    {
        private static bool IsUnity6000OrNewer()
        {
#if UNITY_6 || UNITY_6000_0_OR_NEWER
            return true;
#else
            return false;
#endif
        }

        private static bool FilterLegacyActivity(string fileName, BuildProcessorContext context)
        {
            var enable = context.BuildTarget == BuildTarget.Android && !IsUnity6000OrNewer();
            if (!enable)
                Debug.Log("Filter Core Android plugin: excluded " + fileName);
            return !enable;
        }

        private static bool FilterUnity6000Activity(string fileName, BuildProcessorContext context)
        {
            var enable = context.BuildTarget == BuildTarget.Android && IsUnity6000OrNewer();
            if (!enable)
                Debug.Log("Filter Core Android plugin: excluded " + fileName);
            return !enable;
        }

        static CorePluginFilters()
        {
            PluginFilter.RegisterFilter("com.pluginset.core/Plugins/Android/Legacy", FilterLegacyActivity);
            PluginFilter.RegisterFilter("com.pluginset.core/Plugins/Android/Unity6000", FilterUnity6000Activity);
        }
    }
}
