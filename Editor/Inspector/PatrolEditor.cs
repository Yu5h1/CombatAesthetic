using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using System.Linq;
using Yu5h1Lib;
using System;
using Yu5h1Lib.Runtime;

[CustomEditor(typeof(Patrol))]
public class PatrolEditor : Editor<Patrol>
{
    public static float mouseHitThreshold = 5;
    public static float dotSize = 0.1f;
    public static float DottedLineSize = 3;
    public static float lineHitDistance = 5;
    public static bool EnableEditMode = true;
    

    private int selectedPointIndex = -1;

    Vector3 position => targetObject.transform.position;
    Vector3 lossyScale => targetObject.transform.lossyScale;
    Vector2[] points { 
        get => targetObject.route.points;
        set => targetObject.route.points = value;
    }
    Vector3[] worldPoints;
    private bool IsDragging { get; set; }
    private void OnEnable()
    {
        if (!EditorApplication.isPlaying)
            targetObject.Init();
        worldPoints = new Vector3[targetObject.route.points.IsEmpty() ? 0 : points.Length];
        CalcuteRoutes();
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();



        //if (GUILayoutAdvanced.Toggle(ref EnableEditMode, "Edit", EditorStyles.toolbarButton))
        //{
        //    SceneView.RepaintAll();
        //}
        if (GUI.changed)
            CalcuteRoutes();
    }

    //private void EditModeGUI()
    //{


    //}
    private void CalcuteRoutes()
    {
        if (worldPoints == null)
            worldPoints = new Vector3[targetObject.route.points.IsEmpty() ? 0 : points.Length];
        else
            Array.Resize(ref worldPoints, targetObject.route.points.Length);

        Vector2 offset = targetObject.offset;

        if (targetObject.UseLocalCoordinate)
            for (int i = 0; i < points.Length; i++)
                worldPoints[i] = offset + (Vector2) (targetObject.offsetQ * points[i]);
        else
            for (int i = 0; i < points.Length; i++)
                worldPoints[i] = offset + points[i];
    }

    private void OnSceneGUI()
    {
        if (!EnableEditMode)
            return;
        var e = Event.current;
        
        if (!targetObject)
            return;
        var mousePos = e.mousePosition;
        var sceneView = SceneView.currentDrawingSceneView;
        var camera = sceneView?.camera;
        if (sceneView == null || camera == null)
            return;

        mousePos.y = sceneView.camera.pixelRect.height - mousePos.y;


        if (e.type == EventType.MouseDown)
        {
            MouseHitTest(e,mousePos, camera);
        }
        else if (e.type == EventType.MouseDrag )
        {
            if (!e.control)
            {
                if (!IsDragging)
                {
                    Undo.RegisterCompleteObjectUndo(targetObject, "patrol points Change");
                    IsDragging = true;
                }
                if (selectedPointIndex != -1 && Event.current.button == 0)
                {
                    UpdateSelectedPoint(mousePos, sceneView);
                    SceneView.RepaintAll();
                }
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            IsDragging = false;
        }
        else if (e.type == EventType.MouseMove)
        {
            SceneView.RepaintAll();
            return;
        }

        if (e.type == EventType.MouseMove )
        {
            SceneView.RepaintAll();
            return;
        }else if (e.type == EventType.Repaint )
        {
            CalcuteRoutes();
            DrawLinesAndDots(e.type);
        }
    }
    private void DrawLinesAndDots(EventType eventType)
    {
        if (points.IsEmpty() || points.Length < 2)
            return;

        var originColor = Handles.color;
        Handles.color = Color.white;

        for (int i = 0; i < points.Length - 1; i++)
        {
            var p1 = worldPoints[i];
            var p2 = worldPoints[i + 1];
            DrawLine(p1, p2);
            DrawDot(eventType, p1, i);
        }
        DrawDot(eventType, worldPoints.Last(), points.Length - 1);
        if (targetObject.route.loop && points.Length > 2)
            DrawLine(worldPoints.First(), worldPoints.Last());

        Handles.color = originColor;
    }

    
    private void UpdateSelectedPoint(Vector2 mousePoint, SceneView view)
    {
        if (!points.IsValid(selectedPointIndex))
            return;
        var camera = view.camera;
        var depth = Vector3.Distance(camera.transform.position, position);
        var worldPoint = camera.ScreenToWorldPoint(new Vector3(mousePoint.x,mousePoint.y, -depth));

        if (targetObject.UseLocalCoordinate)
        {
            points[selectedPointIndex] = targetObject.transform.InverseTransformPoint(worldPoint);
        }
        else
            points[selectedPointIndex] = Vector3.Scale(worldPoint, lossyScale) - position;

        //transform.InverseTransformPoint(Vector3.Scale(worldPoint, transform.lossyScale));
    }
    private void MouseHitTest(Event e,Vector2 mousePos, Camera camera)
    {
        if (worldPoints.IsEmpty())
            return;
        selectedPointIndex = -1;
        for (int i = 0; i < worldPoints.Length; i++)
        {
            if (DotHitTest(e, i))
                break;
            else if (e.control)
                LineHitTest(e, i);
        }
        if (selectedPointIndex >= 0  && e.control )
        {
            Undo.RegisterCompleteObjectUndo(targetObject, "patrol points Resized");
            var list = points.ToList();
            list.RemoveAt(selectedPointIndex);
            points = list.ToArray();
        }
    }
    private bool LineHitTest(Event e, int i)
    {
        var p1 = worldPoints[i];
        var ii = i + 1 < worldPoints.Length ? i + 1 : 0;
        var p2 = worldPoints[ii];
        var hit = HandleUtility.DistanceToLine(p1, p2) < lineHitDistance;

        var d1 = DistanceFromDot(p1);
        var d2 = DistanceFromDot(p2);

        if (d1 > dotSize && d2 > dotSize && hit)
        {
            Undo.RegisterCompleteObjectUndo(targetObject, "patrol points Resized");
            var list = points.ToList();
            list.Insert(ii, (points[i] + points[ii]) / 2);
            points = list.ToArray();
        }
        return hit;
    }
    private bool DotHitTest(Event e, int i)
    {
        var distance = HandleUtility.DistanceToCube(worldPoints[i], Quaternion.identity, dotSize * 2);
        if (distance > dotSize)
            return false;
        selectedPointIndex = i;
        e.Use();
        return true;
    }
    private void DrawLine(Vector3 p1, Vector3 p2){
        var originColor = Handles.color;

        var d1 = DistanceFromDot(p1);
        var d2 = DistanceFromDot(p2);

        if (Event.current.control)
            Handles.color = d1 > dotSize && d2 > dotSize &&
                HandleUtility.DistanceToLine(p1, p2) < lineHitDistance ? Color.gray : Color.white;

        Handles.DrawDottedLine(p1, p2, DottedLineSize);
        Handles.color = originColor;
    }
    private void DrawDot(EventType type, Vector3 point, int index)
    {
        var originColor = Handles.color;
        var distance = DistanceFromDot(point);
        Handles.color = index == selectedPointIndex || distance < dotSize ? Color.gray : Color.white;
        if (EditorApplication.isPlaying)
        { 
            if (index == targetObject.current)
                Handles.color =  Color.green;
        }
        if (type == EventType.Repaint)
            Handles.DotHandleCap(0, point, Quaternion.identity, dotSize, EventType.Repaint);
        Handles.color = originColor;
    }
    private float DistanceFromDot(Vector3 p) => HandleUtility.DistanceToCube(p, Quaternion.identity, dotSize* 2);
};