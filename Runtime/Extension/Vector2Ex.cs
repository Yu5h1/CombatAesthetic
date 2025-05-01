
using System.ComponentModel;
using UnityEngine;
using Yu5h1Lib.Graphic2D;

namespace Yu5h1Lib
{
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    public static class Vector2Ex
    {
        public static float DistanceToLine(this Vector2 point, Vector2 a, Vector2 b, out Vector2 projection)
        {
            Vector2 ab = b - a;
            Vector2 ap = point - a;
            float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);
            projection = a + t * ab;

            return Vector2.Distance(point, projection);
        }

        public static bool GetClosetLine(this Vector2 p, ref float minDistance, Vector2[] path,out Line result,out Vector2 proj)
        {
            proj = Vector2.zero;
            result = default;
            for (int i = 0; i < path.Length; i++)
            {
                Vector2 a = path[i];
                Vector2 b = path[(i + 1) % path.Length]; 
                Debug.DrawLine(a, b, Color.yellow);

                float distance = ((Vector2)p).DistanceToLine(a, b, out proj);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    result.begin = a;
                    result.end = b;
                    return true;
                }
            }
            return false;
        }
        public static Line GetClosetLine(this Vector2 p, Vector2[][] paths,out Vector2 proj)
        {
            proj = Vector2.zero;
            float minDistance = float.MaxValue;
            Line result = default;
            for (int i = 0; i < paths.Length; i++)
            {
                if (p.GetClosetLine(ref minDistance, paths[i], out Line found,out Vector2 projvalue))
                {
                    result = found;
                    proj = projvalue;
                }
            }
            return result;
        }

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