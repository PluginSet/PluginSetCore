using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    [BuildTools]
    [BuildChannelsParams("iOS", -10000, "苹果打包相关设置")]
    [VisibleCaseBoolValue("SupportIOS", true)]
    public class IosBuildParams: ScriptableObject
    {
        [Tooltip("存储KeyChains时默认使用的services字符串")]
        public string KeyChainServices = "com.pluginset.core";
        
        [Tooltip("使用的appleDeveloperTeamID")]
        public string TeamID;
        
        [Tooltip("启动自动签名")]
        public bool AutomaticallySign;

        [Tooltip("签名文件ID")]
        [VisibleCaseBoolValue("AutomaticallySign", false)]
        public string ProfileId;

        [OnSyncEditorSetting]
        public static void OnSyncExportSetting_IOS(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.iOS)
                return;

            var asset = context.BuildChannels;
            var setting = asset.Get<IosBuildParams>("iOS");
            
            if (!string.IsNullOrEmpty(setting.TeamID))
                PlayerSettings.iOS.appleDeveloperTeamID = setting.TeamID;
            
            PlayerSettings.iOS.appleEnableAutomaticSigning = setting.AutomaticallySign; 
            if (!setting.AutomaticallySign && !string.IsNullOrEmpty(setting.ProfileId))
                PlayerSettings.iOS.iOSManualProvisioningProfileID = setting.ProfileId;
        }

        [iOSXCodeProjectModify(int.MaxValue)]
        public static void OnIosXCodeProjectModify(BuildProcessorContext context, PBXProjectManager project)
        {
            var plist = project.PlistDocument;
            //ios一系类权限请求文本
            plist.AddPlistValue("NSLocationAlwaysUsageDescription", "Your location is required for xyz benefits for you");
            plist.AddPlistValue("NSLocationWhenInUseUsageDescription", "Your location is required for xyz benefits for you");
            plist.AddPlistValue("NSPhotoLibraryUsageDescription", "This app requires access to the photo library.");
            plist.AddPlistValue("NSCameraUsageDescription", "Your consent is required to access the camera");
            plist.AddPlistValue("NSMicrophoneUsageDescription", "Your consent is required to access the microphone");
            plist.AddPlistValue("NSLocationUsageDescription", "Your consent is required to access the microphone");
            plist.AddPlistValue("NSCalendarsUsageDescription", "Your consent is required to access the calendar");
            plist.AddPlistValue("NSMotionUsageDescription", "Your consent is required to access sports and fitness");
            //这两个权限是关于健康啥的 一般app不用申请 不然会拒审
            //plist.SetPlistValue("NSHealthUpdateUsageDescription", "Your consent is required to access health updates");
            //plist.SetPlistValue("NSHealthShareUsageDescription", "Your consent is required to access health sharing");
            plist.AddPlistValue("NSBluetoothPeripheralUsageDescription", "Your consent is required to access Bluetooth");
            plist.AddPlistValue("NSAppleMusicUsageDescription", "Your consent is required to access the media database");
            plist.AddPlistValue("NSRemindersUsageDescription", "Your consent is required to access reminders");
            plist.AddPlistValue("NSUserTrackingUsageDescription","Your consent is required to access advertising tracking");

            var buildParams = context.BuildChannels.Get<IosBuildParams>("iOS");
            plist.AddPlistValue("KeyChainServices", buildParams.KeyChainServices);
            
            // DevicesUtil.mm 需要依赖 AppTrackingTransparency, 但Unity中并没有相应选项
            project.Project.AddFrameworkToProject(project.UnityFramework, "AppTrackingTransparency.framework", true);
            
            var xcodeTarget = project.UnityFramework;
            project.Project.AddBuildProperty(xcodeTarget, "CLANG_ENABLE_MODULES", "YES");
            project.Project.AddBuildProperty(xcodeTarget, "LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
        }


    }
}