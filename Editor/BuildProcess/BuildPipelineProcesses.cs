using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
						CopyGradleFiles(exportPath);
					}
					
					// 找到align工具和sign工具
					var buildToolsPath = GetLatestBuildTools();
					if (!string.IsNullOrEmpty(buildToolsPath))
					{
						var signTool = Path.Combine(buildToolsPath, "apksigner");
						var alignTool = Path.Combine(buildToolsPath, "zipalign");
#if UNITY_EDITOR_WIN
						signTool += ".bat";
						alignTool += ".exe";
#endif
						context.SetBuildResult("ApkSignTool", signTool);
						context.SetBuildResult("ApkAlignTool", alignTool);
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
    
		public static void CopyGradleFiles(string targetPath)
		{
			var corePath = Global.GetPackageFullPath("com.pluginset.core");
			var toolsPath = Path.Combine(corePath, "AndroidTools~");
			Global.CopyFilesTo( targetPath, toolsPath, "*", SearchOption.TopDirectoryOnly);

			var wrapperPath = Path.Combine(toolsPath, "gradle", "wrapper");
			var srcPath = Path.Combine(targetPath, "gradle", "wrapper");
			Global.CopyFilesTo( srcPath, wrapperPath, "*", SearchOption.TopDirectoryOnly);

#if UNITY_2020_3_OR_NEWER
			var gradleVersion = "gradle-6.1.1-bin";
#else
			var gradleVersion = "gradle-5.6.4-bin";
#endif
			var editorGradleVersion = GetEditorGradleVersion();
			if (!string.IsNullOrEmpty(editorGradleVersion))
				gradleVersion = $"gradle-{editorGradleVersion}-bin";
			var propertiesFile = Path.Combine(srcPath, "gradle-wrapper.properties");
			var properties = File.ReadAllLines(propertiesFile);
			properties[properties.Length - 1] = $"distributionUrl=https\\://services.gradle.org/distributions/{gradleVersion}.zip";
			File.WriteAllLines(propertiesFile, properties);
				
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				Global.SetFileExecutable(Path.GetFullPath(Path.Combine(targetPath, "gradlew")).Replace("\\", "/"));
			}
		}
			
		private static string GetEditorGradleVersion()
		{
			string path = EditorPrefs.GetString("GradlePath");
			if (EditorPrefs.HasKey("GradleUseEmbedded") && EditorPrefs.GetBool("GradleUseEmbedded") || string.IsNullOrEmpty(path))
			{
#if UNITY_EDITOR_OSX
				path = Path.Combine(Path.GetDirectoryName(EditorApplication.applicationPath), "PlaybackEngines", "AndroidPlayer", "Tools", "gradle");
#else
				path = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer", "Tools", "gradle");
#endif
			}

			path = Path.Combine(path, "lib");
			foreach (var file in Directory.GetFiles(path, "gradle-*.jar"))
			{
				var fileName = Path.GetFileNameWithoutExtension(file);
				var match = Regex.Match(fileName, "gradle(-\\w+)+-([0-9]+\\.[0-9]+(\\.[0-9]+)?)");
				if (match.Groups.Count >= 3)
				{
					return match.Groups[2].Value;
				}
			}

			return null;
		}

		public static string GetLatestBuildTools()
		{
			
#if UNITY_EDITOR_OSX
			var buildToolsPath = Path.Combine(Path.GetDirectoryName(EditorApplication.applicationPath), "PlaybackEngines", "AndroidPlayer", "SDK", "build-tools");
#else
			var buildToolsPath = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer", "SDK", "build-tools");
#endif
			var latestVersion = Global.FindHighestVersionDirectory(buildToolsPath);
			if (!string.IsNullOrEmpty(latestVersion))
			{
				return Path.Combine(buildToolsPath, latestVersion);
			}

			return null;
		}
    }
}
