using UnityEngine;
using UnityEngine.Assertions;

namespace PluginSet.Core.Editor
{
    public static class SerializedDataSetExtension
    {
        public static T Get<T>(this SerializedDataSet dataSet) where T : ScriptableObject
        {
            var type = typeof(T);
            var attributes = type.GetCustomAttributes(typeof(BuildChannelsParamsAttribute), false);
            if (attributes.Length == 1)
            {
                var attribute = (BuildChannelsParamsAttribute) attributes[0];
                return dataSet.Get<T>(attribute.Key);
            }

            attributes = type.GetCustomAttributes(typeof(PluginSetConfigAttribute), false);
            Assert.IsTrue(attributes.Length == 1, $"Expect {type} to have exact 1 PluginSetConfigAttribute, got {attributes.Length}");
            var attribute2 = (PluginSetConfigAttribute) attributes[0];
            return dataSet.Get<T>(attribute2.Key);
        }
    }
}
