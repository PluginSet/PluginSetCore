
using UnityEditor;
using UnityEditor.PackageManager;

namespace PluginSet.Core.Editor
{
    [BuildTools]
    public static class ModuleSymbols
    {
        private static bool IncludePackage(string packageName)
        {
            return !string.IsNullOrEmpty(Global.GetPackageFullPath(packageName));
        }
        
        [OnSyncEditorSetting]
        public static void CollectAllModuleSymbols(BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<ModuleIncludeParams>();
            
            buildParams.IncludePhysics = buildParams.IncludePhysics || IncludePackage("com.unity.modules.physics");
            buildParams.IncludeAudio = buildParams.IncludeAudio || IncludePackage("com.unity.modules.audio");
            buildParams.IncludeAnimation = buildParams.IncludeAnimation || IncludePackage("com.unity.modules.animation");
            buildParams.IncludeImageConversion = buildParams.IncludeImageConversion || IncludePackage("com.unity.modules.imageconversion");
            buildParams.IncludeUnityWebRequestTexture = buildParams.IncludeUnityWebRequestTexture || IncludePackage("com.unity.modules.unitywebrequesttexture");
            EditorUtility.SetDirty(context.BuildChannels);
            EditorUtility.SetDirty(buildParams);

            if (buildParams.IncludePhysics)
                context.Symbols.Add("UNITY_MODULE_PHYSICS");
            if (buildParams.IncludeAudio)
                context.Symbols.Add("UNITY_MODULE_AUDIO");
            if (buildParams.IncludeAnimation)
                context.Symbols.Add("UNITY_MODULE_ANIMATION");
            if (buildParams.IncludeImageConversion && buildParams.IncludeUnityWebRequestTexture)
                context.Symbols.Add("UNITY_MODULE_WEBTEXTURE");
        }
    }
}