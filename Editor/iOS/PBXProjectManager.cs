using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using UnityEditor.iOS.Xcode;

namespace PluginSet.Core.Editor
{
    public class PBXProjectManager : ProjectCapabilityManager
    {
        public PBXProject Project;
        public PlistDocument PlistDocument;
        public PlistDocument WorkSpaceSettings;
        public PlistDocument Entitlements;

        public string ProjectPath => m_PbxProjectPath;

        private string m_PbxProjectPath;
        
#if UNITY_2019_3_OR_NEWER
        // 2019.3以上的新接口
        public string MainFramework => Project.GetUnityMainTargetGuid();
        public string UnityFramework => Project.GetUnityFrameworkTargetGuid();
#else
        public string MainFramework => Project.TargetGuidByName("Unity-iPhone");
        public string UnityFramework => Project.TargetGuidByName("Unity-iPhone");
#endif
        
        public PBXProjectManager(string pbxProjectPath,
            string entitlementFilePath,
            string targetName)
            : base(pbxProjectPath, entitlementFilePath, targetName)
        {
            m_PbxProjectPath = pbxProjectPath;
            Project = this.project;
            var getOrCreateInfoDoc = typeof(ProjectCapabilityManager)
                .GetMethod("GetOrCreateInfoDoc", BindingFlags.Instance | BindingFlags.NonPublic);
            var getOrCreateEntitlementDoc = typeof(ProjectCapabilityManager)
                .GetMethod("GetOrCreateEntitlementDoc", BindingFlags.Instance | BindingFlags.NonPublic);
            PlistDocument = getOrCreateInfoDoc?.Invoke(this, null) as PlistDocument;
            Entitlements = getOrCreateEntitlementDoc?.Invoke(this, null) as PlistDocument;
            
            WorkSpaceSettings = new PlistDocument();
            var workSpaceSettingPath = Path.Combine(pbxProjectPath, "..", "project.xcworkspace", "xcshareddata", "WorkspaceSettings.xcsettings");
            if (File.Exists(workSpaceSettingPath))
                WorkSpaceSettings.ReadFromFile(workSpaceSettingPath);
            else
                WorkSpaceSettings.Create();
        }

        public void Save()
        {
            var parentPath = Path.Combine(m_PbxProjectPath, "..", "project.xcworkspace", "xcshareddata");
            if (!Directory.Exists(parentPath))
                Directory.CreateDirectory(parentPath);
            
            var workSpaceSettingPath = Path.Combine(parentPath, "WorkspaceSettings.xcsettings");
            WorkSpaceSettings.WriteToFile(workSpaceSettingPath);
            
            WriteToFile();
        }

        public bool TryAddCapability(
            string targetGuid,
            PBXCapabilityType capability,
            string entitlementsFilePath = null,
            bool addOptionalFramework = false)
        {
            try
            {
                return project.AddCapability(targetGuid, capability, entitlementsFilePath, addOptionalFramework);
            }
            catch (WarningException e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        public void AddAssociatedDomainsApplinks(params string[] links)
        {
            TryAddCapability(MainFramework, PBXCapabilityType.AssociatedDomains);
            const string domainKey = "com.apple.developer.associated-domains";
            PlistElementArray domains = Entitlements.root.FindOrCreateArray(domainKey);
            foreach (var link in links)
            {
                domains.AddStringIfNo($"applinks:{link}");
            }
        }
        
    }
}