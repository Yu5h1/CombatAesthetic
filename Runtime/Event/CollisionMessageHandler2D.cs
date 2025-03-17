using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionMessageHandler2D : MonoBehaviour
{
    [SerializeField]
    private string message;
	public void SendOtherMessage(Collider2D other)
	{
        other.SendMessage(message);
    }
}
