using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;
using Yu5h1Lib.Runtime;

public class Patrol : MonoBehaviour
{
    [SerializeField,ReadOnly]
    private CharacterController2D Body;
    [SerializeField]
    private Transform _target;
    public Transform target{
        get => _target;
        set 
        {
            if (_target == value)
                return;
            _target = value;
        }
    }

    public static float arriveRange = 1;

    public float RangeDistance;

    public bool SetOffsetOnStart = true;
    [SerializeField,ReadOnly]
    private Vector2 _offset;
    public Vector2 offset { get => _offset; private set => _offset = value; }    

    private Quaternion _offsetQ = Quaternion.identity;
    public Quaternion offsetQ { get => UseLocalCoordinate ? _offsetQ : Quaternion.identity; private set => _offsetQ = value; }

    [SerializeField]
    private Route2D _route;
    public Route2D route => _route;

    private int _current;
    public int current => _current;

    public bool UseLocalCoordinate = true;

    public Vector2 Destination => offset + route.points[current].Rotate(offsetQ);

 
    private void Reset()
    {
        
    }
    private void Start()
    {
        TryGetComponent(out Body);
        Init();

    }
    public void Init()
    {
        if (SetOffsetOnStart)
            offset = transform.position;
        offsetQ = UseLocalCoordinate ? transform.rotation : Quaternion.identity;
    }

    public Vector2 GetDirection(bool moveNext, UnityAction nodeArrived)
        => route.GetDirection(Body.detector.collider.ClosestPoint(Destination),
            offset, offsetQ, ref _current, arriveRange, moveNext,
            nodeArrived);

    public void MoveNext() => route.MoveNext(ref _current);

    public void SetCurrentPoint(Vector2 position)
    {
        if (route.points.IsValid(current))
            route.points[current] = Quaternion.Inverse(offsetQ) * (position - offset);
    }

#if UNITY_EDITOR

    public bool debug;

    private void OnDrawGizmos()
    {
        var originColor = Gizmos.color;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(offset, 0.1f);

        Gizmos.color = originColor;


    }
    public void Visualize()
    {
        if (!route.points.IsValid(current))
            return;
        DebugUtil.DrawCircle(Destination, Quaternion.identity, Patrol.arriveRange);
    }

#endif
}
