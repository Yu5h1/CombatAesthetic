using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib.Game.Character;

[CustomEditor(typeof(ColliderDetector2D))]
public class ColliderDetector2DEditor : Editor<ColliderDetector2D>
{
    public Controller2D controller;
    public Rigidbody2D rigidbody => targetObject.rigidbody;
    public Collider2D collider => targetObject.collider;
    public Vector2 infoOffset = new Vector2(0, 0.5f);
    public Autopilot autopilot;
    private void OnEnable()
    {
        targetObject.Init();
        if (targetObject.TryGetComponent(out controller))
        {
            if (controller.host is Autopilot pilot)
                autopilot = pilot;
        }
    }
    private void OnSceneGUI()
    {
        if (!collider || !rigidbody)
            return;
        Handles.Label(infoOffset+targetObject.top,$"velocity:{rigidbody.velocity}");

        //Handles.DotHandleCap(0, targetObject.front, Quaternion.identity, 0.05f, EventType.Repaint);
        VisualizeAutopilot();
    }
    private void VisualizeAutopilot()
    {
        if (!controller || !autopilot)
            return;
        
    }
}
