using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib.Game.Character;
using Yu5h1Lib.Runtime;

[CustomEditor(typeof(ColliderDetector2D))]
public class ColliderDetector2DEditor : Editor<ColliderDetector2D>
{
    Transform transform => targetObject.transform;
    public CharacterController2D controller;
    public ColliderScanner2D scanner => targetObject.scanner;
    public Rigidbody2D rigidbody => targetObject.rigidbody;
    public Collider2D collider => targetObject.collider;
    public Vector2 infoOffset = new Vector2(0, 0.5f);
    public Autopilot autopilot;
    protected void OnEnable()
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
        if (!InternalEditorUtility.GetIsInspectorExpanded(target) || !targetObject.isActiveAndEnabled)
            return;
        if (scanner.useCircleCast)
            DebugUtil.DrawCircle(transform.position, Quaternion.identity, scanner.distance);
        if (!collider || !rigidbody)
            return;
        Handles.Label(infoOffset+targetObject.top,$"velocity:{rigidbody.velocity}");

        //Handles.DotHandleCap(0, targetObject.front, Quaternion.identity, 0.05f, EventType.Repaint);
        
    }

}
