using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor<MovingPlatform>
{

    Vector2[] points => targetObject.route.points;

    private int selectedIndex;
    private bool IsDragging;

    public Timer timer => targetObject.timer;

    private void OnEnable()
    {
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            @$"IsCompleted{timer.IsCompleted} {timer.time}
repeat:{timer.repeatCounter},{timer.repeatCount}
{targetObject.current} {targetObject.route.GetNext(targetObject.current)}
velocity:{targetObject.velocity}"
,MessageType.Info);
        DrawDefaultInspector();

        
    }

    private void OnSceneGUI()
    {
        if (targetObject.route.points.IsEmpty())
            return;
        if (!EditorApplication.isPlaying)
            targetObject.ResetOffset();
        targetObject.route.Handle(target, ref selectedIndex, ref IsDragging, targetObject.next,targetObject.offset);
    }
};