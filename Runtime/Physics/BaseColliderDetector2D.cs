using UnityEngine;
using Yu5h1Lib;

public class BaseColliderDetector2D : Rigidbody2DBehaviour , ITransform2D
{
    #region field
    [SerializeField]
    protected Collider2D _collider;
#pragma warning disable 0109
    public new Collider2D collider { get => _collider; protected set => _collider = value; }
#pragma warning restore 0109

    public ContactFilter2D filter;
    public LayerMask layerMask { get => filter.layerMask; set => filter.layerMask = value; }
    public RaycastHit2D[] results;
    [Range(0, 1.0f)]
    public float distance = 0.2f;
    [SerializeField, Range(1, 10)]
    private int _resultsCount = 5;
    public int ResultsCount { get => _resultsCount; protected set => _resultsCount = value; }
    public bool ignoreSiblingColliders = true;
    #endregion

    #region property
    public Vector2 position => rigidbody == null ? transform.position : rigidbody.position;
    public Vector2 offset => _collider.offset;
    public Vector2 extents { get; private set; }
    public Vector2 center => position + (transform.right * new Vector2(offset.x, 0)) + (transform.up * new Vector2(0, offset.y));
    public Vector2 front => center + (right * extents.x);
    public Vector2 top => center + (up * extents.y);
    public Vector2 bottom => center + (down * extents.y);


    #endregion

    protected override void Reset()
    {
        base.Reset();
        _collider = GetComponent<CapsuleCollider2D>();
    }
    protected override void OnInitializing()
    {
        base.OnInitializing();
        if (!_collider)
        {
            Debug.LogWarning($"({name})'s collider of decteor was unassigned.");
            return;
        }
        extents = _collider.GetSize() * 0.5f;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="Count">results count</param>
    public virtual int Cast(Vector2 direction)
    {
        results = new RaycastHit2D[ResultsCount];
        if (!collider)
            return 0;
        return collider.Cast(direction, filter, results, distance, ignoreSiblingColliders);
    }

}
