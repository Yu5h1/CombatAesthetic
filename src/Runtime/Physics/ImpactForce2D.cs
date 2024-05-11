using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib.Game.Character;

public class ImpactForce2D : MonoBehaviour
{
    [SerializeField]
    private Vector2 _impactOffset = new Vector2(0, 0.5f);
    public Vector2 impactOffset => transform.rotation * _impactOffset;
    public Vector2 impactPos => ((Vector2)transform.position) + impactOffset;
    [SerializeField]
    private Vector2 _size = Vector2.one;
    public Vector2 size => _size * transform.localScale;

    private void OnDrawGizmos()
    {
        var originalMatrix = Gizmos.matrix;
        DrawGizmosWireCube(size, _impactOffset, transform, Color.magenta);
        Gizmos.matrix = originalMatrix;

    }
    void DrawGizmosWireCube(Vector3 CubeSize, Vector3 offset, Transform target, Color color)
    {
        Gizmos.color = color;
        Gizmos.matrix = Matrix4x4.TRS(target.position, target.rotation, target.lossyScale);
        Gizmos.DrawWireCube(offset, CubeSize);
    }
    public void Push(Vector3 force)
    {
       
        var results = Physics2D.OverlapBoxAll(impactPos, size, 0, 1 << LayerMask.NameToLayer("Character"));
        for (int i = 0; i < results.Length; i++)
        {
            var col = results[i];
            if (col.TryGetComponent(out Controller2D controller))
                controller.AddForce(force);
            else if (col.TryGetComponent(out Rigidbody2D otherRigidbody) && otherRigidbody.transform.root != transform.root)
                otherRigidbody.AddForce(force);
        }
    }
}
