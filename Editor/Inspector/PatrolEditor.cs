using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using System.Linq;
using Yu5h1Lib;
using System;
using Yu5h1Lib.Runtime;
using System.Reflection;
using UnityEditorInternal;

[CustomEditor(typeof(Patrol))]
public class PatrolEditor : Editor<Patrol>
{
    private int selectedPointIndex = -1;
 
    Vector2[] points { 
        get => targetObject.route.points;
        set => targetObject.route.points = value;
    }
    private bool IsDragging;

    protected void OnEnable()
    {
        InitPartol();
        if (!EditorApplication.isPlaying)
            targetObject.scanner.transform = targetObject.transform;
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
        if (!InternalEditorUtility.GetIsInspectorExpanded(target) || !targetObject.isActiveAndEnabled)
            return;
        //Gizmos.DrawWireSphere(Destination, arriveRange);

        //if(Event.current.type == EventType.MouseMove)
            targetObject.Visualize();

        if (!EditorApplication.isPlaying)
            targetObject.Init();
        targetObject.route.Handle( targetObject, ref selectedPointIndex, ref IsDragging, targetObject.current,
            targetObject.offset,targetObject.offsetQ);

        if (targetObject.scanner.collider)
        Handles.DotHandleCap(0,
            targetObject.scanner.start,
            Quaternion.identity,
            0.05f, EventType.Repaint);
    }
    private void InitPartol()
    {
        if (!EditorApplication.isPlaying)
            targetObject.Init();
    }

};