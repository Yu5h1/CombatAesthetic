using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib;

[CustomEditor(typeof(RouteTracer))]
public class RouteTracerEditor : Editor<RouteTracer>
{
    Camera routeCamera;
    Vector2[] points => targetObject.route.points;

    private int selectedIndex;
    private bool IsDragging;

    public Timer timer => targetObject.timer;

    public float routeProgress;
    Vector3 originameCameraPosition;
    private void OnEnable()
    {
        var cameraobj= GameObject.FindGameObjectWithTag("MainCamera");
        if (cameraobj && cameraobj.name == "RouteCamera" && cameraobj.TryGetComponent(out routeCamera))
            originameCameraPosition = routeCamera.transform.position;
    }
    public override void OnInspectorGUI()
    {
//        EditorGUILayout.HelpBox(
//            @$"IsCompleted{timer.IsCompleted} {timer.time}
//repeat:{timer.repeatCounter},{timer.repeatCount}
//{targetObject.current} {targetObject.route.GetNext(targetObject.current)}
//velocity:{targetObject.velocity}"
//,MessageType.Info);
        DrawDefaultInspector();

        if (routeCamera)
        {
            var progress = EditorGUILayout.Slider("processing", routeProgress, 0, 1);
            if (routeProgress != progress)
            {
                routeProgress = progress;
                routeCamera.transform.position = targetObject.offset + GetPositionFromNormal(routeProgress, targetObject.route.points);
            }
        }
    }

    private void OnSceneGUI()
    {
        if (targetObject.route.points.IsEmpty())
            return;
        //if (!EditorApplication.isPlaying)
        //    targetObject.ResetOffset();
        targetObject.route.Handle(target, ref selectedIndex, ref IsDragging, targetObject.next,targetObject.offset);
    }

    private void OnDisable()
    {
        if (routeCamera)
            routeCamera.transform.position = originameCameraPosition;
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
};