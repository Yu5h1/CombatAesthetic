using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rigibody2DController : Rigidbody2DBehaviour
{
    public Vector2 velocity;
    protected override void OnEnable()
    {
        base.OnEnable();
        rigidbody.velocity = transform.right * velocity.magnitude;
            //velocity.LookAtDirection(transform.right);
    }
}
