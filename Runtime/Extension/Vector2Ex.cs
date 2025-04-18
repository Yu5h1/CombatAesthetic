
using System.ComponentModel;
using UnityEngine;

namespace Yu5h1Lib
{
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    public static class Vector2Ex
    {
        public static bool IsSameDirectionAs(this Vector2 vector, Vector2 other)
        {
            float dotProduct = vector.x * other.x + vector.y * other.y;
            float magnitudeProduct = vector.magnitude * other.magnitude;
            return Mathf.Approximately(dotProduct, magnitudeProduct);
        }
        public static Vector2 Rotate(this Vector2 v, float angle) => Quaternion.Euler(0, 0, angle) * v;
        public static Vector2 Rotate(this Vector2 v, Quaternion quaternion) => quaternion * v;

        public static Vector2 Multiply(this Vector2 a, Vector2 b)
            => new Vector2(a.x * b.x, a.y * b.y);

        public static bool IsDirectionAngleWithinThreshold(this Vector2 referenceDirection, Vector2 targetDirection, float threshold)
        {
            referenceDirection.Normalize();
            targetDirection.Normalize();
            float dot = Vector2.Dot(referenceDirection, targetDirection);
            float cosThreshold = Mathf.Cos(threshold * Mathf.Deg2Rad);
            return dot >= cosThreshold;
        }
        public static Quaternion DirectionToQuaternion2D(this Vector2 dir, Direction2D direction = Direction2D.right)
        {
            var degree = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            switch (direction)
            {
                case Direction2D.left:
                    degree -= 180;
                    break;
                case Direction2D.up:
                    degree -= 90;
                    break;
                case Direction2D.down:
                    degree += 90;
                    break;
            }
            return Quaternion.Euler(0, 0, degree);
        }
    } 
}