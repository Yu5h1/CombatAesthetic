using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.Game.Character;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(CharacterController2D))]
public class Controller2DEditor : Editor<CharacterController2D> 
{
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