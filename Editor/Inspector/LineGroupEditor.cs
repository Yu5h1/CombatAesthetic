using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib;

[CustomEditor(typeof(LineGroup))]
public class LineGroupEditor : Editor<LineGroup>
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
    private void OnSceneGUI()
    {
        if (targetObject.IsAvailable() && !targetObject.lineControllers.IsEmpty())
            foreach (var l in targetObject.lineControllers)
                if (l.IsAvailable())
                    l.Refresh();
    }
    private void OnDisable()
    {

    }
}