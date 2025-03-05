using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Rigidbody2DBehaviour : MonoBehaviour
{

    [SerializeField]
    private Rigidbody2D _rigidbody;
#pragma warning disable 0109
    public new Rigidbody2D rigidbody => _rigidbody;
#pragma warning restore 0109
    public virtual Vector2 velocity { get => rigidbody.velocity; protected set => rigidbody.velocity = value; }
    public Vector2 up => (Vector2)transform.up;
    public Vector2 down => -up;
    public Vector2 right => transform.right;
    public Vector2 left => -right;

    protected virtual void Reset() {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    protected virtual void OnEnable()
    {
        if (!_rigidbody)
            _rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }
    protected virtual void OnDisable()
    {
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
    }
    
}
