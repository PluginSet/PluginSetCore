using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PluginSet.Core.Editor
{
    public enum IosArchiveMethod
    {
        AppStore,
        AdHoc,
        Development,
    }
    
    [Serializable]
    public struct IosBuildTypeInfo
    {
        [SerializeField]
        public string teamId;
        [SerializeField]
        public string bundleId;
        [SerializeField]
        public IosArchiveMethod method;
        [SerializeField]
        public bool automaticallySign;
        [DropDownList("ProfileNames")]
        [SerializeField]
        public string profileName;
        [SerializeField]
        public bool biteCode;
    }
    
    [BuildTools]
    [BuildChannelsParams("iOS", -10000, "苹果打包相关设置")]
    [VisibleCaseBoolValue("SupportIOS", true)]
    public class IosBuildParams: ScriptableObject
    {
        [Tooltip("存储KeyChains时默认使用的services字符串")]
        public string KeyChainServices = "com.pluginset.core";

        [Tooltip("所有用到的签名文件")]
        public BuildProvisioningProfile[] BuildProfiles;
        
        [Tooltip("可提供的所有构建方式及参数")]
        public IosBuildTypeInfo[] BuildTypeInfos;
        
        [Tooltip("预设的构建方式")]
        public IosArchiveMethod PresetMothod;

        [Tooltip("开启数据收集")]
        public bool EnableAppTrackingTransparency = true;

        [Tooltip("保留 ATS 例外（NSAllowsArbitraryLoads=YES）：仅当业务/第三方 SDK 必须访问明文 HTTP 接口时才勾选，并在 App Review Notes 里说明原因")]
        public bool EnableAppTransportSecurityException = true;

        // ===== iOS 权限用途说明（Info.plist purpose string）=====
        // 重要：这些字符串会原样显示在系统权限弹窗上，App Store 审核逐条校验。
        //   · 确实用到的权限：填写“什么功能 + 具体用途”，例如
        //     "我们需要访问你的相册，以便你在意见反馈中上传截图。"
        //   · 用不到的权限：必须保持空字符串 —— 留空时 PlistDocument.AddPlistValue 会跳过写入，
        //     该权限键根本不会出现在 Info.plist 里（Guidelines 5.1.1(iii) 数据最小化）。
        //   · 切勿使用 “Your consent is required to access xxx” / “The app requires access to xxx”
        //     这类占位文案，会被自动分析判为 placeholder 直接拒审。
        [Tooltip("相册读取用途说明；不用相册请留空")]
        public string PhotoLibraryUsageDescription = "";

        [Tooltip("广告标识符(IDFA)用途说明；不申请 ATT 数据收集请留空")]
        public string UserTrackingUsageDescription = "";

        [Tooltip("定位（使用期间）用途说明；不用定位请留空")]
        public string LocationWhenInUseUsageDescription = "";

        [Tooltip("定位（始终）用途说明；不用定位请留空")]
        public string LocationAlwaysUsageDescription = "";

        [Tooltip("定位（旧 iOS 键）用途说明；不用定位请留空")]
        public string LocationUsageDescription = "";

        [Tooltip("相机用途说明；不用相机请留空")]
        public string CameraUsageDescription = "";

        [Tooltip("麦克风用途说明；不用麦克风请留空")]
        public string MicrophoneUsageDescription = "";

        [Tooltip("日历用途说明；不用日历请留空")]
        public string CalendarsUsageDescription = "";

        [Tooltip("运动与健身用途说明；不用计步/摇一摇等能力请留空")]
        public string MotionUsageDescription = "";

        [Tooltip("蓝牙用途说明；不用蓝牙请留空")]
        public string BluetoothPeripheralUsageDescription = "";

        [Tooltip("媒体与 Apple Music 用途说明；不用媒体库请留空")]
        public string AppleMusicUsageDescription = "";

        [Tooltip("提醒事项用途说明；不用提醒事项请留空")]
        public string RemindersUsageDescription = "";

        // ===== 用途说明校验（IosPurposeStringValidator）=====
        // 机制在插件、政策在项目：这里只声明“哪些键必须存在/禁止出现”，
        // 具体文案仍由上面各 UsageDescription 字段与各项目渠道配置提供。
        [Tooltip("校验：必须存在且非空的用途说明键（每行一个，也支持逗号分隔）。\n" +
                 "留空 = 只校验空值与占位文案，不校验“必须存在哪些键”")]
        public string RequiredUsageDescriptions = "";

        [Tooltip("校验：禁止出现的用途说明键（每行一个，也支持逗号分隔）。\n" +
                 "留空 = 不校验“禁止出现哪些键”。用于确保未申请权限的键不会意外进包")]
        public string ForbiddenUsageDescriptions = "";

        public IEnumerable<string> ProfileNames
        {
            get
            {
                return BuildProfiles?.Select(e => e.ProfileSpecifier);
            }
        }

        private void OnValidate()
        {
            if (BuildProfiles != null)
            {
                for (int i = 0; i < BuildProfiles.Length; i++)
                {
                    OnUpdateProfile(ref BuildProfiles[i]);
                }
            }
        }
        
        private static readonly string s_PatternTeamID = "<key>TeamIdentifier<\\/key>[\n\t]*<array>[\n\t]*<string>([^<>]*)</string>";
        private static readonly string s_PatternUUID = "<key>UUID<\\/key>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternSpecifier = "<key>Name<\\/key>[\n\t]*<string>([^<>]*)</string>";
        private static readonly string s_PatternTeamName = "<key>TeamName<\\/key>[\n\t]*<string>([^<>]*)</string>";
        private static readonly string s_PatternDeveloperCertificates = "<key>DeveloperCertificates<\\/key>[\n\t]*<array>[\n\t]*<data>([\\w\\/+=]+)<\\/data>";
        
        private static readonly string DefaultProvisioningProfileSearchPath =
            "{Home}/Library/MobileDevice/Provisioning Profiles";

        /**
         * UID=U8GTR34PG2, CN=\"Apple Distribution: Hangzhou Yuesheng Technology Co., Ltd. (U8GTR34PG2)\", OU=U8GTR34PG2, O=\"Hangzhou Yuesheng Technology Co., Ltd.\", C=US 
         */
        private static Dictionary<string, string> ParseCertSubject(string subject)
        {
            var result = new Dictionary<string, string>();

            string key = "";
            string value = null;
            int state = 0;
            bool inQuot = false;
            StringBuilder builder = new StringBuilder();

            int i = 0;
            while (i < subject.Length)
            {
                var @char = subject[i++];
                if (@char == '\\')
                {
                    builder.Append(subject[i++]);
                    continue;
                }
                if ((@char == '\"' && (i >= subject.Length || subject[i] == ',')) ||
                    @char == ',' && !inQuot)
                {
                    if (state != 2)
                        throw new Exception($"Unknown subject string at pos {i - 1}: {subject}");

                    value = builder.ToString();
                    builder.Clear();
                    state = 0;

                    result.Add(key, value);
                    i++;
                    inQuot = false;
                    continue;
                }

                if (@char == '"')
                {
                    inQuot = true;
                }
                else if (@char == '=')
                {
                    if (state != 1)
                        throw new Exception($"Unknown subject string at pos {i - 1}: {subject}");

                    key = builder.ToString();
                    builder.Clear();
                    state = 2;
                }
                else if (@char == '\"')
                {
                    if (state != 2)
                        throw new Exception($"Unknown subject string at pos {i - 1}: {subject}");

                    if (i >= subject.Length || subject[i] == ',')
                    {
                        value = builder.ToString();
                        builder.Clear();
                        state = 0;

                        result.Add(key, value);
                        i++;
                    }
                }
                else
                {
                    builder.Append(@char);
                    if (state == 0)
                        state = 1;
                }
            }

            return result;
        }

        private static void ParseFile(string filePath, out string profileId, out string profileSpecifier, out string codeSignIdentity, out ProvisioningProfileType type)
        {
            string input = File.ReadAllText(filePath);
            Match match1 = Regex.Match(input, s_PatternUUID, RegexOptions.Singleline);
            if (match1.Success)
                profileId = match1.Groups[1].Value;
            else
                profileId = string.Empty;
            
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
            
            Match match2 = Regex.Match(input, s_PatternDeveloperCertificates, RegexOptions.Singleline);
            if (match2.Success)
            {
                type = ProvisioningProfileType.Automatic;
                var data = Convert.FromBase64String(match2.Groups[1].Value);
                var cert = new X509Certificate(data);
                var result = ParseCertSubject(cert.Subject);
                if (result.TryGetValue("CN", out var cn))
                {
                    codeSignIdentity = cn;
                    if (codeSignIdentity.StartsWith("iPhone Distribution:") || codeSignIdentity.StartsWith("Apple Distribution:"))
                        type = ProvisioningProfileType.Distribution;
                    else if (codeSignIdentity.StartsWith("iPhone Development:") || codeSignIdentity.StartsWith("Apple Development:"))
                        type = ProvisioningProfileType.Development;
                }
            }
            else
                type = ProvisioningProfileType.Automatic;
            
            Match match3 = Regex.Match(input, s_PatternSpecifier, RegexOptions.Singleline);
            if (match3.Success)
                profileSpecifier = match3.Groups[1].Value;
            else
                profileSpecifier = string.Empty;
        }
        
        private static void OnUpdateProfile(ref BuildProvisioningProfile profile)
        {
            if (string.IsNullOrEmpty(profile.ProfileFile))
                return;

            ParseFile(Path.Combine(".", profile.ProfileFile), out profile.ProfileId, out profile.ProfileSpecifier, out profile.CodeSignIdentity, out profile.ProfileType);
        }

        private static void CopyProvisioningProfile(in BuildProvisioningProfile profile)
        {
            if (string.IsNullOrEmpty(profile.ProfileFile))
                return;
            
            string path = DefaultProvisioningProfileSearchPath.Replace("{Home}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            Global.CopyFile(Path.Combine(".", profile.ProfileFile), Path.Combine(path, $"{profile.ProfileId}.mobileprovision"), null);
        }
        

        [OnSyncEditorSetting]
        public static void OnSyncExportSetting_IOS(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.iOS)
                return;

            var asset = context.BuildChannels;
            var setting = asset.Get<IosBuildParams>();
            
            if (setting.BuildProfiles != null)
            {
                for (int i = 0; i < setting.BuildProfiles.Length; i++)
                {
                    CopyProvisioningProfile(in setting.BuildProfiles[i]);
                }
            }

            IosBuildTypeInfo? buildInfo = null;
            if (setting.BuildTypeInfos != null)
            {
                foreach (var info in setting.BuildTypeInfos)
                {
                    if (info.method == setting.PresetMothod)
                    {
                        buildInfo = info;
                        break;
                    }
                }
            }

            if (buildInfo.HasValue)
            {
                var info = buildInfo.Value;
                if (!string.IsNullOrEmpty(info.teamId))
                    PlayerSettings.iOS.appleDeveloperTeamID = info.teamId;
                
                PlayerSettings.iOS.appleEnableAutomaticSigning = info.automaticallySign;
                if (!info.automaticallySign)
                {
                    if (!string.IsNullOrEmpty(info.profileName))
                    {
                        BuildProvisioningProfile? profileValue = null;
                        if (setting.BuildProfiles != null)
                        {
                            foreach (var profile in setting.BuildProfiles)
                            {
                                if (profile.ProfileSpecifier.Equals(info.profileName))
                                {
                                    profileValue = profile;
                                    break;
                                }
                            }
                        }

                        if (profileValue.HasValue)
                        {
                            var profile = profileValue.Value;
                            PlayerSettings.iOS.iOSManualProvisioningProfileID = profile.ProfileId;
                            PlayerSettings.iOS.iOSManualProvisioningProfileType = profile.ProfileType;
                        }
                        else
                        {
                            throw new BuildException($"Cannot find profile with name {info.profileName}");
                        }
                        
                    }
                }
            }
        }

        [iOSXCodeProjectModify(int.MaxValue)]
        public static void OnIosXCodeProjectModify(BuildProcessorContext context, PBXProjectManager project)
        {
            var plist = project.PlistDocument;
            var buildParams = context.BuildChannels.Get<IosBuildParams>();
            //ios一系类权限请求文本
            plist.AddPlistValue("NSLocationUsageDescription", buildParams.LocationUsageDescription);
            plist.AddPlistValue("NSLocationAlwaysUsageDescription", buildParams.LocationAlwaysUsageDescription);
            plist.AddPlistValue("NSLocationWhenInUseUsageDescription", buildParams.LocationWhenInUseUsageDescription);
            plist.AddPlistValue("NSPhotoLibraryUsageDescription", buildParams.PhotoLibraryUsageDescription);
            plist.AddPlistValue("NSCameraUsageDescription", buildParams.CameraUsageDescription);
            plist.AddPlistValue("NSMicrophoneUsageDescription", buildParams.MicrophoneUsageDescription);
            plist.AddPlistValue("NSCalendarsUsageDescription", buildParams.CalendarsUsageDescription);
            plist.AddPlistValue("NSMotionUsageDescription", buildParams.MotionUsageDescription);
            //这两个权限是关于健康啥的 一般app不用申请 不然会拒审
            //plist.SetPlistValue("NSHealthUpdateUsageDescription", "Your consent is required to access health updates");
            //plist.SetPlistValue("NSHealthShareUsageDescription", "Your consent is required to access health sharing");
            plist.AddPlistValue("NSBluetoothPeripheralUsageDescription", buildParams.BluetoothPeripheralUsageDescription);
            plist.AddPlistValue("NSAppleMusicUsageDescription", buildParams.AppleMusicUsageDescription);
            plist.AddPlistValue("NSRemindersUsageDescription", buildParams.RemindersUsageDescription);

            plist.AddPlistValue("KeyChainServices", buildParams.KeyChainServices);

            // DevicesUtil.mm 需要依赖 AppTrackingTransparency, 但Unity中并没有相应选项
            if (buildParams.EnableAppTrackingTransparency)
            {
                plist.AddPlistValue("NSUserTrackingUsageDescription", buildParams.UserTrackingUsageDescription);
                project.AddFrameworkToProject(project.UnityFramework, "AppTrackingTransparency.framework", true);
                // project.PrivacyPlist.SetPlistValue("NSPrivacyTracking", true);
                // 不设置NSPrivacyTracking = true, 容易与NSPrivacyTrackingDomains、NSPrivacyCollectedDataTypeTracking 等不一致，违反隐私清单准确性要求。
                // 在Info.plist中添加App Transport Security Settings -> NSAllowsArbitraryLoads 为 YES：
                // 这是需要向审核说明的例外，改成显式开关，默认保持原有行为；
                // 不需要访问明文 HTTP 接口的项目建议关掉，或改为按域名的 NSExceptionDomains 收敛。
                if (buildParams.EnableAppTransportSecurityException)
                    plist.AddDictElement("NSAppTransportSecurity", "NSAllowsArbitraryLoads", true);

#if UNITY_IOS
                // ATT 授权后可访问 IDFA，需在 PrivacyInfo.xcprivacy 声明相关收集类型（与 Firebase/AdMob 使用 OverrideBoolean 合并）
                var privacy = project.PrivacyPlist;
                var collectDataArray = PlistDocument.FindOrCreateArray(privacy.Root, "NSPrivacyCollectedDataTypes");

                // 设备ID (IDFA，用户授权 ATT 后用于广告归因与追踪)
                var devicePrivacy = PlistDocument.FindOrCreateDictInArray(collectDataArray, "NSPrivacyCollectedDataType",
                    "NSPrivacyCollectedDataTypeDeviceID");
                PlistDocument.OverrideBoolean(devicePrivacy, "NSPrivacyCollectedDataTypeLinked", false);
                PlistDocument.OverrideBoolean(devicePrivacy, "NSPrivacyCollectedDataTypeTracking", true);
                PlistDocument.AddArray(devicePrivacy, "NSPrivacyCollectedDataTypePurposes",
                    "NSPrivacyCollectedDataTypePurposeThirdPartyAdvertising",
                    "NSPrivacyCollectedDataTypePurposeDeveloperAdvertising",
                    "NSPrivacyCollectedDataTypePurposeAnalytics");

                // 广告数据 (IDFA 授权后的广告相关数据)
                var advertisingDataPrivacy = PlistDocument.FindOrCreateDictInArray(collectDataArray, "NSPrivacyCollectedDataType",
                    "NSPrivacyCollectedDataTypeAdvertisingData");
                PlistDocument.OverrideBoolean(advertisingDataPrivacy, "NSPrivacyCollectedDataTypeLinked", false);
                PlistDocument.OverrideBoolean(advertisingDataPrivacy, "NSPrivacyCollectedDataTypeTracking", true);
                PlistDocument.AddArray(advertisingDataPrivacy, "NSPrivacyCollectedDataTypePurposes",
                    "NSPrivacyCollectedDataTypePurposeThirdPartyAdvertising",
                    "NSPrivacyCollectedDataTypePurposeDeveloperAdvertising");
#endif

            }
            // 不在此处写入 NSPrivacyTracking = false：本回调在 int.MaxValue 顺序下最后执行，
            // 强制 false 会覆盖 AdMob/Firebase 等已在 PrivacyInfo.xcprivacy 中声明的追踪相关字段，
            // 造成与 NSPrivacyTrackingDomains、NSPrivacyCollectedDataTypeTracking 等不一致，违反隐私清单准确性要求。
            
            var xcodeTarget = project.UnityFramework;
            project.AddBuildProperty(xcodeTarget, "CLANG_ENABLE_MODULES", "YES");
            project.AddBuildProperty(xcodeTarget, "LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
        }


    }
}