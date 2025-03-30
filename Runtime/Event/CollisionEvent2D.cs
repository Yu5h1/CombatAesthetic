using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class CollisionEvent2D : EventMask2D
{
    [SerializeField]
    private UnityEvent<Collision2D> _CollisionEnter;
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!Validate(collision.gameObject.transform))
            return;
        _CollisionEnter?.Invoke(collision);
    }
    public void log(Collider2D collider) => collider.name.print();
}
public class CollisionEvent2D<T> : BaseMonoBehaviour where T : Behaviour
{
    [Serializable]
    public class TEvent : UnityEvent<T> { }

    [SerializeField]
    private TEvent _CollisionEnter;

    protected override void OnInitializing() { }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsAvailable() || !collision.gameObject.TryGetComponent(out T behaviour))
            return;
        _CollisionEnter?.Invoke(behaviour);
    }

    
}