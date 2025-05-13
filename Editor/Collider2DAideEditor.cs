using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib;
using Yu5h1Lib.Graphic2D;

[CustomEditor(typeof(Collider2DAide))]
public class Collider2DAgentEditor : Editor<Collider2DAide>
{
    Transform transform => targetObject.transform;
    Collider2D collider => targetObject.collider;

    Vector2[][] paths;

    private void OnEnable()
    {
        if (!collider)
            return;
    }

    private void OnSceneGUI()
    {
        DrawOutlines();
    }
    public void DrawOutlines()
    {
        if (!(collider is CompositeCollider2D composite) || !composite.TryGetPath(out paths,true))
            return;
        Vector2 p = Event.current.GetSceneViewMouseWorldPosition();

        if (paths.TryGetClosetLine(p,out Line result,out Vector2 proj,2))
        {
            result.Draw(Color.red);
            Handles.DrawLine(p, proj, 2);
        }

        //if (p.GetClosetLine(paths,out Line line, out Vector2 proj))
        //{ 

        //}

        //int pathCount = composite.pathCount;
        //float minDistance = 1;// float.MaxValue;
        //Vector2 closestA = Vector2.zero;
        //Vector2 closestB = Vector2.zero;
        ////var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        //var n = Vector2.zero;
        //for (int i = 0; i < pathCount; i++)
        //{
        //    int pointCount = composite.GetPathPointCount(i);
        //    Vector2[] points = new Vector2[pointCount];
        //    composite.GetPath(i, points);

        //    for (int j = 0; j < pointCount; j++)
        //    {
        //        Vector2 a = transform.TransformPoint(points[j]);
        //        Vector2 b = transform.TransformPoint(points[(j + 1) % pointCount]); // Ãö³¬¦±½u
        //        Debug.DrawLine(a, b, Color.yellow);

        //        float distance = ((Vector2)p).DistanceToLine(a, b, out Vector2 proj);
        //        if (distance < minDistance)
        //        {
        //            minDistance = distance;
        //            closestA = a;
        //            closestB = b;
        //            n = proj;
        //        }
        //    }
        //}
        //Debug.DrawLine(closestA, closestB, Color.red);
        //using (new EditorScopes.HandlesScope(new Color(0, 1, 0)))
        //{
        //    Handles.DrawLine(p, n, 2);
        //}

        //SceneView.RepaintAll();
    }

}