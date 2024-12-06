using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using System.Linq;
using Yu5h1Lib;
using System;
using Yu5h1Lib.Runtime;
using System.Reflection;

[CustomEditor(typeof(Patrol))]
public class PatrolEditor : Editor<Patrol>
{
    private int selectedPointIndex = -1;
 
    Vector2[] points { 
        get => targetObject.route.points;
        set => targetObject.route.points = value;
    }
    private bool IsDragging;

    private void OnEnable()
    {
        InitPartol();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //if (GUILayoutAdvanced.Toggle(ref EnableEditMode, "Edit", EditorStyles.toolbarButton))
        //{
        //    SceneView.RepaintAll();
        //}
        if (GUI.changed)
            InitPartol();
    }
    private void OnSceneGUI()
    {
        if (!EditorApplication.isPlaying)
            targetObject.Init();
        targetObject.route.Handle( targetObject, ref selectedPointIndex, ref IsDragging, targetObject.current,
            targetObject.offset,targetObject.offsetQ);
    }
    private void InitPartol()
    {
        if (!EditorApplication.isPlaying)
            targetObject.Init();
    }

};