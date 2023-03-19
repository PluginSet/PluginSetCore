using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginSetConfigAttribute : Attribute
    {
        public string Key;

        public PluginSetConfigAttribute(string key)
        {
            Key = key;
        }
    }


    public class PluginSetConfig : SerializedDataSet
    {
        private static PluginSetConfig _instance;

        public static PluginSetConfig Asset
        {
            get
            {
                if (_instance == null)
                {
                    _instance = SettingAssetLoader.MainSettingLoader.GetMain<PluginSetConfig>();
                }

                return _instance;
            }
        }

#if UNITY_EDITOR
        public static PluginSetConfig NewAsset
        {
            get
            {
                SettingAssetLoader.MainSettingLoader.RemoveMainAsset<PluginSetConfig>();
                return Asset;
            }
        }
#endif


        private static List<SerializedType> pluginSetSerializedTypes;

        public override IEnumerable<SerializedType> SerializedTypes
        {
            get
            {
                if (pluginSetSerializedTypes != null)
                    return pluginSetSerializedTypes;

                pluginSetSerializedTypes = new List<SerializedType>();

                var list = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    where type.IsDefined(typeof(PluginSetConfigAttribute), false)
                    select type;

                foreach (var type in list)
                {
                    foreach (var attr in type.GetCustomAttributes(typeof(PluginSetConfigAttribute), false))
                    {
                        var paramAttr = (PluginSetConfigAttribute) attr;
                        pluginSetSerializedTypes.Add(SerializedType.Create(paramAttr.Key, type, string.Empty));
                    }
                }

                return pluginSetSerializedTypes;
            }
        }
    }
}
