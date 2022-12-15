using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ItemObjectType = UnityEngine.ScriptableObject;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;

#endif

namespace PluginSet.Core
{
    public class SerializedType
    {
        public static SerializedType Create<T>(string key, string toolTips = null) where T: ItemObjectType
        {
            return Create(key, typeof(T), toolTips);
        }
        
        public static SerializedType Create(string key, Type type, string toolTips = null)
        {
            return new SerializedType(key, type, toolTips);
        }
        
        private SerializedType(string key, Type type, string toolTips)
        {
            Key = key;
            ClassType = type;
            ToolTips = toolTips;
        }

        public string Key;
        public Type ClassType;
        public string ToolTips;
    }
    

    [Serializable]
    public class SerializedDataItem
    {
        [SerializeField]
        public string Key;

        [SerializeField]
        public string ClassID;
        
        [SerializeField]
        public ItemObjectType Data;

        [NonSerialized]
        public string ToolTips;
        
        [NonSerialized]
        public object serializedObject;

#if UNITY_EDITOR
        public override string ToString()
        {
            return $"{Key}:{ClassID}@[{AssetDatabase.GetAssetPath(Data)}]";
        }
#endif
    }
    
    public abstract class SerializedDataSet: ScriptableObject, ISettingAsset
    {
        [HideInInspector]
        public List<SerializedDataItem> DataItems = new List<SerializedDataItem>();

        public abstract IEnumerable<SerializedType> SerializedTypes { get; }

        private Dictionary<string, SerializedDataItem> _itemsMap;
        private Dictionary<string, SerializedDataItem> ItemsMap
        {
            get
            {
                if (_itemsMap == null)
                {
                    DataItems.Clear();
                    CheckDataItems();
                }

                return _itemsMap;
            }
        }

        private bool _isLoading;

        public void OnLoad()
        {
            CheckDataItems();
        }

        public void Reload()
        {
            DataItems.Clear();
            CheckDataItems();
        }

        public void CheckDataItems()
        {
            if (_isLoading)
                return;

            _isLoading = true;
            
            List<string> validKeys = new List<string>();
            Dictionary<string, Type> validTypes = new Dictionary<string, Type>();
            foreach (var info in SerializedTypes)
            {
                validKeys.Add(info.Key);
                validTypes[GetTypeId(info.ClassType)] = info.ClassType;
            }
            
            if (_itemsMap == null)
                _itemsMap = new Dictionary<string, SerializedDataItem>();
            else
                _itemsMap.Clear();
            
            foreach (var item in DataItems)
            {
                if (validKeys.Contains(item.Key) && validTypes.TryGetValue(item.ClassID, out var type))
                {
                    _itemsMap.Add(item.Key, item);
                }
            }

#if UNITY_EDITOR
        try
        {
            var path = AssetDatabase.GetAssetPath(this);
            var subAssets = new List<Object>(AssetDatabase.LoadAllAssetRepresentationsAtPath(path));
            bool dirty = false;
#endif
            foreach (var info in SerializedTypes)
            {
                var key = info.Key;
                if (_itemsMap.ContainsKey(key))
                    continue;


#if UNITY_EDITOR
                subAssets.RemoveAll(o => o == null);
                var subAsset = subAssets.Find(val => val.name.Equals(key));
                if (subAsset != null)
                {
                    var item = new SerializedDataItem();
                    item.Key = key;
                    item.ClassID = GetTypeId(subAsset.GetType());
                    item.ToolTips = info.ToolTips;
                    item.Data = (ItemObjectType) subAsset;
                    
                    DataItems.Add(item);
                    _itemsMap.Add(key, item);
                    
                    Debug.Log("========== add back item " + item);
                    continue;
                }
                
                dirty = true;
#endif
                Add(key, info.ClassType, info.ToolTips);
            }

#if UNITY_EDITOR
            if (dirty)
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
            }
        }
        finally
        {
            _isLoading = false;
        }

#endif
            _isLoading = false;
        }

        /// <summary>
        /// 推荐使用不带参数的版本，自动从 T 的 Attribute 里面获取 key。免得手动填错了。
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string key) where T: ItemObjectType
        {
            if (ItemsMap == null)
            {
                return default(T);
            }
            
            if (ItemsMap.TryGetValue(key, out var existItem))
            {
                return (T) existItem.Data;
            }

            Debug.LogError($"SerializedDataSet has no data with key:{key}!");
            return default(T);
        }

        public T TryGet<T>(string key, T defaultValue) where T: ItemObjectType
        {
            if (ItemsMap.TryGetValue(key, out var existItem))
            {
                return (T) existItem.Data;
            }

            return defaultValue;
        }

        public SerializedDataItem GetItem(string key)
        {
            return ItemsMap[key];
        }

        private SerializedDataItem Add(string key, Type type, string toolTips = null)
        {
            var item = new SerializedDataItem();
            item.Key = key;
            item.ClassID = GetTypeId(type);
            item.ToolTips = toolTips;
            var data = ScriptableObject.CreateInstance(type);
            data.name = key;
            item.Data = (ItemObjectType) data;
            
            DataItems.Add(item);
            ItemsMap.Add(key, item);
            
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.AddObjectToAsset(data, path);
            Debug.Log($"================== Add {key} data({toolTips}) to {path}");
#endif
            return item;
        }
        
        private static string GetTypeId(Type type)
        {
            return type.FullName ?? type.Name;
        }
        
#if UNITY_EDITOR
        public virtual void OnChildChanged()
        {
            
        }
#endif
    }
}