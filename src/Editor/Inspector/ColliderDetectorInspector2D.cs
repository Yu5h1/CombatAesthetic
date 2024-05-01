using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yu5h1Lib.Game.Character;

[CustomEditor(typeof(ColliderDetector2D))]
public class ColliderDetectorInspector2D : Editor<ColliderDetector2D>
{
    public Rigidbody2D rigidbody;
    public Collider2D collider;
    public Vector2 infoOffset = new Vector2(0, 0.5f);
    private void OnEnable()
    {
        targetObject.Init();
        targetObject.TryGetComponent(out rigidbody);
        targetObject.TryGetComponent(out collider);
    }
    private void OnSceneGUI()
    {
        if (!rigidbody)
            return;
        Handles.Label(infoOffset+targetObject.top,$"velocity:{rigidbody.velocity}");
    }
}
