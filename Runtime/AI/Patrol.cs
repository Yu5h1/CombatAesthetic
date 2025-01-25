using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using Yu5h1Lib.Runtime;

public class Patrol : MonoBehaviour
{
    [SerializeField]
    private Transform _target;
    public Transform target{ 
        get => _target;
        set 
        {
            if (_target == value)
                return;
            _target = value;
            if (value)
                _Spotted?.Invoke();
        }
    }
    public static float arriveRange = 1;

    public float RangeDistance;

    public ColliderScanner2D scanner;

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

    [SerializeField]
    protected UnityEvent _Spotted;
    public event UnityAction Spotted
    {
        add => _Spotted.AddListener(value);
        remove => _Spotted.RemoveListener(value);
    }
    private void Reset()
    {
        
    }
    private void Start()
    {
        Init();
        scanner.transform = transform;
        scanner.Init();
    }
    public void Init()
    {
        if (SetOffsetOnStart)
            offset = transform.position;
        offsetQ = UseLocalCoordinate ? transform.rotation : Quaternion.identity;
    }
    [ContextMenu(nameof(UsedefaultScannerSetting))]
    public void UsedefaultScannerSetting()
    {
        scanner.layerMask = LayerMask.GetMask("Character");
        scanner.Tag.tag = "Player";
        scanner.ObstacleMask = LayerMask.GetMask("PhysicsObject");
        scanner.Tag.type = TagOption.ComparisionType.Equal;
        scanner.filter.useTriggers = false;
        scanner.filter.useLayerMask = true;
        scanner.direction = Vector2.right;
    }
    public Vector2 GetDirection()
        => route.GetDirection(transform.position, offset, offsetQ, ref _current, arriveRange);

    public void MoveNext() => route.MoveNext(ref _current);

    public void SetCurrentPoint(Vector2 position)
    {
        if (route.points.IsValid(current))
            route.points[current] = Quaternion.Inverse(offsetQ) * (position - offset);
    }

    public bool Scan(out RaycastHit2D hit) => scanner.Scan(out hit);

#if UNITY_EDITOR

    public bool debug;
    [ContextMenu(nameof(ScanTest))]
    private void ScanTest()
    {
        $"{Scan(out RaycastHit2D result)} {result}".print();
    }
    private void OnDrawGizmos()
    {
        var originColor = Gizmos.color;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(offset, 0.1f);

        Gizmos.color = originColor;


    }

    private void OnDrawGizmosSelected()
    {
      
        if (debug){
            if (scanner.transform == null)
            {
                scanner.transform = transform;
                scanner.Init();
            }
            if (scanner.Scan(out RaycastHit2D hit))
            {
                Debug.DrawLine(scanner.start, hit.point,Color.yellow);
            }
                
        }
    }

    public void Visualize()
    {
        if (!route.points.IsValid(current))
            return;
        DebugUtil.DrawCircle(Destination, Quaternion.identity, Patrol.arriveRange);
        if (scanner.useCircleCast)
            DebugUtil.DrawCircle(transform.position, Quaternion.identity, scanner.distance);
    }

#endif
}
