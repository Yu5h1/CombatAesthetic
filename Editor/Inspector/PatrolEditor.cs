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
    public static float mouseHitThreshold = 5;
    public static float dotSize = 0.1f;
    public static float DottedLineSize = 3;
    public static float lineHitDistance = 5;
    

    private int selectedPointIndex = -1;

    Vector3 position => targetObject.transform.position;
    Vector3 lossyScale => targetObject.transform.lossyScale;
    Vector2[] points { 
        get => targetObject.route.points;
        set => targetObject.route.points = value;
    }
    private bool IsDragging;

    private void OnEnable()
    {
        if (!EditorApplication.isPlaying)
            targetObject.Init();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //if (GUILayoutAdvanced.Toggle(ref EnableEditMode, "Edit", EditorStyles.toolbarButton))
        //{
        //    SceneView.RepaintAll();
        //}
        if (GUI.changed)
            targetObject.Init();
    }

    //private void EditModeGUI()
    //{


    //}
    private void OnSceneGUI()
    {
        if (!EditorApplication.isPlaying)
            targetObject.Init();
        targetObject.route.Handle( targetObject, ref selectedPointIndex, ref IsDragging, targetObject.current,
            targetObject.offset,targetObject.offsetQ,targetObject.transform.lossyScale);
    }
};