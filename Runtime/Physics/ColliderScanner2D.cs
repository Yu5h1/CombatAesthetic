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

    private Vector2 _localstart;
    public Vector2 start => collider.transform.TransformPoint(_localstart);
    
    public LayerMask Obstaclelayer;

    public Vector2 size { get; private set; } 

    public void Init(Patrol patrol)
    {
        if (!collider)
            FindCollider(patrol.transform);
        if ($"The Collider of {patrol.name}Scanner does not exist.!".PopupWarnningIf(!collider))
            return;
        _localstart = collider.transform.InverseTransformPoint(collider.GetPoint(-direction));
        size = collider.GetSize();
    }

    public bool Scan(out RaycastHit2D hit)
    {
        hit = default(RaycastHit2D);
        if ("The collider of CollierScanner2D is not assigned".printWarningIf(!collider))
        {
            return false;
        }
        if (direction == Vector2.zero)
			return false;
        var dir = collider.transform.TransformDirection(direction);

        for (int i = 0; i < Cast(dir); i++)
		{
            var obstacleHit = default(RaycastHit2D);
            if (Obstaclelayer.value != 0)
            {
                obstacleHit = Physics2D.Linecast(start, results[i].point, Obstaclelayer);
                if (obstacleHit)
                {
                    Debug.DrawLine(start, collider.transform.position);
                    //"obstacleHit from scanner".print();
                }
            }

            if (!obstacleHit && Tag.Compare(results[i].transform.tag))
            {
                Debug.DrawLine(start, results[i].point);
                hit = results[i];
                return true;
            }
        }
        return false;
    }
}
