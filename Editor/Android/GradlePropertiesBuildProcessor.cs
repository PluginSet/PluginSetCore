using System.IO;
using UnityEditor.Android;

namespace PluginSet.Core.Editor
{
	public class GradlePropertiesBuildProcessor : IPostGenerateGradleAndroidProject
	{
		public int callbackOrder => 0;

		public void OnPostGenerateGradleAndroidProject(string androidProjectPath)
		{
			if (BuildProcessorContext.Current == null)
			{
				return;
			}
			
			var handler = new BuildTaskHandler();
			handler.AddNextTask(new BuildModifyAndroidProject(Path.Combine(androidProjectPath, "..")));
			handler.Execute(BuildProcessorContext.Current);
		}
	}
}