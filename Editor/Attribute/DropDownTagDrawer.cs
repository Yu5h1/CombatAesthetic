using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DropDownTagAttribute))]
public class DropDownTagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUILayout.HelpBox("DropDownTag is only available for string type.", MessageType.Warning);
            return;
        }
        EditorGUI.BeginProperty(position, label, property);
        property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
        EditorGUI.EndProperty();
    }
}
