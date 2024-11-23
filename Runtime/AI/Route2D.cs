using System;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;

[Serializable]
public class Route2D 
{
    public bool loop;
    public Vector2[] points;
    public Vector2 this[int index]
    {
        get => points[index];
        set => points[index] = value;
    }
    public float CalculateLength(out float[] lengths) {
        lengths = new float[points.Length];
        if (points.Length < 1)
            return 0;
        var length = 0f;
        for (int i = 0; i < points.Length - 1; i++)
        {
            var d = Vector2.Distance(points[i], points[i + 1]);
            length += d;
            lengths[i] = d;
        }
        var backlength = Vector2.Distance(points[points.Length - 1], points[0]);
        lengths[points.Length - 1] = backlength;
        if (loop)
            length += backlength;
        return length;
    }
    public void DrawGizmos(Vector2 offset ,Color color)
    {
        if (points.IsEmpty())
            return; 
        for (int i = 0; i < points.Length - 1; i++)
        {
            var current = offset + points[i];
            var next = offset + points[i + 1];
            Debug.DrawLine(current, next, color);
        }
    }
    
    public Vector2 GetDirection(Vector2 position, Vector2 offset, Quaternion offsetQ, ref int current, float arriveRange)
            => GetDirection(loop, points, position, offset, offsetQ, ref current, arriveRange);

    public bool MoveNext(ref int current) => MoveNext(points, loop, ref current);
    public int GetNext(int current) => GetNext(points, loop, current);
    

    public static bool IsArrived(Vector2 position, Vector2 destination, float arriveRange)
        => Vector2.Distance(position, destination) < arriveRange;

    public static Vector2 GetDirection(bool loop,Vector2[] points,Vector2 position,
        Vector2 offset, Quaternion offsetQ, ref int current, float arriveRange )
    {
        if (points.IsEmpty() || !points.IsValid(current))
            return Vector2.zero;
        if (offsetQ == default(Quaternion))
            offsetQ = Quaternion.identity;
        var destination = offset + points[current].Rotate(offsetQ);
        if (IsArrived(position, destination, arriveRange))
        {
            if (!MoveNext(points,loop,ref current))
                return Vector2.zero;
        }
        return (destination - position).normalized;
    }


    public static bool MoveNext(Vector2[] points,bool loop, ref int current)
    {
        if (current + 1 < points.Length)
            current++;
        else if (loop)
            current = 0;
        else
            return false;
        return true;
    }
    public static int GetNext(Vector2[] points, bool loop, int current)
    {
        if (current + 1 < points.Length)
            return current + 1;
        else if (loop)
            return 0;
        else
            return current;
    }
}
