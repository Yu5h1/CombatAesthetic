using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class Patrol : MonoBehaviour
{
    public Transform target;
    public static float arriveRange = 1;

    public float RangeDistance;
    //[SerializeField]
    //private Vector2 _DirectionScale = Vector2.one;
    //public Vector2 directionScale => _DirectionScale;

    public ColliderScanner2D scanner;

    public bool SetOffsetOnStart = true;
    [SerializeField]
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
        Init();
        if (!scanner.collider)
            scanner.FindCollider(transform);
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
        scanner.layerMask = LayerMask.GetMask("Character", "PhysicsObject");
        scanner.Tag.tag = "Player,Line";
        scanner.Tag.type = TagOption.ComparisionType.Equal;
        scanner.filter.useTriggers = true;
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

#if UNITY_EDITOR
    [ContextMenu(nameof(ScanTest))]
    private void ScanTest()
    {
        $"{scanner.Scan(out RaycastHit2D result)} {result}".print();
    }
    private void OnDrawGizmos()
    {
        var originColor = Gizmos.color;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(offset, 0.1f);

        Gizmos.color = originColor;

        if (target)
            Debug.DrawLine(scanner.start, target.position, Color.red);
    }

    private void OnDrawGizmosSelected()
    {
        if (!route.points.IsValid(current))
            return;
        Gizmos.DrawWireSphere(Destination, arriveRange);
    }

#endif
}
