using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    [Serializable]
    public struct BuildProvisioningProfile
    {
        private static readonly string s_PatternTeamID = "<key>TeamIdentifier<\\/key>[\n\t]*<array>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternUUID = "<key>UUID<\\/key>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternSpecifier = "<key>Name<\\/key>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternTeamName = "<key>TeamName<\\/key>[\n\t]*<string>((\\w*\\-?){5})";
        private static readonly string s_PatternDeveloperCertificates = "<key>DeveloperCertificates<\\/key>[\n\t]*<array>[\n\t]*<data>([\\w\\/+=]+)<\\/data>";
        private static readonly string s_DistributionPattern = "iPhone Distribution: ";

        private static void ParseFile(string filePath, out string profileId, out string profileSpecifier, out string codeSignIdentity, out ProvisioningProfileType type)
        {
            string input = File.ReadAllText(filePath);
            Match match1 = Regex.Match(input, s_PatternUUID, RegexOptions.Singleline);
            if (match1.Success)
                profileId = match1.Groups[1].Value;
            else
                profileId = string.Empty;
            
            Match match2 = Regex.Match(input, s_PatternDeveloperCertificates, RegexOptions.Singleline);
            if (match2.Success)
                type = !Encoding.UTF8.GetString(Convert.FromBase64String(match2.Groups[1].Value))
                    .Contains(s_DistributionPattern)
                    ? ProvisioningProfileType.Development
                    : ProvisioningProfileType.Distribution;
            else
                type = ProvisioningProfileType.Automatic;
            
            Match match3 = Regex.Match(input, s_PatternSpecifier, RegexOptions.Singleline);
            if (match3.Success)
                profileSpecifier = match1.Groups[1].Value;
            else
                profileSpecifier = string.Empty;

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
        }
        
        internal static void OnUpdateProfile(ref BuildProvisioningProfile profile)
        {
            if (string.IsNullOrEmpty(profile.ProfileFile))
                return;
            
            ParseFile(Path.Combine(".", profile.ProfileFile), out profile.ProfileId, out profile.ProfileSpecifier, out profile.CodeSignIdentity, out profile.ProfileType);
        }
        
        [Tooltip("签名文件ID")]
        [BrowserFile("选择文件", "mobileprovision")]
        [SerializeField]
        public string ProfileFile;
        
        [DisableEdit]
        [SerializeField]
        public string ProfileId;
        
        [DisableEdit]
        [SerializeField]
        public ProvisioningProfileType ProfileType;
        
        [DisableEdit]
        [SerializeField]
        public string ProfileSpecifier;
        
        [DisableEdit]
        [SerializeField]
        public string CodeSignIdentity;
    }
}