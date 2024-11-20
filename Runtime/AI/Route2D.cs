using System;
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
    
    public bool IsArrived(Vector2 position, Vector2 offset, Quaternion offsetQ, int current, float arriveRange, Vector2 scale)
            => IsArrived(points, position, offset, offsetQ, current, arriveRange, scale);
    public Vector2 GetDirection(Vector2 position, Vector2 offset, Quaternion offsetQ, ref int current, float arriveRange, Vector2 scale)
            => GetDirection(loop, points, position, offset, offsetQ, ref current, arriveRange, scale);
    public bool MoveNext(ref int current) => MoveNext(points, loop, ref current);

    public static bool IsArrived(Vector2[] points,Vector2 position, Vector2 offset, Quaternion offsetQ, int current, float arriveRange, Vector2 scale)
        => points.IsEmpty() ? true : Vector2.Distance(position.Multiply(scale), (offset + ((Vector2)(offsetQ * points[current]))).Multiply(scale)) < arriveRange;

    public static Vector2 GetDirection(bool loop,Vector2[] points,Vector2 position,
        Vector2 offset, Quaternion offsetQ, ref int current, float arriveRange, Vector2 scale )
    {
        if (points.IsEmpty() || !points.IsValid(current))
            return Vector2.zero;
        if (offsetQ == default(Quaternion))
            offsetQ = Quaternion.identity;
        if (IsArrived(points,position, offset, offsetQ, current, arriveRange, scale))
        {
            if (!MoveNext(points,loop,ref current))
                return Vector2.zero;
        }
        return (offset + ((Vector2)(offsetQ * points[current])) - position).Multiply(scale).normalized;
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

}
