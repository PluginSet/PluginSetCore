using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#if false
namespace PluginSet.Core.Editor
{
    public static class BuildPipelineProcesses
    {
	    private static readonly Logger Logger = LoggerManager.GetLogger("BuildPipelineProcesses");

        [PostProcessScene(-99999999)]
        public static void ResetPlayerDefaultBuildContext()
        {
	        Logger.Debug("ResetPlayerDefaultBuildContext 0000000000000000000000");
		        
            if (EditorApplication.isCompiling || EditorApplication.isPlaying)
                return;

            BuildProcessorContext context = BuildProcessorContext.Current;
            if (context == null)
            {
	            Logger.Warn("Not supported platform");
	            return;
            }
	            
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
            
	        if (context.ExportProject)
	        {
		        context.ProjectPath = exportPath;
	        }
	        
            var handler = new BuildTaskHandler();
            if (target == BuildTarget.iOS)
            {
                handler.AddNextTask(new BuildModifyIOSProject(exportPath));
            }
            handler.Execute(BuildProcessorContext.Current);
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
            
			context.SetBuildResult("unityVersion", Application.unityVersion);
			
#if UNITY_IOS
            context.SetBuildResult("bundleId", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS));
            context.SetBuildResult("platform", "iOS");
#elif UNITY_ANDROID
            context.SetBuildResult("bundleId", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
            context.SetBuildResult("platform", "Android");
#elif UNITY_WEBGL
            context.SetBuildResult("platform", "WebGL");
#endif
			
			if (context.ExportProject)
			{
				context.SetBuildResult("projectPath", Path.GetFullPath(exportPath));
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
						var corePath = Global.GetPackageFullPath("com.pluginset.core");
						Global.CopyFilesTo( exportPath, Path.Combine(corePath, "AndroidTools~"), "*");
					}
					
					var androidProject = new AndroidProjectManager(exportPath);
					context.SetBuildResult("targetSdkVersion", androidProject.LauncherGradle.TargetSdkVersion);
				}
				
				var md5FileName = context.TryGet<string>("md5FileName", null);
				var md5Context = context.TryGet<string>("md5Context", null);
				if (!string.IsNullOrEmpty(md5FileName) && !string.IsNullOrEmpty(md5Context))
					File.WriteAllText(md5FileName, md5Context);
			}
			else
			{
				context.SetBuildResult("apkPath", Path.GetFullPath(exportPath));
			}
            
            if (!Application.isBatchMode)
            {
				string tag = context.BuildExportProjectTag();
				string md5FileName = Path.Combine(context.ProjectPath, "channelMd5.txt");
				md5FileName = context.TryGet("md5FileName", md5FileName);
				tag = context.TryGet("md5Context", tag);
				
				context.Set("md5FileName", md5FileName);
				context.Set("md5Context", tag);
				
				var handler = new BuildTaskHandler();
				handler.AddNextTask(new BuildEnd());
				handler.Execute(context);
            }
        }
    }
}
#endif