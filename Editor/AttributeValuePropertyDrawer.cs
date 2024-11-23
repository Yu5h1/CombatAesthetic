using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[CustomPropertyDrawer(typeof(AttributeType))]
public class CharacterAttributeValueDrawer : PropertyDrawer
{
    public static readonly string[] characterAttribute = new string[] {
        $"{AttributeType.Health}",
        $"{AttributeType.Mana}",
        $"{AttributeType.Stamina}"
    };
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        //if (property.serializedObject.targetObject is AttributeStatusBehaviour statusBehaviour
        //    && statusBehaviour.TryGetComponent(out CharacterController2D character) )
        //{
        //    var index = Array.IndexOf(characterAttribute, $"{(AttributeType)property.enumValueFlag}");
        //    var newIndex = EditorGUI.EnumFlagsField(position, label, characterAttribute);
        //    if (!newIndex.Equals(index) && Enum.TryParse(characterAttribute[newIndex], out AttributeType newVal))
        //        property.SetEnumValue(newVal);
        //}
        //else
            EditorGUI.PropertyField(position, property, label);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true);
}
