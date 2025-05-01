using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;


public class Collider2DAgent : MonoBehaviour
{
    [SerializeField]
    private Collider2D _collider;
#pragma warning disable 0109
    public new Collider2D collider { get => _collider; protected set => _collider = value; }
#pragma warning restore 0109

    [SerializeField]
    private Tags ignoreCollisionByTags;
    private void Start()
    {
        if (_collider)
            foreach (var obj in ignoreCollisionByTags.FindMatchedGameObjects())
                foreach (var col in obj.GetComponents<Collider2D>())
                    Physics2D.IgnoreCollision(collider,col);
    }

 
}
