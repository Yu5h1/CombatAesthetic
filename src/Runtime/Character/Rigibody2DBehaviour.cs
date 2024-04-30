using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Rigidbody2DBehaviour : MonoBehaviourEnhance
{
    [SerializeField]
    private Rigidbody2D _rigidbody;
    public new Rigidbody2D rigidbody => _rigidbody;

    protected virtual void Reset() {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    protected virtual void OnEnable()
    {
        if (!_rigidbody)
            _rigidbody = GetComponent<Rigidbody2D>();
    }

}
