using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.ComponentModel;
using System.Linq;

namespace Yu5h1Lib.EditorExtension
{
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    public static class PolygonCollider2DEx
    {
        public static float epsilon = 0.1f;

        public const string LabelBase = "CONTEXT/PolygonCollider2D/";
        public const string LabelOptimizePolyPoints = LabelBase + "Optimize points";

        [MenuItem(LabelOptimizePolyPoints)]
        public static void OptimizePolyPoints(this MenuCommand command)
        {
            var polygon = (PolygonCollider2D)command.context;
            polygon.points = SimplifyPolygon(polygon.points.ToList(), epsilon).ToArray();
        }
        #region Process methods
        public static List<Vector2> SimplifyPolygon(List<Vector2> points, float epsilon)
        {
            List<Vector2> result = new List<Vector2>();
            if (points.Count < 3)
                return points;

            result.Add(points[0]);
            SimplifyRecursively(points, 0, points.Count - 1, epsilon, result);
            result.Add(points[points.Count - 1]);
            return result;
        }

        private static void SimplifyRecursively(List<Vector2> points, int start, int end, float epsilon, List<Vector2> result)
        {
            float maxDist = 0;
            int index = 0;

            // Find the point with the maximum distance from the line between start and end
            for (int i = start + 1; i < end; i++)
            {
                float dist = PerpendicularDistance(points[i], points[start], points[end]);
                if (dist > maxDist)
                {
                    index = i;
                    maxDist = dist;
                }
            }

            // If the maximum distance is greater than epsilon, recursively simplify
            if (maxDist > epsilon)
            {
                SimplifyRecursively(points, start, index, epsilon, result);
                result.Add(points[index]);
                SimplifyRecursively(points, index, end, epsilon, result);
            }
        }

        private static float PerpendicularDistance(Vector2 point, Vector2 start, Vector2 end)
        {
            return Mathf.Abs((end.x - start.x) * (start.y - point.y) - (start.x - point.x) * (end.y - start.y)) /
                   Mathf.Sqrt(Mathf.Pow(end.x - start.x, 2) + Mathf.Pow(end.y - start.y, 2));
        }

        #endregion
    }
}