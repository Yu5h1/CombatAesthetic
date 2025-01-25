using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(LineRendererController)), CanEditMultipleObjects]
public class LineRendererControllerEditor : Editor<LineRendererController> {

    private string IsConnectingName => nameof(targetObject.IsConnecting);
    private void OnEnable()
    {
        if (EditorApplication.isPlaying)
            return;
    }
    public override void OnInspectorGUI()
    {

        //DrawDefaultInspector();
        this.Iterate(DrawProperty);
    }
    private void DrawProperty( SerializedProperty property)
    {
        if (property.name == $"_{IsConnectingName}")
        {
            targetObject.IsConnecting = EditorGUILayout.Toggle(IsConnectingName, targetObject.IsConnecting);
        }
        else
            EditorGUILayout.PropertyField(property,true);
    }
    private void OnSceneGUI()
    {
        if (EditorApplication.isPlaying)
            return;
        targetObject.Refresh();
    }
}