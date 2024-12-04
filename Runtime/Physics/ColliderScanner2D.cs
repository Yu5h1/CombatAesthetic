using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

[System.Serializable]
public class ColliderScanner2D : CollierCastInfo2D
{
	public TagOption Tag;
	public Vector2 direction;
    public Vector2 start { get; private set; }

    public void Init()
    {
        start = collider.transform.InverseTransformPoint(collider.GetPoint(direction));
    }

    public bool Scan(out RaycastHit2D hit)
    {
        hit = default(RaycastHit2D);
        if ("The collider of CollierScanner2D is not assigned".printWarningIf(!collider)) 
            return false;
        if (direction == Vector2.zero)
			return false;
		for (int i = 0; i < Cast(collider.transform.TransformDirection(direction)); i++)
		{
            //var obstacleCheck = Physics2D.Raycast(collider.left, directionToEnemy, distanceToEnemy, obstacleMask);

            if (Tag.Compare(results[i].transform.tag))
            {
                hit = results[i];
                return true;
            }
        }
        return false;
    }
}
