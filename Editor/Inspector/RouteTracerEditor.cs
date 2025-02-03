using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib;

[CustomEditor(typeof(RouteTracer))]
public class RouteTracerEditor : Editor<RouteTracer>
{
    Transform transform => targetObject.transform;
    Vector2[] points => targetObject.route.points;

    private int selectedIndex;
    private bool IsDragging;

    public Timer timer => targetObject.timer;

    
    public float routeProgress;

    private bool simulateToggle;
    private float simulateProgress;
    private Vector3? OriginalPosition;

    public override void OnInspectorGUI()
    {
//        EditorGUILayout.HelpBox(
//            @$"IsCompleted{timer.IsCompleted} {timer.time}
//repeat:{timer.repeatCounter},{timer.repeatCount}
//{targetObject.current} {targetObject.route.GetNext(targetObject.current)}
//velocity:{targetObject.velocity}"
//,MessageType.Info);
        DrawDefaultInspector();

        if (EditorApplication.isPlaying)
            return;
        var simulateCheck = GUILayout.Toggle(simulateToggle, nameof(simulateToggle),"Button");
        if (simulateToggle != simulateCheck)
        {
            simulateToggle = simulateCheck;
            if (simulateToggle)
                OriginalPosition = transform.localPosition;
            else if (OriginalPosition != null)
                transform.localPosition = (Vector3)OriginalPosition;
        }
        if (simulateToggle)
        if (this.TrySlider("progress:", ref simulateProgress, 0, 1))
            targetObject.transform.position = targetObject.offset + GetPositionFromNormal(simulateProgress, targetObject.route.points);
    }

    private void OnSceneGUI()
    {
        if (targetObject.route.points.IsEmpty())
            return;
        if (!EditorApplication.isPlaying && !simulateToggle)
            targetObject.ResetOffset();
        targetObject.route.Handle(target, ref selectedIndex, ref IsDragging, targetObject.next,targetObject.offset);
    }
 
    protected void OnDisable()
    {
        if (!EditorApplication.isPlaying && OriginalPosition != null)
            targetObject.transform.localPosition = (Vector3)OriginalPosition;
    }
    protected override void ExitingEditMode()
    {
        if (OriginalPosition != null)
            targetObject.transform.localPosition = (Vector3)OriginalPosition;
    }
    Vector2 GetPositionFromNormal(float normal, Vector2[] points)
    {
        if (points == null || points.Length < 2)
        {
            Debug.LogError("Points array must have at least 2 points!");
            return Vector3.zero;
        }

        float scaledPosition = normal * (points.Length - 1); 
        int indexA = Mathf.FloorToInt(scaledPosition);       
        int indexB = Mathf.CeilToInt(scaledPosition);
        float t = scaledPosition - indexA;                  

        indexA = Mathf.Clamp(indexA, 0, points.Length - 1);
        indexB = Mathf.Clamp(indexB, 0, points.Length - 1);

        return Vector2.Lerp(points[indexA], points[indexB], t);
    }
}