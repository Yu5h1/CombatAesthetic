using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class Collider2DEx
{
	public static Vector2 GetSize(this Collider2D c) => c switch
    {
        CapsuleCollider2D capsule => capsule.size,
        BoxCollider2D box => box.size,
        CircleCollider2D circle => Vector2.one * circle.radius,
        _ => Vector2.zero
    };
}