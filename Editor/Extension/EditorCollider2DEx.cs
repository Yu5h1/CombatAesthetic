using UnityEngine;
using UnityEditor;
using System.ComponentModel;

namespace Yu5h1Lib.EditorExtension
{
    public enum HandleStyle
    {
        None = 0,
        Button,
        Slide,
        Slide2D
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public static class EditorCollider2DEx
    {
        public static void DrawHandles(this Collider2D collider,Color? color = null,HandleStyle style = HandleStyle.None)
        {
            var originalcolor = Handles.color;
            if (color != null)
                Handles.color = color.Value; // 設定顏色

            var matrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(collider.transform.position, collider.transform.rotation, collider.transform.lossyScale);

            switch (collider)
            {
                case BoxCollider2D boxCollider:
                    DrawBoxCollider(boxCollider,style);
                    break;
                case CircleCollider2D circleCollider:
                    DrawCircleCollider(circleCollider);
                    break;
                case PolygonCollider2D polygonCollider:
                    DrawPolygonCollider(polygonCollider);
                    break;
                default:
                    Debug.LogWarning($"Unsupported Collider2D type. ({collider.GetType()})");
                    break;
            }
            if (color != null)
                Handles.color = originalcolor;
            Handles.matrix = matrix;
        }
        private static void DrawBoxCollider(BoxCollider2D boxCollider, HandleStyle style = HandleStyle.None)
        {
            switch (style)
            {
                case HandleStyle.None:
                    Handles.DrawWireCube(boxCollider.offset, boxCollider.size);
                    break;
                case HandleStyle.Button:
                    if (Handles.Button(boxCollider.offset, Quaternion.identity, 1f,1f, Handles.CubeHandleCap))
                        Selection.activeObject = boxCollider.gameObject;
                    break;
                case HandleStyle.Slide:
                    Vector3 newPosition = Handles.Slider(boxCollider.offset, Vector3.right);
                    if (newPosition != Vector3.zero)
                    {
                        Undo.RecordObject(boxCollider, "Move BoxCollider2D");
                        boxCollider.offset = (Vector2)newPosition; // 更新偏移量
                    }
                    break;
                case HandleStyle.Slide2D:
                    Vector3 newPosition2D = Handles.Slider2D(boxCollider.offset, Vector3.forward, Vector3.right, Vector3.up, 1, Handles.RectangleHandleCap, 0.1f);
                    if (newPosition2D != Vector3.zero)
                    {
                        Undo.RecordObject(boxCollider, "Move BoxCollider2D");
                        boxCollider.offset = (Vector2)newPosition2D; // 更新偏移量
                    }
                    break;
                default:
                    break;
            }
        }
        private static void DrawCircleCollider(CircleCollider2D circleCollider)
        {
            var position = circleCollider.transform.position + (Vector3)circleCollider.offset;
            Handles.DrawWireArc(position, Vector3.forward, Vector3.right, 360f, circleCollider.radius);
        }

        private static void DrawPolygonCollider(PolygonCollider2D polygonCollider)
        {
            Vector2[] points = polygonCollider.points;

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 startPoint = polygonCollider.transform.TransformPoint(points[i]);
                Vector2 endPoint = polygonCollider.transform.TransformPoint(points[(i + 1) % points.Length]);
                Handles.DrawLine(startPoint, endPoint);
            }
        }
    }
}
