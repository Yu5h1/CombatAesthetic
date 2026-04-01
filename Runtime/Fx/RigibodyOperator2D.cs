using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib.Game.Character;

public class RigibodyOperator2D : Rigidbody2DBehaviour
{
    public void Stop()
    {
        velocity = Vector3.zero;
        rigidbody.angularVelocity = 0;
    }
    public void SetVelocity(Vector2 v)
    {
        velocity = transform.TransformDirection(v);
    }
    public void AddVelocityX(float X)
    {
        velocity += (Vector2)transform.TransformDirection(new Vector3(X,0,0));
    }
    public void AddVelocityY(float Y)
    {
        velocity += (Vector2)transform.TransformDirection(new Vector3(0, Y, 0));
    }
    public void SetVelocityX(float X)
    {
        velocity = (Vector2)transform.TransformDirection(new Vector3(X, 0, 0));
    }
    public void SetVelocityY(float Y)
    {
        velocity = (Vector2)transform.TransformDirection(new Vector3(0, Y, 0));
    }
    public void AddForceToContacts()
    {
        
        var colliders = new Collider2D[0];
        var count = rigidbody.GetContacts(colliders);
        for (int i = 0; i < count; i++)
        {
            if (colliders[i].TryGetComponent(out AnimatorCharacterController2D controller))
            {
                controller.AddForce(velocity);
                Debug.Log($"{controller.name} add force by rigibodyOperator");
            }
            else if (colliders[i].TryGetComponent(out Rigidbody2D otherRigidbody) && otherRigidbody.transform.root != transform.root)
            {
                otherRigidbody.AddForce(velocity);
            }
        }
    }
    public void LogContacts()
    {
        var colliders = new Collider2D[0];
        var count = rigidbody.GetContacts(colliders);
        Debug.Log($"count:{count}" + string.Join(',', colliders.Select(c => c.name)));
    }
}
