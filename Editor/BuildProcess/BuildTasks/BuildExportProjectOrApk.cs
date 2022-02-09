using System.IO;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class BuildExportProjectOrApk: IBuildProcessorTask
    {
        public void Execute(BuildProcessorContext context)
        {
	        var streamingAssetsPath = context.TryGet<string>("StreamingAssetsPath", null);
	        if (!string.IsNullOrEmpty(streamingAssetsPath))
	        {
				Global.ClearDirectory(streamingAssetsPath, "*.DS_Store");
				Global.ClearDirectory(streamingAssetsPath, "*.meta");
				Global.ClearDirectory(streamingAssetsPath, "*.manifest");
				AssetDatabase.Refresh();
	        }
		        
			BuildOptions buildOption = BuildOptions.None;

			bool debugMode = context.DebugMode;
			if (debugMode)
			{
				buildOption |= BuildOptions.Development;
				buildOption |= BuildOptions.AllowDebugging;
//				buildOption |= BuildOptions.ConnectWithProfiler;
			}
			else
			{
				buildOption |= BuildOptions.None;
			}

			var target = context.BuildTarget;
			var buildPath = context.ProjectPath;
			string locationPath = buildPath;
			if (target == BuildTarget.iOS)
			{
				// 导出目录保持一致
#if !UNITY_2020_1_OR_NEWER
				locationPath = Path.Combine(buildPath , PlayerSettings.productName);
#endif
				EditorUserBuildSettings.iOSBuildConfigType = debugMode ? iOSBuildType.Debug : iOSBuildType.Release;
			}
			else if (target == BuildTarget.Android)
			{
				if (!context.ExportProject)
				{
					locationPath = Path.Combine(buildPath, "android.apk");
				}
				EditorUserBuildSettings.androidBuildType =
					debugMode ? AndroidBuildType.Debug : AndroidBuildType.Release;
				EditorUserBuildSettings.exportAsGoogleAndroidProject = context.ExportProject;
			}
			else
			{
				throw new BuildException($"Unsupported build target {target}");
			}
			
			Debug.Log("local path::::::::::: " + locationPath);
			
			if (context.ExportProject)
			{
				Global.CheckAndDeletePath(locationPath);
				if (!Directory.Exists(locationPath))
					Directory.CreateDirectory(locationPath);
			}

			//关闭unity logo
			PlayerSettings.SplashScreen.showUnityLogo = false;
			EditorUserBuildSettings.development = debugMode;
#if UNITY_2018_4_OR_NEWER && (!UNITY_2019_1_OR_NEWER || UNITY_2019_2_OR_NEWER)
			EditorUserBuildSettings.androidCreateSymbolsZip = context.ProductMode;
#endif
			BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, locationPath, target, buildOption);
			
			if (context.ExportProject)
			{
#if UNITY_ANDROID
#if UNITY_EDITOR_OSX
				const string gradlewFileName = "gradlew";
#else
				const string gradlewFileName = "gradlew.bat";
#endif
				string command = Path.Combine(locationPath, gradlewFileName);
				if (!File.Exists(command))
				{
					var corePath = Global.GetPackageFullPath("com.pluginset.core");
					Global.CopyFilesTo( locationPath, Path.Combine(corePath, "AndroidTools~"), "*");
				}
#endif
				
				var md5FileName = context.TryGet<string>("md5FileName", null);
				var md5Context = context.TryGet<string>("md5Context", null);
				if (!string.IsNullOrEmpty(md5FileName) && !string.IsNullOrEmpty(md5Context))
					File.WriteAllText(md5FileName, md5Context);
			}
        }
    }
}