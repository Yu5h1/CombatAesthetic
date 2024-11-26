using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Yu5h1Lib;
using Yu5h1Lib.Runtime;

public static class Collider2DEx
{
	public static Vector2 GetSize(this Collider2D c) => c switch
    {
        CapsuleCollider2D capsule => capsule.size,
        BoxCollider2D box => box.size,
        CircleCollider2D circle => Vector2.one * circle.radius,
        _ => Vector2.zero
    };
    public static bool CompareLayer(this Collider2D col, LayerMask layerMask) 
        => col.gameObject.CompareLayer(layerMask);
    public static bool CompareLayer(this Collider2D col, string layerName)
    => col.gameObject.CompareLayer(layerName);

    //public static void GetAnchorPoint(this Collider2D c, Direction direction)
    //{

    //    return
    //}
}