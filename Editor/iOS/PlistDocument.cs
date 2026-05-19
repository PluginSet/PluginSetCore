#if UNITY_IOS
#define UNITY_IOS_API
#endif

namespace PluginSet.Core.Editor
{
    public class PlistDocument
    {
#if UNITY_IOS_API
        private readonly UnityEditor.iOS.Xcode.PlistDocument Document;
        
        public UnityEditor.iOS.Xcode.PlistElementDict Root => Document.root;
#endif
        
        public PlistDocument()
#if UNITY_IOS_API
            :this(new UnityEditor.iOS.Xcode.PlistDocument())
#endif
        {
        }
        
#if UNITY_IOS_API
        public PlistDocument(UnityEditor.iOS.Xcode.PlistDocument document)
        {
            Document = document;
        }
#endif

        public void Create()
        {
#if UNITY_IOS_API
            Document.Create();
#endif
        }

        public void ReadFromFile(string path)
        {
#if UNITY_IOS_API
            Document.ReadFromFile(path);
#endif
        }
        
        public void WriteToFile(string path)
        {
#if UNITY_IOS_API
            Document.WriteToFile(path);
#endif
        }
        
        
        public bool HasPlistValue(string key)
        {
#if UNITY_IOS_API
            return Root.values.ContainsKey(key);
#else
            return false;
#endif
        }

        public void RemoveElement(string key)
        {
#if UNITY_IOS_API
            Root.values.Remove(key);
#endif
        }
        
        public void AddPlistValue(string key, string value)
        {
#if UNITY_IOS_API
            if (HasPlistValue(key)) return;
            Root.SetString(key, value);
#endif
        }
        
        public void AddPlistValue(string key, bool value)
        {
#if UNITY_IOS_API
            if (HasPlistValue(key)) return;
            Root.SetBoolean(key, value);
#endif
        }
        
        public void AddPlistValue(string key, int value)
        {
#if UNITY_IOS_API
            if (HasPlistValue(key)) return;
            Root.SetInteger(key, value);
#endif
        }

        public void SetPlistValue(string key, string value)
        {
#if UNITY_IOS_API
            Root.SetString(key, value);
#endif
        }
        
        public void SetPlistValue(string key, bool value)
        {
#if UNITY_IOS_API
            Root.SetBoolean(key, value);
#endif
        }
        
        public void SetPlistValue(string key, int value)
        {
#if UNITY_IOS_API
            Root.SetInteger(key, value);
#endif
        }
        
        public void AddArrayElement(string arrayKey, params string[] values)
        {
#if UNITY_IOS_API
            AddArray(Root, arrayKey, values);
#endif
        }
        
        public void AddDictElement(string dictKey, string key, bool value)
        {
#if UNITY_IOS_API
            var dict = FindOrCreateDict(dictKey);
            dict.SetBoolean(key, value);
#endif
        }
        
        public void AddDictElement(string dictKey, string key, int value)
        {
#if UNITY_IOS_API
            var dict = FindOrCreateDict(dictKey);
            dict.SetInteger(key, value);
#endif
        }
        
        public void AddDictElement(string dictKey, string key, string value)
        {
#if UNITY_IOS_API
            var dict = FindOrCreateDict(dictKey);
            dict.SetString(key, value);
#endif
        }

        public void AddXcodeURLType(string id, string role, params string[] schemes)
        {
#if UNITY_IOS_API
            var cfBundleUrlTypes = FindOrCreateArray(Root, "CFBundleURLTypes");

            var node = cfBundleUrlTypes.values.Find(element => element.AsDict().values["CFBundleURLName"].AsString().Equals(id))?.AsDict();
            if (node == null)
            {
                node = cfBundleUrlTypes.AddDict();
            }
            
            node.SetString("CFBundleTypeRole", role);
            node.SetString("CFBundleURLName", id);
            
            var schemeList = FindOrCreateArray(node, "CFBundleURLSchemes");
            foreach (var scheme in schemes)
            {
                AddStringIfNo(schemeList, scheme);
            }
#endif
        }

        public void AddApplicationQueriesSchemes(string scheme)
        {
#if UNITY_IOS_API
            AddStringIfNo(FindOrCreateArray(Root, "LSApplicationQueriesSchemes"), scheme);
#endif
        }
        
#if UNITY_IOS_API
        public UnityEditor.iOS.Xcode.PlistElementDict FindOrCreateDict(string key)
        {
            if (Root.values.ContainsKey(key))
            {
                return Root[key].AsDict();
            }
            else
            {
                return Root.CreateDict(key);
            }
        }

        public static UnityEditor.iOS.Xcode.PlistElementDict FindOrCreateDictInArray(
            UnityEditor.iOS.Xcode.PlistElementArray array, string key, string value)
        {
            foreach (var item in array.values)
            {
                if (item is UnityEditor.iOS.Xcode.PlistElementDict dict)
                {
                    if (dict.values.ContainsKey(key) && dict[key] is UnityEditor.iOS.Xcode.PlistElementString v && value.Equals(v.value))
                    {
                        return dict;
                    }
                }
            }

            var newDict = array.AddDict();
            newDict.SetString(key, value);
            return newDict;
        }

        public static UnityEditor.iOS.Xcode.PlistElementArray FindOrCreateArray(UnityEditor.iOS.Xcode.PlistElementDict node, string key)
        {
            if (node.values.ContainsKey(key))
            {
                return node[key].AsArray();
            }
            else
            {
                return node.CreateArray(key);
            }
        }

        public static void AddStringIfNo(UnityEditor.iOS.Xcode.PlistElementArray array, string value)
        {
            if (array.values.Find(element => element.AsString().Equals(value)) == null)
            {
                array.AddString(value);
            }
        }

        public static void AddArray(UnityEditor.iOS.Xcode.PlistElementDict node, string key, params string[] values)
        {
            var array = FindOrCreateArray(node, key);
            foreach (var value in values)
            {
                AddStringIfNo(array, value);
            }
        }

        public static void OverrideBoolean(UnityEditor.iOS.Xcode.PlistElementDict node, string key, bool value)
        {
            if (value)
            {
                node.SetBoolean(key, true);
            }
            else
            {
                var item = node[key];
                if (item == null)
                    node.SetBoolean(key, false);
            }
        }
#endif
    }
}