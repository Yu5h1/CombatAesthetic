using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

[System.Serializable]
public struct Line2D
{
    [SerializeField]
    private Vector2 _Start;
    [SerializeField]
    private Vector2 _End;
    public Vector2 Start
    {
        get => _Start;
        set 
        {
            if (_Start == value )
                return;
            _Start = value;
            OnValueChanged();
        }
    }

    public Vector2 End
    {
        get => _End;
        set
        {
            if (_End == value)
                return;
            _End = value;
            OnValueChanged();
        }
    }
    private Vector2 _Center;
    public Vector2 Center => _Center;

    private Vector2 _direction;
    public Vector2 direction => _direction;

    public Vector2 right => new Vector2(direction.y, -direction.x).normalized;
    private float _angle;
    public float angle => _angle;
    private float _Length;
    public float Length => _Length;


    public Line2D(Vector2 start, Vector2 end)
    {
        _Start = start;
        _End = end;
        CalculateInfo(start,end,out _Center,out _direction, out _Length,out _angle);
    }

    private void OnValueChanged() => CalculateInfo();

    private void CalculateInfo() => CalculateInfo(Start, End, out _Center, out _direction, out _Length, out _angle);
    public static void CalculateInfo(Vector2 start,Vector2 end,out Vector2 center,out Vector2 direction, out float length,out float angle)
    {
        center = (start + end) / 2;
        direction = end - start;
        length = Vector2.Distance(start, end);
        angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
    }

    private Rect GetBounds()
    {
        float minX = Mathf.Min(Start.x, End.x);
        float minY = Mathf.Min(Start.y, End.y);
        float maxX = Mathf.Max(Start.x, End.x);
        float maxY = Mathf.Max(Start.y, End.y);
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    /// <summary>
    /// Are bounding boxes intersecting
    /// </summary>
    public bool IntersectingB(Line2D line2)
        => GetBounds().Overlaps(line2.GetBounds());

    

    public bool IsPointWithinBounds(Vector2 offset,Vector2 point) 
        => Vector2.Dot(point - (Start + offset), direction) > 0 && Vector2.Dot(point - (End+ offset), direction) < 0;

    public bool TryGetIntersection(Line2D other, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        var d1 = direction;
        var d2 = other.direction;

        float determinant = d1.x * d2.y - d1.y * d2.x; 

        if (Mathf.Abs(determinant) < Mathf.Epsilon)
            return false;

        var s = other.Start - Start;
        float t = (s.x * d2.y - s.y * d2.x) / determinant;
        float u = (s.x * d1.y - s.y * d1.x) / determinant;

        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
        {
            intersection = Start + t * d1;
            return true;
        }
        return false;
    }
    public static bool TryGetIntersection(IEnumerable<Line2D> lines,Line2D line,out Vector2 intersection)
    {
        intersection = Vector2.zero;
        foreach (var item in lines)
        {
            if (item.IntersectingB(line) && item.TryGetIntersection(line, out intersection))
                return true;
        }
        return false;
    }
    public override string ToString() => $"start:{Start}\nend:{End}\ncenter:{Center}\nDirection:{direction}\nLength:{Length}";
}