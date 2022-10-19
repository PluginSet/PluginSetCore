using System;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace PluginSet.Core.Editor
{
    public class BuildModifyIOSProject: IBuildProcessorTask
    {
        
        [Serializable]
        private struct BuildTypeInfo
        {
            [SerializeField]
            public string teamId;
            [SerializeField]
            public string bundleId;
            [SerializeField]
            public string method;
            [SerializeField]
            public string signStyle;
            [SerializeField]
            public string profile;
            [SerializeField]
            public string cert;
            [SerializeField]
            public bool biteCode;
        }
        
        [Serializable]
        private struct BuildTypes
        {
            [SerializeField]
            public BuildTypeInfo appstore;
            [SerializeField]
            public BuildTypeInfo adHoc;
        }
        
        [Serializable]
        private struct BuildConfig
        {
            [SerializeField]
            public string info;
            [SerializeField]
            public string scheme;
            [SerializeField]
            public BuildTypes buildTypes;
        }
        
        private string IOSProjectPath;

        public BuildModifyIOSProject(string path)
        {
            IOSProjectPath = path;
        }

        public void Execute(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.iOS)
                return;
            
            context.Set("projectPath", IOSProjectPath);
            string xcodeProjectPath = Path.Combine(IOSProjectPath, "Unity-iPhone.xcodeproj", "project.pbxproj");

            var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
            var bundleStrings = bundleId.Split('.');
            var project = new PBXProjectManager(xcodeProjectPath
                , $"Unity-iPhone/{bundleStrings[bundleStrings.Length - 1]}.entitlements"
                , "Unity-iPhone");

            Global.CallCustomOrderMethods<iOSXCodeProjectModifyAttribute, BuildToolsAttribute>(context, project);

            //通用的必须加的包
            var pbxProject = project.Project;
            var targetGuid = project.UnityFramework;
            var mainTargetGuild = project.MainFramework;

//            pbxProject.AddFrameworkToProject(targetGuid, "AppTrackingTransparency.framework", false); // TODO
            // 填写个假的token  https://blog.csdn.net/yinfourever/article/details/107385530
            pbxProject.AddBuildProperty(mainTargetGuild, "USYM_UPLOAD_AUTH_TOKEN", "490e4f2bba5acc946a3eafb76c145605");
            pbxProject.AddBuildProperty(targetGuid, "USYM_UPLOAD_AUTH_TOKEN", "490e4f2bba5acc946a3eafb76c145605");

//            //推送权限处理  TODO
//            var unityIphone = pbxProject.ProjectGuid();
//            pbxProject.AddBuildProperty(unityIphone, "GCC_PREPROCESSOR_DEFINITIONS", "DISABLE_PUSH_NOTIFICATIONS=1");

            if (project.WorkSpaceSettings.HasPlistValue("BuildSystemType"))
                project.WorkSpaceSettings.root.values.Remove("BuildSystemType");

            var iosParams = context.BuildChannels.Get<IosBuildParams>();
            var teamId = iosParams.TeamID;

            var appStoreProfile = iosParams.AppStoreBuildProfile;
            if (!iosParams.AutomaticallySign && !string.IsNullOrEmpty(appStoreProfile.ProfileFile))
            {
                pbxProject.AddBuildProperty(mainTargetGuild, "CODE_SIGN_IDENTITY", "iPhone Distribution");
                pbxProject.AddBuildProperty(mainTargetGuild, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", appStoreProfile.CodeSignIdentity);
                pbxProject.AddBuildProperty(mainTargetGuild, "PROVISIONING_PROFILE_APP", appStoreProfile.ProfileId);
                pbxProject.AddBuildProperty(mainTargetGuild, "PROVISIONING_PROFILE_SPECIFIER", appStoreProfile.ProfileSpecifier);
            }
            project.Save();
            
            var buildConfig = new BuildConfig
            {
                info = "Info.plist",
                scheme = "Unity-iPhone",
                buildTypes = new BuildTypes
                {
                    appstore = new BuildTypeInfo
                        {
                            teamId = teamId,
                            bundleId = bundleId,
                            method = "app-store",
                            signStyle = iosParams.AutomaticallySign ? "automatic" : "manual",
                            profile = iosParams.AppStoreBuildProfile.ProfileId,
                            cert = iosParams.AppStoreBuildProfile.CodeSignIdentity,
                            biteCode = false,
                        },
                    adHoc = new BuildTypeInfo
                        {
                            teamId = teamId,
                            bundleId = bundleId,
                            method = "ad-hoc",
                            signStyle = iosParams.AutomaticallySign ? "automatic" : "manual",
                            profile = iosParams.AdHocBuildProfile.ProfileId,
                            cert = iosParams.AdHocBuildProfile.CodeSignIdentity,
                            biteCode = false,
                        },
                }
            };

            File.WriteAllText(Path.Combine(IOSProjectPath, "build_config.json"), JsonUtility.ToJson(buildConfig));
        }

    }
}