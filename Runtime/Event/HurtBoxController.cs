using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HurtBoxController : MonoBehaviour
{
    public Collider2D[] colliders;

    public LayerMask HurtLayer => 1 << LayerMask.NameToLayer("HurtBox");


    [SerializeField]
    private UnityEvent<Collider2D> _Hurt;
    public event UnityAction<Collider2D> hurt
    {
        add => _Hurt.AddListener(value);
        remove => _Hurt.RemoveListener(value);
    }

    // Start is called before the first frame update
    void Start()
    {
        var list = new List<Collider2D>();
        foreach (var c in GetComponentsInChildren<Collider2D>())
        {
            if (HurtLayer.Contains(c.gameObject))
                list.Add(c);
        }
        colliders = list.ToArray();
    }

    public void OnHurt(Collider2D collider)
    {
        _Hurt?.Invoke(collider);
    }
}
