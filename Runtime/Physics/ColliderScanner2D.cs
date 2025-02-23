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

    private Transform _transform;
    public Transform transform 
    {
        get => useCircleCast ? _transform : collider.transform;
        set => _transform = value;
    }

    [ReadOnly]
    private Vector2 _localStart;
    public Vector2 start => useCircleCast ? transform.position : transform.TransformPoint(_localStart);
    public LayerMask ObstacleMask;
    public Vector2 size { get; private set; }
    [SerializeField]
    private bool _useCircleCast;
    public bool useCircleCast => _useCircleCast || !collider;

    public bool InfiniteDistance;

    public override void Init()
    {
        base.Init();
        if (!collider)
            return;
        collider.isTrigger = true;
        _localStart = collider.transform.InverseTransformPoint(collider.GetPoint(collider.transform.TransformDirection(-direction)));
        size = collider.GetSize();
    }
    private int Scan(Vector2 dir){
        if (!useCircleCast && collider)
            return Cast(dir);
        results = new RaycastHit2D[resultsCount];
        return InfiniteDistance ? 
                Physics2D.CircleCast(transform.position, distance, Vector2.zero, filter, results):
                Physics2D.CircleCast(transform.position, distance, Vector2.zero, filter, results,distance);
    }

    public bool Scan(out RaycastHit2D hit)
    {
        hit = default(RaycastHit2D);
        var dir = direction.IsZero() ? Vector2.zero : (Vector2)transform.TransformDirection(direction);

        for (int i = 0; i < Scan(dir); i++)
		{
            if (Tag.Compare(results[i].transform.gameObject))
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

                    hit = results[i];
#if UNITY_EDITOR
                    Debug.DrawLine(start, hit.point, Color.yellow);
#endif
                    return true;
                }
            }
        }
        return false;
    }
}
