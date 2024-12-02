using System.ComponentModel;
using UnityEditor;
using UnityEngine;

namespace Yu5h1Lib.EditorExtension
{
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    public static class SpriteRendererEx
    {
        [MenuItem("CONTEXT/SpriteRenderer/Set Size By Camera Projection")]
        private static void SetSizeByCameraProjection(MenuCommand command)
        {
            var renderer = (SpriteRenderer)command.context;
            var camera = GameObject.FindAnyObjectByType<CameraController>();
            if ("Requires CameraController implementation method".printWarningIf(!camera))
                return;
            camera.FitSpriteWithProjection(renderer, renderer.transform.position.z);
        }
    }
}
