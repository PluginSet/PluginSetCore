using System.IO;
using UnityEditor.Android;

namespace PluginSet.Core.Editor
{
	public class GradlePropertiesBuildProcessor : IPostGenerateGradleAndroidProject
	{
		public int callbackOrder => 0;

		public void OnPostGenerateGradleAndroidProject(string androidProjectPath)
		{
			var handler = new BuildTaskHandler();
			var projectPath = Path.Combine(androidProjectPath, "..");
			handler.AddNextTask(new BuildModifyAndroidProject(projectPath));
			handler.Execute(BuildProcessorContext.Current);
		}
	}
}