using System;
using System.Reflection;
using Google;
using UnityEditor;

namespace PluginSet.Tool.PlayerServicesResolver.Editor
{
    [InitializeOnLoad]
    public static class DefaultResolverSetting
    {
        const string Namespace = "GooglePlayServices.";
        private const string AutoResolveKey = Namespace + "AutoResolverEnabled";
        
        static DefaultResolverSetting()
        {
            SetEnableAutoResolution(false);

            IOSResolver.PodfileAlwaysAddMainTarget = false;
        }

        private static void SetEnableAutoResolution(bool value)
        {
            var projectSettings = new ProjectSettings(Namespace);
            projectSettings.SetBool(AutoResolveKey, value);
        }
    }
}