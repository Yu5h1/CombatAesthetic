
using UnityEngine;

public static class Vector2Ex
{
    public static bool IsSameDirectionAs(this Vector2 vector, Vector2 other)
    {
        float dotProduct = vector.x * other.x + vector.y * other.y;
        float magnitudeProduct = vector.magnitude * other.magnitude;
        return Mathf.Approximately(dotProduct, magnitudeProduct);
    }
    public static Vector2 Rotate(this Vector2 v,float angle)
        => Quaternion.Euler(0, 0, angle) * new Vector3(v.x, v.y, 0);

    public static Vector2 Multiply(this Vector2 a, Vector2 b)
        => new Vector2(a.x * b.x, a.y * b.y);

}