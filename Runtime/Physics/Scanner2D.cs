using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

[System.Serializable]
public class Scanner2D : BaseMonoBehaviour
{
    [SerializeField, ReadOnly]
    private Transform _target;
    public Transform target => _target;

    [SerializeField] private CollierCastInfo2D castInfo;
    [SerializeField] private TagOption Tag;
    [SerializeField] private bool InfiniteDistance;
    [SerializeField] private Vector2 direction;
    [SerializeField] private LayerMask ObstacleMask;
    [ReadOnly]
    private Vector2 _offset;
    public Vector2 offset => useCircleCast ? transform.position : transform.TransformPoint(_offset);
#pragma warning disable 0109
    public new Collider2D collider => castInfo.collider;
#pragma warning restore 0109
    public RaycastHit2D[] results => castInfo.results;
    public float distance => castInfo.distance;

    public Vector2 size { get; private set; }
    [SerializeField]
    private bool _useCircleCast;
    public bool useCircleCast
    {
        get => _useCircleCast;
        private set => _useCircleCast = value;
    }


    public RaycastHit2D result =>  results.IsValid(resultIndex) ? results[resultIndex] : default;

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
            _target = result.transform;
            //(target ? (last ? changed : detected) : lost)?.Invoke(last);
        }
    }

    //public event UnityAction<Transform> detected;
    //public event UnityAction<Transform> changed;
    //public event UnityAction<Transform> lost;

    private void Reset()
    {
        if (Tag == null)
            Tag = new TagOption();
        Tag.tag = "Player";
        Tag.type = TagOption.ComparisionType.Equal;
        if (castInfo == null)
            castInfo = new CollierCastInfo2D();
        castInfo.filter.useTriggers = false;
        castInfo.filter.useLayerMask = true;
        castInfo.layerMask = LayerMask.GetMask("Character");
        ObstacleMask = LayerMask.GetMask("PhysicsObject");
        direction = Vector2.zero;
        useCircleCast = true;
        castInfo.distance = 10;

        if (transform.TryGetComponentInChildren<Collider2D>("scanner", out Collider2D collider))
            castInfo.collider = collider;
    }

    public void Init()
    {
        castInfo.Init();
        if (collider)
        {
            collider.isTrigger = true;
            _offset = collider.transform.InverseTransformPoint(collider.GetPoint(collider.transform.TransformDirection(-direction)));
            size = collider.GetSize();
        }
        
    }

    private void Start()
    {
        Init();
    }
    private int Cast(Vector2 dir)
    {
        if (!useCircleCast && collider)
            return castInfo.Cast(dir);
        return InfiniteDistance ?
               Physics2D.CircleCast(transform.position, castInfo.distance, Vector2.zero, castInfo.filter, results) :
               Physics2D.CircleCast(transform.position, castInfo.distance, Vector2.zero, castInfo.filter, results, castInfo.distance);
    }

    public bool Scan(out RaycastHit2D hit)
    {
        hit = default(RaycastHit2D);
        var dir = direction.IsZero() ? Vector2.zero : (Vector2)transform.TransformDirection(direction);

        for (int i = 0; i < Cast(dir); i++)
        {
            if (Tag.Compare(results[i].transform.gameObject))
            {
                var closetpoint = results[i].collider.ClosestPoint(offset);
#if UNITY_EDITOR
                Debug.DrawLine(offset, closetpoint, Color.yellow);
#endif
                var obstacleHit = default(RaycastHit2D);
                if (ObstacleMask.value != 0)
                {
                    obstacleHit = Physics2D.Linecast(offset, closetpoint, ObstacleMask);
#if UNITY_EDITOR
                    if (obstacleHit)
                    {
                        Debug.DrawLine(offset, obstacleHit.point, Color.blue);
                        //$"{collider.transform.parent.name} obstacleHit:({obstacleHit.collider.name}) from scanner".print();
                    }
#endif
                }
                if (!obstacleHit)
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


}
