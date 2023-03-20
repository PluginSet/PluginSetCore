using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PluginSet.Core
{
    public abstract class ResourcesManager
    {
        public static T NewInstance<T>() where T: ResourcesManager
        {
            if (Instance != null) return (T)Instance;
            
            var inst = Activator.CreateInstance<T>();
            Instance = inst;
            Instance.OnCreate();
            return inst;
        }

        public static void PurgeInstance()
        {
            Instance = null;
        }

        public static string PatchesWriteFlag = Application.version.Replace(".", "_");
        
        public static readonly string PatchesRootPath = Path.Combine( Application.persistentDataPath, "patches");
#if UNITY_EDITOR
        public static readonly string PatchesSavePath = Application.streamingAssetsPath;
#else
        public static readonly string PatchesSavePath = Path.Combine(
            PatchesRootPath,
            PatchesWriteFlag
        );
#endif
        
        // streamingAssetsPath 的URL链接地前缀
        public static string StreamingInternalUrlPrefix
        {
            get
            {
#if UNITY_EDITOR_OSX
            return "file://" + Application.streamingAssetsPath + "/";
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
			return "file:///" + Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID
            return Application.streamingAssetsPath + "/";
#elif UNITY_WEBGL
            return Application.streamingAssetsPath + "/";
#else
            return "file://" + Application.streamingAssetsPath + "/";
#endif
            }
        }
        
        // 暂时standalone平台只支持windows的热更新
        public static string PatchesSavePathUrlPrefix
        {
            get
            {
#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR_OSX || UNITY_WEBGL
                return "file://" + PatchesSavePath + "/";
#elif UNITY_EDITOR
                return "file:///" + PatchesSavePath + "/";
#else
                return  PatchesSavePath + "/";
#endif
            }
        }

        
        public static string ResourceVersion { get; protected set; }
        
        public static ResourcesManager Instance { get; protected set; }
        
        public readonly List<string> SearchPaths = new List<string>();

        public virtual string RunningVersion => ResourceVersion;
        
        protected virtual void OnCreate() {}
        
        public abstract string StreamingAssetsName { get; }

        public virtual void AddSearchPath(string searchPath, bool front = false)
        {
            if (front)
                SearchPaths.Insert(0, searchPath);
            else
                SearchPaths.Add(searchPath);
        }

        public virtual void RemoveSearchPath(string searchPath)
        {
            SearchPaths.Remove(searchPath);
        }

        public abstract bool ExistsBundle(string bundleName);
        
        public abstract bool ExistsAsset(string bundleName, string assetName);

        public virtual T Load<T>(string path) where T : Object
        {
            return Load(path, typeof(T)) as T;
        }

        public abstract AsyncOperationHandle<T> LoadAsync<T>(string path) where T : Object;

        public abstract Object Load(string path, Type type);
        
        public abstract AsyncOperationHandle<Object> LoadAsync(string path, Type type);
        
        public abstract AssetBundle LoadBundle(string bundleName);

        public abstract AsyncOperationHandle<AssetBundle> LoadBundleAsync(string bundleName);
        
        public abstract void ReleaseBundle(AssetBundle bundle);

        public abstract void DontReleaseBundle(AssetBundle bundle);

        public abstract void ReleaseBundle(string bundleName);

        public abstract void DontReleaseBundle(string bundleName);

        public abstract void ReleaseAll();

        public virtual T LoadAsset<T>(string bundleName, string assetName) where T: Object
        {
            return LoadAsset(bundleName, assetName, typeof(T)) as T;
        }

        public abstract Object LoadAsset(string bundleName, string assetName, Type type);

        public abstract AsyncOperationHandle<T> LoadAssetAsync<T>(string bundleName, string assetName) where T : Object;

        public abstract AsyncOperationHandle<Object> LoadAssetAsync(string bundleName, string assetName, Type type);

        public virtual T[] LoadAllAssets<T>(string bundleName) where T : Object
        {
            return LoadAllAssets(bundleName, typeof(T)) as T[];
        }

        public abstract Object[] LoadAllAssets(string bundleName, Type type);

        public abstract AsyncOperationHandle<T[]> LoadAllAssetsAsync<T>(string bundleName) where T : Object;

        public abstract AsyncOperationHandle<Object[]> LoadAllAssetsAsync(string bundleName, Type type);

        public virtual byte[] LoadLuaBytes(string assetName)
        {
            return null;
        }

#if UNITY_EDITOR
        public virtual bool IsValidAssetFile(string file)
        {
            return System.IO.File.Exists(file);
        }
#endif
    }
}