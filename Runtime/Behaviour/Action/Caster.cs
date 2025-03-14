using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Runtime;

//[RequireComponent]
public class Caster : MonoBehaviour
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
    [System.Serializable]
    public class Info
    {
        [AutoFill(typeof(AutoFillResources), "Fx")]
        public string source;
        public TargetingMode mode;
        [FlagsOption(FlagsOptionStyle.Option)]
        public Direction direction = Direction.right;
    }
    [SerializeField]
    private Scanner2D _scanner;
    private Scanner2D scanner => _scanner;
    public Info info;


    public void Init()
    {
        TryGetComponent(out _scanner);
    }
    private void Reset()
    {
        Init();
    }
    private void OnEnable()
    {
        Init();
    }
    [ContextMenu(nameof(Cast))]
    public void Cast()
    {
        if (!scanner)
            return;
        if (scanner.Scan(out RaycastHit2D hit))
            Cast(info, transform.position, transform.rotation, hit.transform);
    }

    public void Cast(Transform target)
    {
        Cast(info, transform.position, transform.rotation, target);
    }
    public void Cast(Info info,Vector3 pos, Quaternion rot, Transform target)
    {
        switch (info.mode)
        {
            case TargetingMode.Position:

                pos = target.transform.position;
                rot = Quaternion.identity;
                break;
            case TargetingMode.Ground:
                pos = target.transform.position;
                if (GetGroundHeight(pos, -target.transform.up, scanner.distance, out float height))
                    pos.y = height;
                else
                {
                    "CastFX Failed cause no Ground !".printWarning();
                    return;
                }
                rot = Quaternion.identity;
                break;
            case TargetingMode.EnvironmentSurface:
                if (GetEnvironmentSurfacePoint(target.transform.position, scanner.distance,out RaycastHit2D hit))
                {

                    rot = hit.normal.DirectionToQuaternion2D(info.direction);
                    pos = hit.point;

                }
                else
                {
                    "CastFX Failed cause no Surface !".printWarning();
                    return;
                }
                break;
            case TargetingMode.Aim:
                rot = GetQuaternionToResult(target);
                break;
            default:
                break;
        }
        Retrieve(info.source, pos, rot);
    }

    public Vector3 GetDirectionToResult(Transform target) => target.position - transform.position;
    public Quaternion GetQuaternionToResult(Transform target) => ((Vector2)GetDirectionToResult(target)).DirectionToQuaternion2D();

    public void Retrieve(string source, Vector3 pos, Quaternion rot)
    {
        var fx = PoolManager.Spawn<Transform>(source, pos, rot);
        foreach (var mask in fx.GetComponents<EventMask2D>())
            mask.owner = transform;
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
