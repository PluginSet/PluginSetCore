#if UNITY_IOS
#define ENABLE
#endif
#if ENABLE
using System.IO;
using UnityEditor;
#endif

namespace PluginSet.Core.Editor
{
    public class BuildModifyIOSProject: IBuildProcessorTask
    {
        private string IOSProjectPath;

        public BuildModifyIOSProject(string path)
        {
            IOSProjectPath = path;
        }
        
        public void Execute(BuildProcessorContext context)
        {
#if ENABLE
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

            project.Save();
            
            Global.CallCustomOrderMethods<iOSXCodeProjectModifyCompletedAttribute, BuildToolsAttribute>(context, IOSProjectPath);
#endif
        }

    }
}