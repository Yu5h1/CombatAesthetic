using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib;

[CustomEditor(typeof(LineRendererController)), CanEditMultipleObjects]
public class LineRendererControllerEditor : Editor<LineRendererController> {

    private string IsConnectingName => nameof(targetObject.IsConnecting);
    protected void OnEnable()
    {
        if (EditorApplication.isPlaying)
            return;
    }
    public override void OnInspectorGUI()
    {

        //DrawDefaultInspector();
        this.Iterate(DrawProperty, BeginDrawProperty);
    }
    private void BeginDrawProperty()
    {

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