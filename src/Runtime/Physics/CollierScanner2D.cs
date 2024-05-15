using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollierScanner2D : CollierCastInfo2D
{
	public TagOption Tag;
	public Vector2 direction;

    public bool Scan(out Collider2D target)
    {
        target = null;
        if (!collider) {
            "The collider of CollierScanner2D is not assigned".LogWarning();
            return false;
        }
        if (direction == Vector2.zero)
			return false;
		for (int i = 0; i < Cast(collider.transform.TransformDirection(direction)); i++)
		{
            if (Tag.Compare(results[i].transform.tag))
            {
                target = results[i].collider;
                return true;
            }
        }
        return false;
    }
}
