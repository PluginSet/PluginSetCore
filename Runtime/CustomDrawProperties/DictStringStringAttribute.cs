using System;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#endif

namespace PluginSet.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DictStringStringAttribute: DrawablePropertyAttribute
    {
#if UNITY_EDITOR
        private bool _layouted = false;
        
        public override void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            
            if (Event.current.type == EventType.Repaint && !_layouted)
                return;

            if (Event.current.type == EventType.Layout)
                _layouted = true;
            else
                _layouted = false;
            
            var pairs = property.FindPropertyRelative("Pairs");
            if (pairs == null || !pairs.isArray)
                return;
            
            var size = pairs.arraySize;
            pairs.isExpanded = EditorGUILayout.Foldout(pairs.isExpanded, $"{label.tooltip ?? label.text}:Count = {size}");
            
            if (pairs.isExpanded)
            {
                EditorGUI.BeginChangeCheck();
                size = EditorGUILayout.IntField("Count", size);
                if (EditorGUI.EndChangeCheck())
                {
                    pairs.arraySize = size;
                    pairs.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
                
                EditorGUI.BeginChangeCheck();
                var containsKeys = new List<string>();
                for (int i = 0; i < size; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Separator();
                    
                    var item = pairs.GetArrayElementAtIndex(i);
                    var key = item.FindPropertyRelative("Key");
                    bool isRepeated = false;
                    if (!string.IsNullOrEmpty(key.stringValue))
                    {
                        if (containsKeys.Contains(key.stringValue))
                            isRepeated = true;
                        else
                            containsKeys.Add(key.stringValue);
                    }

                    var color = GUI.contentColor;
                    if (isRepeated)
                        GUI.contentColor = Color.red;
                    EditorGUILayout.PropertyField(key, GUIContent.none);
                    GUI.contentColor = color;
                    
                    EditorGUILayout.LabelField("=>", GUILayout.MaxWidth(40));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("Value"), GUIContent.none);

                    EditorGUILayout.EndHorizontal();
                }

                if (EditorGUI.EndChangeCheck())
                    property.serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}