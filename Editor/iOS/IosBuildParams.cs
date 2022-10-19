using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        
        [Tooltip("adHoc包对应的签名文件")]
        [VisibleCaseBoolValue("AutomaticallySign", false)]
        public BuildProvisioningProfile AppStoreBuildProfile;

        [Tooltip("adHoc包对应的签名文件")]
//        [VisibleCaseBoolValue("AutomaticallySign", false)]
        public BuildProvisioningProfile AdHocBuildProfile;

        private void OnValidate()
        {
            OnUpdateProfile(ref AdHocBuildProfile);
            OnUpdateProfile(ref AppStoreBuildProfile);
        }
        
        private static readonly string s_PatternTeamID = "<key>TeamIdentifier<\\/key>[\n\t]*<array>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternUUID = "<key>UUID<\\/key>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternSpecifier = "<key>Name<\\/key>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternTeamName = "<key>TeamName<\\/key>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternDeveloperCertificates = "<key>DeveloperCertificates<\\/key>[\n\t]*<array>[\n\t]*<data>([\\w\\/+=]+)<\\/data>";
        private static readonly string s_DistributionPattern = "iPhone Distribution: ";

        private static void ParseFile(string filePath, out string profileId, out string profileSpecifier, out string codeSignIdentity, out ProvisioningProfileType type)
        {
            string input = File.ReadAllText(filePath);
            Match match1 = Regex.Match(input, s_PatternUUID, RegexOptions.Singleline);
            if (match1.Success)
                profileId = match1.Groups[1].Value;
            else
                profileId = string.Empty;
            
            Match match2 = Regex.Match(input, s_PatternDeveloperCertificates, RegexOptions.Singleline);
            if (match2.Success)
                type = !Encoding.UTF8.GetString(Convert.FromBase64String(match2.Groups[1].Value))
                    .Contains(s_DistributionPattern)
                    ? ProvisioningProfileType.Development
                    : ProvisioningProfileType.Distribution;
            else
                type = ProvisioningProfileType.Automatic;
            
            Match match3 = Regex.Match(input, s_PatternSpecifier, RegexOptions.Singleline);
            if (match3.Success)
                profileSpecifier = match1.Groups[1].Value;
            else
                profileSpecifier = string.Empty;

            string teamId;
            Match match4 = Regex.Match(input, s_PatternTeamID, RegexOptions.Singleline);
            if (match4.Success)
                teamId = match4.Groups[1].Value;
            else
                teamId = string.Empty;

            string teamName;
            Match match5 = Regex.Match(input, s_PatternTeamName, RegexOptions.Singleline);
            if (match5.Success)
                teamName = match5.Groups[1].Value;
            else
                teamName = string.Empty;

            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(teamName))
                codeSignIdentity = string.Empty;
            else
                codeSignIdentity = $"Apple Distribution: {teamName} ({teamId})";
        }
        
        internal static void OnUpdateProfile(ref BuildProvisioningProfile profile)
        {
            if (string.IsNullOrEmpty(profile.ProfileFile))
                return;
            
            ParseFile(Path.Combine(".", profile.ProfileFile), out profile.ProfileId, out profile.ProfileSpecifier, out profile.CodeSignIdentity, out profile.ProfileType);
        }


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
            if (!setting.AutomaticallySign && !string.IsNullOrEmpty(setting.AppStoreBuildProfile.ProfileFile))
            {
                PlayerSettings.iOS.iOSManualProvisioningProfileID = setting.AppStoreBuildProfile.ProfileId;
                PlayerSettings.iOS.iOSManualProvisioningProfileType = setting.AppStoreBuildProfile.ProfileType;
            }
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