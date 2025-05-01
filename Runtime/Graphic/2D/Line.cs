using System.Collections.Generic;
using UnityEngine;


namespace Yu5h1Lib.Graphic2D
{
    [System.Serializable]
    public struct Line
    {
        [SerializeField]
        private Vector2 BEGIN;
        [SerializeField]
        private Vector2 END;
        public Vector2 begin
        {
            get => BEGIN;
            set
            {
                if (BEGIN == value)
                    return;
                BEGIN = value;
                OnValueChanged();
            }
        }

        public Vector2 end
        {
            get => END;
            set
            {
                if (END == value)
                    return;
                END = value;
                OnValueChanged();
            }
        }
        private Vector2 CENTER;
        public Vector2 center => CENTER;

        private Vector2 DIRECTION;
        public Vector2 direction => DIRECTION;

        public Vector2 right => new Vector2(direction.y, -direction.x).normalized;
        private float ANGLE;
        public float angle => ANGLE;
        private float LENGTH;
        public float Length => LENGTH;


        public Line(Vector2 start, Vector2 end)
        {
            BEGIN = start;
            END = end;
            CalculateInfo(start, end, out CENTER, out DIRECTION, out LENGTH, out ANGLE);
        }

        private void OnValueChanged() => CalculateInfo();

        private void CalculateInfo() => CalculateInfo(begin, end, out CENTER, out DIRECTION, out LENGTH, out ANGLE);
        public static void CalculateInfo(Vector2 start, Vector2 end, out Vector2 center, out Vector2 direction, out float length, out float angle)
        {
            center = (start + end) / 2;
            direction = end - start;
            length = Vector2.Distance(start, end);
            angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        }

        private Rect GetBounds()
        {
            float minX = Mathf.Min(begin.x, end.x);
            float minY = Mathf.Min(begin.y, end.y);
            float maxX = Mathf.Max(begin.x, end.x);
            float maxY = Mathf.Max(begin.y, end.y);
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Are bounding boxes intersecting
        /// </summary>
        public bool IntersectingB(Line l)
            => GetBounds().Overlaps(l.GetBounds());



        public bool IsPointWithinBounds(Vector2 offset, Vector2 point)
            => Vector2.Dot(point - (begin + offset), direction) > 0 && Vector2.Dot(point - (end + offset), direction) < 0;

        public bool TryGetIntersection(Line other, out Vector2 intersection)
        {
            intersection = Vector2.zero;

            var d1 = direction;
            var d2 = other.direction;

            float determinant = d1.x * d2.y - d1.y * d2.x;

            if (Mathf.Abs(determinant) < Mathf.Epsilon)
                return false;

            var s = other.begin - begin;
            float t = (s.x * d2.y - s.y * d2.x) / determinant;
            float u = (s.x * d1.y - s.y * d1.x) / determinant;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                intersection = begin + t * d1;
                return true;
            }
            return false;
        }
        public static bool TryGetIntersection(IEnumerable<Line> lines, Line line, out Vector2 intersection)
        {
            intersection = Vector2.zero;
            foreach (var item in lines)
            {
                if (item.IntersectingB(line) && item.TryGetIntersection(line, out intersection))
                    return true;
            }
            return false;
        }
        public override string ToString() => $"start:{begin}\nend:{end}\ncenter:{center}\nDirection:{direction}\nLength:{Length}";
    }
}