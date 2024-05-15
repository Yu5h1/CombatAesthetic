using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game.Character;

public class Force2D : MonoBehaviour
{
    public Vector2 force;
    public Dictionary<Collider2D, AnimatorController2D> catchedObjects = new Dictionary<Collider2D, AnimatorController2D>(); 
    public void FixedUpdate()
    {
        if (force == Vector2.zero)
            return;
        foreach (var item in catchedObjects)
            item.Value.AddForce(force * Time.fixedDeltaTime);
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out AnimatorController2D controller))
            catchedObjects.Add(collider,controller);
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (catchedObjects.ContainsKey(collider))
            catchedObjects.Remove(collider);
    }
}
