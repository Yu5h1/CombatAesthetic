using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class TriggerObserver2D : EventMask2D
{
    [SerializeField,ReadOnly]
    private HashSet<Collider2D> colliders = new HashSet<Collider2D>();

    [SerializeField]
    private UnityEvent _FirstColliderEnter;
    public event UnityAction FirstColliderEnter
    {
        add => _FirstColliderEnter.AddListener(value);
        remove => _FirstColliderEnter.RemoveListener(value);
    }
    [SerializeField]
    private UnityEvent _LastColliderExit;
    public event UnityAction LastColliderExit
    {
        add => _LastColliderExit.AddListener(value);
        remove => _LastColliderExit.RemoveListener(value);
    }
    private void Start()
    {
        foreach (var c in GetComponents<Collider2D>())
            c.isTrigger = true;
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!isActiveAndEnabled || !Validate(collider.gameObject))
            return;
        if (colliders.IsEmpty())
            _FirstColliderEnter?.Invoke();
        colliders.Add(collider);
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (NotAllowTriggerExit)
            return;
        colliders.Remove(collider);
        if (colliders.IsEmpty())
            _LastColliderExit?.Invoke();
    }
}
