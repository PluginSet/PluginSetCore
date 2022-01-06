using System.Xml;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    [BuildTools]
    [BuildChannelsParams("Android", -10000, "安卓打包相关设置")]
    [VisibleCaseBoolValue("SupportAndroid", true)]
    public class AndroidBuildParams: ScriptableObject
    {
        [Tooltip("是否使用自定义签名文件")]
        public bool UseCustomKeystore = true;
        
        [FolderDrag(".keystore")]
        [BrowserFile("Choose a KeyStore", "keystore")]
        [Tooltip("使用的KeyStore文件全路径")]
        [VisibleCaseBoolValue("UseCustomKeystore", true)]
        public string KeyStorePath;
        
        [Password]
        [Tooltip("已设定的KeyStore密码")]
        [VisibleCaseBoolValue("UseCustomKeystore", true)]
        public string KeyStorePassword;

        [Tooltip("使用的KeyStore别名")]
        [VisibleCaseBoolValue("UseCustomKeystore", true)]
        public string KeyAliasName;
        
        [Password]
        [Tooltip("已设定的KeyStore别名密码")]
        [VisibleCaseBoolValue("UseCustomKeystore", true)]
        public string KeyAliasPassword;

        [Tooltip("Manifest中的application的name属性")]
        public string ApplicationName;

        [Tooltip("Manifest中的application的theme属性")]
        public string ApplicationTheme;


        [OnSyncEditorSetting]
        public static void OnSyncExportSetting_android(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.Android)
                return;

            var asset = context.BuildChannels;
            var setting = asset.Get<AndroidBuildParams>("Android");

            if (!setting.UseCustomKeystore || string.IsNullOrEmpty(setting.KeyStorePath))
            {
#if UNITY_2019_1_OR_NEWER
                PlayerSettings.Android.useCustomKeystore = false;
                return;
#endif
            }

#if UNITY_2019_1_OR_NEWER
            PlayerSettings.Android.useCustomKeystore = true;
#endif
            PlayerSettings.Android.keystoreName = setting.KeyStorePath;
            PlayerSettings.Android.keyaliasName = setting.KeyAliasName;
            
            PlayerSettings.keystorePass = setting.KeyStorePassword;
            PlayerSettings.Android.keystorePass = setting.KeyStorePassword;
            PlayerSettings.keyaliasPass = setting.KeyAliasPassword;
            PlayerSettings.Android.keyaliasPass = setting.KeyAliasPassword;
            
            AssetDatabase.Refresh();
        }

        [AndroidManifestModify]
        public static void OnManifestModify(BuildProcessorContext context, XmlDocument main)
        {
            if (context.BuildTarget != BuildTarget.Android)
                return;
            
            var asset = context.BuildChannels;
            var setting = asset.Get<AndroidBuildParams>("Android");
            var element = main.findOrCreateElemet(AndroidConst.META_DATA_PARENT);
            if (!string.IsNullOrEmpty(setting.ApplicationName))
                element.SetAttribute("name", AndroidConst.NS_URI, setting.ApplicationName);
            
            if (!string.IsNullOrEmpty(setting.ApplicationTheme))
                element.SetAttribute("theme", AndroidConst.NS_URI, setting.ApplicationTheme);
        }
    }
}