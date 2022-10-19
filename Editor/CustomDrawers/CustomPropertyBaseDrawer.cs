using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public abstract class CustomPropertyBaseDrawer: PropertyDrawer
    {
        protected abstract void PropertyField(Rect position, SerializedProperty property, GUIContent label);
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                foreach (var attr in fieldInfo.GetCustomAttributes(typeof(LogicPropertyAttribute), true))
                {
                    ((LogicPropertyAttribute) attr).BeginProperty(position, property, label);
                }
                
                PropertyField(position, property, label);
                
                foreach (var attr in fieldInfo.GetCustomAttributes(typeof(LogicPropertyAttribute), true))
                {
                    ((LogicPropertyAttribute) attr).EndProperty();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                if (property.hasVisibleChildren && property.isExpanded)
                    return property.CountInProperty() * EditorGUIUtility.singleLineHeight + 10f;
                else
                    return EditorGUIUtility.singleLineHeight;
            }
            
            return 0f;
        }

        private bool IsVisible(SerializedProperty property)
        {
            return VisiblePropertyAttribute.IsVisible(fieldInfo, property.serializedObject);
        }
    }
}