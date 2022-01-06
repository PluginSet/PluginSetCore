using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Core.Editor
{
    public class DrawablePropertyDrawer: CustomPropertyBaseDrawer
    {
        protected override void PropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            var toolTip = fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), false).First();
            if (toolTip != null)
                label.tooltip = ((TooltipAttribute) toolTip).tooltip;
            ((DrawablePropertyAttribute)attribute).DrawProperty(position, property, label);
        }
    }
}