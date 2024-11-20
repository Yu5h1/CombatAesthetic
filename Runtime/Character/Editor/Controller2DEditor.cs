using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.Game.Character;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(Controller2D))]
public class Controller2DEditor : Editor<Controller2D> {

    private void OnEnable()
    {
        
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUI.changed)
        {
            
        }
    }
    private void OnSceneGUI()
    {
        if (!(targetObject.host is Autopilot autopilot))
            return;
        var b = targetObject.hostBehaviour;
    }
};