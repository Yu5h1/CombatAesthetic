using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CustomSceneViewHotkeys
{
    static CustomSceneViewHotkeys()
    {
        // Register callback for when the scene view is updated
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Check for Alt key and mouse middle button drag
        if (e.alt && e.type == EventType.MouseDrag && e.button == 2)
        {
            //Custom rotate view logic
            e.Use();
            RotateView(sceneView, Vector3.up, e.delta.x * 0.3f);// yaw
            RotateView(sceneView, Vector3.right, e.delta.y * 0.2f,false);//pitch
            EditorGUIUtility.AddCursorRect(sceneView.position, MouseCursor.ArrowPlus);
            sceneView.Repaint();
            // Use the event so it doesn't propagate to the default handler

            //SimulateAltRightMouseDrag(sceneView, e.delta);
            //e.Use(); // Use the event so it doesn't propagate to the default handler

        }

        // Prevent default Alt + Mouse left drag rotation
        if (e.alt && e.type == EventType.MouseDrag && e.button == 0)
        {
            //e.Use(); // Use the event so it doesn't propagate to the default handler
        }
    }
    static void SimulateAltRightMouseDrag(SceneView sceneView, Vector2 delta)
    {
        sceneView.SendEvent(new Event
        {
            type = EventType.MouseDrag,
            button = 0, // Right mouse button
            delta = delta,
            alt = true
        });
    }
    static void RotateView(SceneView sceneView,Vector3 dir, float delta,bool local = true) {
        if (delta == 0.0f)
            return;
        var eular = Quaternion.Euler(dir * delta);
        var pos = Quaternion.Inverse(sceneView.rotation) * sceneView.camera.transform.position - sceneView.pivot;
        sceneView.rotation = local ? eular * sceneView.rotation : sceneView.rotation * eular ;
        sceneView.camera.transform.position = eular * pos;
    }
    static void SetCustomCursor(SceneView sceneView, string cursorName)
    {
        // Use reflection to access the internal SetCursor method
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo setCursorMethod = typeof(SceneView).GetMethod("SetCursor", bindingFlags, null, new[] { typeof(string) }, null);

        if (setCursorMethod != null)
        {
            setCursorMethod.Invoke(sceneView, new object[] { cursorName });
        }
    }
}