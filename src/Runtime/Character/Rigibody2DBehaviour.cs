using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rigidbody2DBehaviour : MonoBehaviourEnhance
{
    public new Rigidbody2D rigidbody { get; private set; }

    protected virtual void Reset() {}
    protected virtual void OnEnable()
    {
        rigidbody = rigidbody ?? GetComponent<Rigidbody2D>();
    }

}
