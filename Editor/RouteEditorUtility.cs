using System.Linq;
using UnityEditor;
using UnityEngine;
using Yu5h1Lib;

public static class RouteEditorUtility
{
    public static float mouseHitThreshold = 5;
    public static float dotSizeFactor = 0.05f;
    public static float GetDotSize(Vector3 point) => HandleUtility.GetHandleSize(point) * dotSizeFactor;

    public static float DottedLineSize = 3;
    public static float lineHitDistance = 5;

    public static void Handle(this Route2D route, Object targetObject, ref int selectedIndex, ref bool IsDragging, int next)
        => route.Handle(targetObject, ref selectedIndex, ref IsDragging, next, Vector2.zero, Quaternion.identity);

    public static void Handle(this Route2D route, Object targetObject, ref int selectedIndex, ref bool IsDragging, int next, Vector2 offset)
        => route.Handle(targetObject, ref selectedIndex, ref IsDragging, next, offset, Quaternion.identity);

    public static void Handle(this Route2D route,Object targetObject,ref int selectedIndex,ref bool IsDragging, int next,
        Vector2 offset ,Quaternion offsetQ )
    {
        if (!targetObject)
            return;
        var e = Event.current;
        var mousePos = e.mousePosition;
        var sceneView = SceneView.currentDrawingSceneView;
        if (!sceneView.in2DMode)
            return;
        var camera = sceneView?.camera;
        if (sceneView == null || camera == null)
            return;
        mousePos.y = sceneView.camera.pixelRect.height - mousePos.y;

        if (e.type == EventType.MouseDown)
        {
            route.MouseDown(targetObject, e, mousePos, camera, ref selectedIndex, offset,offsetQ);
        }
        else if (e.type == EventType.MouseDrag)
        {
            if (!e.control)
            {
                if (!IsDragging)
                {
                    Undo.RegisterCompleteObjectUndo(targetObject, "patrol points Change");
                    IsDragging = true;
                }
                if (selectedIndex != -1 && Event.current.button == 0)
                {
                    route.UpdateSelectedPoint(sceneView, mousePos, selectedIndex, offset, offsetQ);
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

        if (e.type == EventType.MouseMove)
        {
            SceneView.RepaintAll();
            return;
        }
        else if (e.type == EventType.Repaint)
            route.DrawLinesAndDots(selectedIndex, next, offset,offsetQ);
    }

    public static void MouseDown(this Route2D route, Object targetObject, Event e, Vector2 mousePos, Camera camera, ref int selectedIndex,
        Vector2 offset, Quaternion offsetQ)
    {
        if (route.points.IsEmpty())
            return;
        selectedIndex = -1;
        for (int i = 0; i < route.points.Length; i++)
        {
            if (route.HitDotTest(e, i, ref selectedIndex,offset, offsetQ))
                break;
            else if (e.control)
                route.InsertPointOnLine(targetObject,i,offset, offsetQ);
        }
        if (selectedIndex >= 0 && e.control)
        {
            Undo.RegisterCompleteObjectUndo(targetObject, "patrol points Resized");
            var list = route.points.ToList();
            list.RemoveAt(selectedIndex);
            route.points = list.ToArray();
        }
    }

    public static bool InsertPointOnLine(this Route2D route, Object targetObject,int i,
        Vector2 offset , Quaternion offsetQ)
    {
        var e = Event.current;
        var p1 = offset + route.points[i].Rotate(offsetQ);
        var ii = i + 1 < route.points.Length ? i + 1 : 0;
        var p2 = offset + route.points[ii].Rotate(offsetQ);
        var hit = HandleUtility.DistanceToLine(p1, p2) < lineHitDistance;


        var d1Size = GetDotSize(p1);
        var d2Size = GetDotSize(p2);

        var d1 = DistanceFromDot(p1);
        var d2 = DistanceFromDot(p2);

        if (d1 > d1Size && d2 > d2Size && hit)
        {
            Undo.RegisterCompleteObjectUndo(targetObject, "patrol points Resized");
            var list = route.points.ToList();
            list.Insert(ii, (route.points[i].Rotate(offsetQ) + route.points[ii].Rotate(offsetQ)) / 2);
            route.points = list.ToArray();
            return true;
        }
        return false;
    }

    public static bool HitDotTest(this Route2D route, Event e, int i,ref int selectedPointIndex,
        Vector2 offset, Quaternion offsetQ)
    {
        var p = offset + route.points[i].Rotate(offsetQ);
        var dotsize = GetDotSize(p);
        var distance = HandleUtility.DistanceToCube(p, Quaternion.identity, dotsize * 2);
        if (distance > dotsize)
            return false;
        selectedPointIndex = i;
        e.Use();
        return true;
    }


    public static void UpdateSelectedPoint(this Route2D route, SceneView view, Vector2 mousePoint, int selectedPointIndex,
    Vector3 offset, Quaternion offsetQ)
    {
        if (!route.points.IsValid(selectedPointIndex))
            return;
        var camera = view.camera;

        var worldPoint = Vector3.zero;

        var depth = Vector3.Distance(camera.transform.position, offset);
        worldPoint = camera.ScreenToWorldPoint(new Vector3(mousePoint.x, mousePoint.y, -depth));

        worldPoint = Quaternion.Inverse(offsetQ) * (worldPoint - offset);
        route.points[selectedPointIndex] = worldPoint;

    }
    private static float DistanceFromDot(Vector3 p) => HandleUtility.DistanceToCube(p, Quaternion.identity, GetDotSize(p) * 2);

    #region Draw
    public static void DrawDot(Vector3 point, Color? color = null)
    {
        if (Event.current.type != EventType.Repaint)
            return;
        var originColor = Handles.color;
        if (color != null)
            Handles.color = color.Value;
        Handles.DotHandleCap(0, point, Quaternion.identity, GetDotSize(point), EventType.Repaint);
        if (color != null)
            Handles.color = originColor;
    }

    public static void DrawDot(Vector3 point, int index, int selectedIndex, int next)
    {
        var dotsize = GetDotSize(point);
        var distance = DistanceFromDot(point);
        var color = index == selectedIndex || distance < dotsize ? Color.gray : Color.white;

        if (EditorApplication.isPlaying)
            if (index == next)
                color = Color.green;

        DrawDot(point, color);
    }
    public static void DrawLine(Vector3 p1, Vector3 p2)
    {
        var originColor = Handles.color;

        var d1s = GetDotSize(p1);
        var d2s = GetDotSize(p2);
        var d1 = DistanceFromDot(p1);
        var d2 = DistanceFromDot(p2);

        if (Event.current.control)
            Handles.color = d1 > d1s && d2 > d2s &&
                HandleUtility.DistanceToLine(p1, p2) < lineHitDistance ? Color.gray : Color.white;

        Handles.DrawDottedLine(p1, p2, DottedLineSize);
        Handles.color = originColor;
    }
    public static void DrawLinesAndDots(this Route2D route, int selectedIndex, int next, Vector2 offset, Quaternion offsetQ)
    {
        var points = route.points;
        if (points.IsEmpty() || points.Length < 2)
            return;

        var originColor = Handles.color;
        Handles.color = Color.white;

        for (int i = 0; i < points.Length - 1; i++)
        {
            var p1 = offset + points[i].Rotate(offsetQ);
            var p2 = offset + points[i + 1].Rotate(offsetQ);
            DrawLine(p1, p2);
            DrawDot(p1, i, selectedIndex, next);
        }
        DrawDot(offset + points.Last().Rotate(offsetQ), points.Length - 1, selectedIndex, next);
        if (route.loop && points.Length > 2)
            DrawLine(offset + points.First().Rotate(offsetQ), offset + points.Last().Rotate(offsetQ));

        Handles.color = originColor;
    }

    #endregion

}
