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

    public Transform transform { get; private set; }

    [ReadOnly]
    public Vector2 _localStart;
    public Vector2 start => transform.TransformPoint(_localStart);
    
    public LayerMask ObstacleMask;

    public Vector2 size { get; private set; }


    
    public float radius => distance * 10;
    [SerializeField]
    private bool _useCircleCast;
    public bool useCircleCast => _useCircleCast || !collider;

    public void Init(Patrol patrol)
    {
        transform = patrol.transform;

        if (!collider)
            FindCollider(patrol.transform);

        if ($"The Collider of {patrol.name}Scanner does not exist.!".PopupWarnningIf(!collider))
            return;
        if (!collider)
            return;
        transform = collider.transform;
        _localStart = collider.transform.InverseTransformPoint(collider.GetPoint(collider.transform.TransformDirection(-direction)));
        size = collider.GetSize();
    }
    public int Scan(Vector2 dir){
        if (!useCircleCast && collider)
            return Cast(dir);
        results = new RaycastHit2D[resultsCount];
        return Physics2D.CircleCast(transform.position, radius, dir, filter, results);
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
        var dir = (Vector2)transform.TransformDirection(direction);
        //Debug.DrawLine(start, start + (size.x * dir) + (Vector2.up * 0.5f));

        for (int i = 0; i < Scan(dir); i++)
		{
            if (Tag.Compare(results[i].transform.tag))
            {
                var obstacleHit = default(RaycastHit2D);
                if (ObstacleMask.value != 0)
                {
                    obstacleHit = Physics2D.Linecast(start, results[i].point, ObstacleMask);
#if UNITY_EDITOR
                    if (obstacleHit)
                    {
                        Debug.DrawLine(start, obstacleHit.point, Color.blue);
                        //$"{collider.transform.parent.name} obstacleHit:({obstacleHit.collider.name}) from scanner".print();
                    } 
#endif
                }
                if (!obstacleHit)
                {
#if UNITY_EDITOR
                    $" {transform.name} found:{results[i].collider.transform.root.name}".print();
                    Debug.DrawLine(start, results[i].point, Color.yellow); 
#endif
                    hit = results[i];
                    return true;
                }
            }
        }
        return false;
    }
}
