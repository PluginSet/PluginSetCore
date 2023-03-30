using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PluginSet.Core
{
    public static class PluginUtil
    {
        private const int MaxVersionSegment = 4;
        
        public static string VersionName => Application.version;
        
        public static int VersionCode => Devices.GetVersionCode();
        
        public static string AppVersion => GetVersionString(VersionName, VersionCode);

        public static string GetVersionString(string name, int code)
        {
            return $"v{name}-{code}";
        }

        public static bool SplitVersionString(string versionString, out string name, out string code)
        {
            name = "0";
            code = "0";
            
            versionString = versionString.ToLower();
            if (!versionString.StartsWith("v"))
            {
                Debug.LogWarning("Build string need start with 'v'");
                return false;
            }
            
            int pos = versionString.LastIndexOf('-');
            if (pos < 0)
            {
                Debug.Log("Build string need contains '-'");
                name = versionString.Substring(1);
            }
            else
            {
                name = versionString.Substring(1, pos - 1);
                code = versionString.Substring(pos + 1);
            }

            return true;
        }

        public static int ParseVersionNumber(string version)
        {
            int versionNum = 0;
            string[] versions = version.Split('.');
            for (int i = 0; i < MaxVersionSegment; i++)
            {
                int val = 0;
                if (i < versions.Length)
                {
                    if (!int.TryParse(versions[i], out val))
                    {
                        val = 0;
                    }
                }
                versionNum *= 100;
                versionNum += val;
            }

            return versionNum;
        }

        private static string Md5BytesToString(byte[] bytes)
        {
            var buffer = new StringBuilder();
            var len = bytes.Length;
            for (int i = 0; i < len; i++)
            {
                buffer.Append(Convert.ToString(bytes[i], 16).PadLeft(2, '0'));
            }

            return buffer.ToString().PadLeft(32, '0');
        }
        
        
        public static string GetMd5(byte[] data)
        {
            var md5 = MD5.Create();
            var bytes = md5.ComputeHash(data);
            return Md5BytesToString(bytes);
        }

        public static string GetMd5(byte[] data, int offset, int count)
        {
            var md5 = MD5.Create();
            var bytes = md5.ComputeHash(data, offset, count);
            return Md5BytesToString(bytes);
        }

        public static string GetMd5(string content)
        {
            return GetMd5(Encoding.UTF8.GetBytes(content));
        }

        public static string GetFileMd5(string filePath)
        {
            return GetMd5(File.ReadAllBytes(filePath));
        }

        private static readonly char[] AnalyticsSpecialStart = new[]
        {
            '#',  // ThinkingAnalytics
        };
        public static bool FilterAnalyticsEventName(string eventName, params char[] ignores)
        {
            if (string.IsNullOrEmpty(eventName))
                return true;
            
            var firstChar = eventName[0];
            if (ignores.Contains(firstChar))
                return false;
            
            if (AnalyticsSpecialStart.Contains(firstChar))
                return true;
            
            return false;
        }
        
        public static Color ConvertFromHtmlColor(string str)
        {
            if (str.Length < 7 || str[0] != '#')
                return Color.black;

            if (str.Length == 9)
            {
                //optimize:avoid using Convert.ToByte and Substring
                //return new Color32(Convert.ToByte(str.Substring(3, 2), 16), Convert.ToByte(str.Substring(5, 2), 16),
                //  Convert.ToByte(str.Substring(7, 2), 16), Convert.ToByte(str.Substring(1, 2), 16));

                return new Color32((byte)(CharToHex(str[3]) * 16 + CharToHex(str[4])),
                    (byte)(CharToHex(str[5]) * 16 + CharToHex(str[6])),
                    (byte)(CharToHex(str[7]) * 16 + CharToHex(str[8])),
                    (byte)(CharToHex(str[1]) * 16 + CharToHex(str[2])));
            }
            else
            {
                //return new Color32(Convert.ToByte(str.Substring(1, 2), 16), Convert.ToByte(str.Substring(3, 2), 16),
                //Convert.ToByte(str.Substring(5, 2), 16), 255);

                return new Color32((byte)(CharToHex(str[1]) * 16 + CharToHex(str[2])),
                    (byte)(CharToHex(str[3]) * 16 + CharToHex(str[4])),
                    (byte)(CharToHex(str[5]) * 16 + CharToHex(str[6])),
                    255);
            }
        }
        
        public static int CharToHex(char c)
        {
            if (c >= '0' && c <= '9')
                return (int)c - 48;
            if (c >= 'A' && c <= 'F')
                return 10 + (int)c - 65;
            else if (c >= 'a' && c <= 'f')
                return 10 + (int)c - 97;
            else
                return 0;
        }
        
        private static readonly char[] HexDigits = new[]
            {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

        public static string CreateUUID()
        {
            char[] s = new char[36];
            for (int i = 0; i < 36; i++)
            {
                s[i] = HexDigits[Random.Range(0, 15)];
            }

            // bits 12-15 of the time_hi_and_version field to 0010
            s[14] = '4';
            
            // bits 6-7 of the clock_seq_hi_and_reserved to 01
            s[19] = HexDigits[(s[19] & 0x3) | 0x8];

            s[8] = s[13] = s[18] = s[23] = '-';

            var uuid = string.Join("", s);
            return uuid; 
        }
    }
}