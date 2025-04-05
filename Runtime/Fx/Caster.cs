
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Runtime;
using Serializable = System.SerializableAttribute;

[DisallowMultipleComponent]
public class Caster : BaseMonoBehaviour
{
    [System.Flags]
    public enum TargetingMode
    {
        None = 0,
        Position = 1 << 0,
        Ground = 1 << 1,
        Aim = 1 << 2,
        EnvironmentSurface = 1 << 3,
        FaceTo = 1 << 4,
    }
    [Serializable]
    public class Info
    {
        [AutoFill(typeof(AutoFillResources), "Fx")]
        public string source;
        public TargetingMode targetingMode;
        public Direction2D direction = Direction2D.right;
    }
 


    [SerializeField,ReadOnly]
    private Scanner2D _scanner;
    private Scanner2D scanner => _scanner;

    [SerializeField]
    private CasterData[] datas;

    private void Reset()
    {
        Init();
    }
    protected override void OnInitializing()
    {
        this.GetComponent(ref _scanner);
    }
    private void Start() { }
    private void OnEnable() { }
    #region create by data
    [ContextMenu(nameof(Cast))]
    public void Cast()
    {
        if (!scanner || datas.IsEmpty())
            return; 
        var data = datas.RandomElement();
        if (!data)
            return;
        if (data.scaleMultiplier.Length == 0 && data.scaleMultiplier.Min >= 0.1f)
            return;
        if (!scanner.Scan(out RaycastHit2D hit) )
            return;
        var fx = Cast(data.info, transform.position, transform.rotation, hit.transform);
        if (fx == null)
            return;
        var s = Random.Range(data.scaleMultiplier.Min, data.scaleMultiplier.Max);
        fx.localScale = new Vector3(s, s, 1);
    }
    #endregion


    public void Cast(Info info, Vector3 defaultPos, Quaternion defaultRot)
    {
        if (scanner.Scan(out RaycastHit2D hit))
            Cast(info, defaultPos, defaultRot, hit.transform);
    }
    public Transform Cast(Info info,Vector3 defaultPos, Quaternion defaultRot, Transform target)
    {
        switch (info.targetingMode)
        {
            case TargetingMode.Position:

                defaultPos = target.transform.position;
                defaultRot = Quaternion.identity;
                break;
            case TargetingMode.Ground:
                defaultPos = target.transform.position;
                if (GetGroundHeight(defaultPos, -target.transform.up, scanner.distance, out float height))
                    defaultPos.y = height;
                else
                {
                    "CastFX Failed cause no Ground !".printWarning();
                    return null;
                }
                defaultRot = Quaternion.identity;
                break;
            case TargetingMode.EnvironmentSurface:
                if (GetEnvironmentSurfacePoint(target.transform.position, scanner.distance,out RaycastHit2D hit))
                {

                    defaultRot = hit.normal.DirectionToQuaternion2D(info.direction);
                    defaultPos = hit.point;

                }
                else
                {
                    "CastFX Failed cause no Surface !".printWarning();
                    return null;
                }
                break;
            case TargetingMode.Aim:
                defaultRot = GetQuaternionToResult(target);
                break;
            default:
                break;
        }
        return Retrieve(info.source, defaultPos, defaultRot);
    }

    public Vector3 GetDirectionToResult(Transform target) => target.position - transform.position;
    public Quaternion GetQuaternionToResult(Transform target) => ((Vector2)GetDirectionToResult(target)).DirectionToQuaternion2D();

    public Transform Retrieve(string source, Vector3 pos, Quaternion rot)
    {
        var fx = PoolManager.Spawn<Transform>(source, pos, rot);
        foreach (var mask in fx.GetComponents<EventMask2D>())
        {
            mask.Filter.tagOption.tag = gameObject.tag;
            mask.Filter.tagOption.mode = TagOption.FilterMode.Exclude;
        }
        var availableColliders = GameManager.instance.characters.
            Where( c => c.IsAvailable() && c.transform != transform && c.detector.IsAvailable() && c.detector.collider != null ).
            Select(d => d.detector.collider);
        
        foreach (var p in fx.GetComponents<ParticleSystemEvent>())
            p.SetTriggerList2D(availableColliders);
        foreach (var p in fx.GetComponentsInChildren<ParticleSystemEvent>())
            p.SetTriggerList2D(availableColliders);

        return fx;
    }

    #region Static
    public static bool GetGroundHeight(Vector2 pos, Vector2 groundDir, float distance, out float height)
    {
        height = 0f;
        if (groundDir.IsZero())
            return false;
        var hit = Physics2D.Raycast(pos, groundDir.normalized, distance, LayerMask.GetMask("PhysicsObject"));
        if (!hit)
            return false;
        height = hit.point.y;
        return true;
    }
    public static bool GetEnvironmentSurfacePoint(Vector2 origin, float distance,out RaycastHit2D result)
    {
        
        var upHit = Physics2D.Raycast(origin, Vector2.up,distance, LayerMask.GetMask("PhysicsObject"));
        var downHit = Physics2D.Raycast(origin, Vector2.down, distance, LayerMask.GetMask("PhysicsObject"));
        var leftHit = Physics2D.Raycast(origin, Vector2.left, distance, LayerMask.GetMask("PhysicsObject"));
        var rightHit = Physics2D.Raycast(origin, Vector2.right, distance, LayerMask.GetMask("PhysicsObject"));
        
        result = default;
        if (upHit || downHit || leftHit || rightHit)
        {
            if (upHit)
                result = upHit;

            if (downHit && (!result || downHit.distance < result.distance))
                result = downHit;

            if (leftHit && (!result || leftHit.distance < result.distance))
                result = leftHit;

            if (rightHit && (!result || rightHit.distance < result.distance))
                result = rightHit;
            //Debug.DrawLine(origin, result.point);
            Debug.DrawLine(result.point, result.point + result.normal);
            return true; 
        }
        return false;
    }
    #endregion

    public Vector2 SnapDirection(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle -= 90;
        if (angle < 0) angle += 360;
        if (angle >= 325 || angle < 45)
            return Vector2.up;
        else if (angle >= 45 && angle < 135)
            return Vector2.right;
        else if (angle >= 135 && angle < 225)
            return Vector2.down;
        else
            return Vector2.left;
    }
}
