using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib.Game.Character;

public class ImpactForce2D : MonoBehaviour
{
    [SerializeField]
    private Vector2 _force = new Vector2(0, 1);
    public Vector2 force => _force * forceMultiplier;
    [SerializeField]
    private float forceMultiplier = 1.0f;
    [SerializeField]
    private Vector2 _offset = new Vector2(0, 0.5f);
    public Vector2 offset => transform.rotation * _offset;
    public Vector2 impactPos => ((Vector2)transform.position) + offset;
    [SerializeField]
    private Vector2 _size = Vector2.one;
    public Vector2 size => _size * transform.localScale;



    public LayerMask validlayers { get; private set; }
    private void Start() {
        validlayers = LayerMask.GetMask("Character", "PhysicsObject");

    }
    public void Push(Vector3 force)
    {
        if (!enabled)
            return;
        force *= forceMultiplier;
        var results = Physics2D.OverlapBoxAll(impactPos, size, 0, validlayers);
        for (int i = 0; i < results.Length; i++)
        {
            var col = results[i];
            if (col.TryGetComponent(out AnimatorCharacterController2D controller))
                controller.AddForce(force);
            else if (col.TryGetComponent(out Rigidbody2D otherRigidbody) && otherRigidbody.transform.root != transform.root)
                otherRigidbody.AddForce(force);
        }
    }
    public void Impact(Vector3 velocity) => Push(-velocity);
    public void Impact() => Push(force);

    [ContextMenu(nameof(PushTest))]
    public void PushTest()
    {
        Push(Vector2.up * 5);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled)
            return;
        var originalMatrix = Gizmos.matrix;
        DrawGizmosWireCube(size, _offset, transform, Color.magenta);
        Gizmos.matrix = originalMatrix;

    }
    void DrawGizmosWireCube(Vector3 CubeSize, Vector3 offset, Transform target, Color color)
    {
        Gizmos.color = color;
        Gizmos.matrix = Matrix4x4.TRS(target.position, target.rotation, target.lossyScale);
        Gizmos.DrawWireCube(offset, CubeSize);
    }
#endif
}
