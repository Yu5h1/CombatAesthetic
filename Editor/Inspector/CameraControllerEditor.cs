using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor<CameraController>
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Toggle("IsPerforming", CameraController.IsPerforming);
        EditorGUI.EndDisabledGroup();
    }
}