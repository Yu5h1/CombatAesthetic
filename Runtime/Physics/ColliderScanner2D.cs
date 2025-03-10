using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    public bool useCircleCast
    {
        get => _useCircleCast || !collider.IsAvailable();
        set => _useCircleCast = value;
    }

    public bool InfiniteDistance;


    public RaycastHit2D result => results.IsValid(resultIndex) ? results[resultIndex] : default;
    [SerializeField, ReadOnly]
    private Transform _target;
    public Transform target { get => _target; set => _target = value; }
    private int _resultIndex = -1;
    public int resultIndex
    {
        get => _resultIndex;
        protected set
        {
            
            if (_resultIndex == value)
                return;
            Transform last = result.transform;
            _resultIndex = value;
            target = result.transform;
            //(target ? (last ? changed : detected) : lost)?.Invoke(last);
        }
    }

    //public event UnityAction<Transform> detected;
    //public event UnityAction<Transform> changed;
    //public event UnityAction<Transform> lost;


    public override void Init()
    {                    
        base.Init();
        if (!collider)
            return;
        collider.isTrigger = true;
        _localStart = collider.transform.InverseTransformPoint(collider.GetPoint(collider.transform.TransformDirection(-direction)));
        size = collider.GetSize();
    }
    public void Init(Transform transform)
    {
        _transform = transform;
        Init();
    }
    private int Scan(Vector2 dir){
        if (!useCircleCast && collider)
            return Cast(dir);
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
                var closetpoint = results[i].collider.ClosestPoint(start);
#if UNITY_EDITOR
                    Debug.DrawLine(start, closetpoint, Color.yellow);
#endif
                var obstacleHit = default(RaycastHit2D);
                if (ObstacleMask.value != 0)
                {
                    obstacleHit = Physics2D.Linecast(start, closetpoint, ObstacleMask);
#if UNITY_EDITOR
                    if (obstacleHit)
                    {
                        Debug.DrawLine(start, obstacleHit.point, Color.blue);
                        //$"{collider.transform.parent.name} obstacleHit:({obstacleHit.collider.name}) from scanner".print();
                    }
#endif
                }
                if (!obstacleHit )
                {

                    hit = results[i];
                    resultIndex = i;

                    return true;
                }
            }
        }
        resultIndex = -1;
        return false;
    }
    public bool GetGroundHeight(Vector2 pos, Vector2 groundDir,out float height)
    {
        height = 0f;
        if (groundDir.IsZero())
            return false;
        var hit = Physics2D.Raycast(pos, groundDir.normalized, distance * 2, LayerMask.GetMask("PhysicsObject"));
        if (!hit)
            return false;
        height = hit.point.y; 
        return true;
    }
    public Vector3 GetDirectionToResult() => target.transform.position - transform.position;
    public Quaternion GetQuaternionToResult() => ((Vector2)GetDirectionToResult()).DirectionToQuaternion2D();
}
