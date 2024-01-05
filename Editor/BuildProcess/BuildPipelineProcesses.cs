using System.IO;
using PluginSet.Platform.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public static class BuildPipelineProcesses
    {
	    private static readonly Logger Logger = LoggerManager.GetLogger("BuildPipelineProcesses");

        [PostProcessScene(-99999999)]
        public static void ResetPlayerDefaultBuildContext()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlaying)
                return;

	        Logger.Debug("ResetPlayerDefaultBuildContext 0000000000000000000000");
		        
            BuildProcessorContext context = BuildProcessorContext.Current;
            if (context == null)
            {
	            Logger.Warn("Not supported platform");
	            return;
            }

            if (context.TaskType == BuildTaskType.None)
	            return;
            
            Global.CallCustomOrderMethods<OnCompileCompleteAttribute, BuildToolsAttribute>(context);
			PlayerSettings.SplashScreen.showUnityLogo = context.BuildChannels.ShowUnityLogo;
        }

        [PostProcessBuild(-99999999 )]
        public static void BuildProcessModifyProject(BuildTarget target, string exportPath)
        {
	        Logger.Debug("BuildProcessModifyProject 0000000000000000000000");
	        var context = BuildProcessorContext.Current;
            if (context == null)
            {
	            Debug.LogWarning("Not supported platform");
	            return;
            }

            if (context.TaskType != BuildTaskType.BuildProject)
	            return;
            
	        if (context.ExportProject)
	        {
		        context.ProjectPath = exportPath;
		        
				if (target == BuildTarget.Android)
				{
#if UNITY_EDITOR_OSX
					const string gradlewFileName = "gradlew";
#else
					const string gradlewFileName = "gradlew.bat";
#endif
					string command = Path.Combine(exportPath, gradlewFileName);
					if (!File.Exists(command))
					{
						PlatformTools.CopyGradleFiles(exportPath);
					}
				}
	        }
	        
            var handler = new BuildTaskHandler();
            if (target == BuildTarget.iOS)
            {
                handler.AddNextTask(new BuildModifyIOSProject(exportPath));
            }
            else if (target == BuildTarget.WebGL)
            {
                handler.AddNextTask(new BuildModifyWebGLProject(exportPath));
            }
            handler.Execute(context);
        }
        
        [PostProcessBuild(99999999)]
        public static void BuildProcessCompleted(BuildTarget target, string exportPath)
        {
	        Logger.Debug("BuildProcessCompleted 0000000000000000000000");
	        var context = BuildProcessorContext.Current;
            if (context == null)
            {
	            Debug.LogWarning("Not supported platform");
	            return;
            }
	        
            var handler = new BuildTaskHandler();
			handler.AddNextTask(new BuildSimpleTask(delegate(BuildProcessorContext processorContext)
			{
				Global.CallCustomOrderMethods<BuildProjectCompletedAttribute, BuildToolsAttribute>(processorContext, exportPath);
			}));
            
            handler.Execute(context);
            
			context.SetBuildResult("unityVersion", Application.unityVersion);
			
#if UNITY_IOS
            context.SetBuildResult("bundleId", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            context.SetBuildResult("platform", "iOS");
#elif UNITY_ANDROID
            context.SetBuildResult("bundleId", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
            context.SetBuildResult("platform", "Android");
#elif UNITY_WEBGL
            context.SetBuildResult("bundleId", context.BuildChannels.PackageName);
            context.SetBuildResult("platform", "WebGL");
#endif

	        if (context.TaskType == BuildTaskType.BuildProject)
	        {
				if (context.ExportProject)
				{
					context.SetBuildResult("projectPath", Path.GetFullPath(exportPath));
					if (target == BuildTarget.Android)
					{
						var androidProject = new AndroidProjectManager(exportPath);
						context.SetBuildResult("targetSdkVersion", androidProject.LauncherGradle.TargetSdkVersion);
					}
				}
				else
				{
					context.SetBuildResult("apkPath", Path.GetFullPath(exportPath));
				}
	        }
        }
    }
}
