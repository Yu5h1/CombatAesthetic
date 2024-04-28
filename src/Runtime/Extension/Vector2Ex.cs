
using UnityEngine;

public static class Vector2Ex
{
    public static bool IsSameDirectionAs(this Vector2 vector, Vector2 other)
    {
        float dotProduct = vector.x * other.x + vector.y * other.y;
        float magnitudeProduct = vector.magnitude * other.magnitude;
        return Mathf.Approximately(dotProduct, magnitudeProduct);
    }
}