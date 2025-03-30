using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class CollisionController2D : Rigidbody2DBehaviour
{
    public float pushForce = 2f;
    [SerializeField]
    private LayerMask layer;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!layer.Contains(collision.gameObject.layer))
            return;
        var otherRb = collision.rigidbody;
        if ( collision.contactCount <= 0)
            return;
        rigidbody.AddForce(collision.GetContact(0).normal * pushForce, ForceMode2D.Impulse);
    }
}
