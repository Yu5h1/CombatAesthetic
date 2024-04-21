using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rigibody2DController : Rigidbody2DBehaviour
{
    public Vector3 velocity = Vector3.zero;
    protected override void OnEnable()
    {
        base.OnEnable();
        rigidbody.velocity = Quaternion.LookRotation(transform.forward) * velocity;
    }
}
