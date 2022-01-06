using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class DefaultPropertyDrawer: CustomPropertyBaseDrawer
    {
        protected override void PropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}