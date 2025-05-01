using UnityEditor;
using UnityEngine;

namespace Yu5h1Lib.EditorExtension
{
    public static class EventEx
    {
        public static Vector3 GetSceneViewMouseWorldPosition(this Event e)
        {
            var mousePos = e.mousePosition;
            var sceneView = SceneView.currentDrawingSceneView;

            mousePos.y = sceneView.camera.pixelRect.height - mousePos.y;

            if (SceneView.currentDrawingSceneView.in2DMode)
            {
                Vector3 worldPos = sceneView.camera.ScreenToWorldPoint(mousePos);
                return worldPos;
            }
            else
            {
                Ray ray = sceneView.camera.ScreenPointToRay(mousePos);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                float distance;
                if (groundPlane.Raycast(ray, out distance))
                {
                    Vector3 worldPos = ray.GetPoint(distance);
                    return worldPos;
                }
            }

            return Vector3.zero; 
        }
    } 
}
