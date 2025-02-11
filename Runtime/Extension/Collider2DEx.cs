using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Yu5h1Lib;
using Yu5h1Lib.Runtime;

public static class Collider2DEx
{
	public static Vector2 GetSize(this Collider2D c) => c switch
    {
        CapsuleCollider2D capsule => capsule.size,
        BoxCollider2D box => box.size,
        CircleCollider2D circle => Vector2.one * circle.radius,
        _ => c.bounds.size
    };
    public static Vector2 GetPoint(this Collider2D c,Vector2 direction) => c switch
    {
        CapsuleCollider2D capsule => GetCapsulePoint(capsule, direction),
        BoxCollider2D box => GetBoxPoint(box, direction),
        CircleCollider2D circle => GetCirclePoint(circle, direction),
        _ => c.ClosestPoint((Vector2)c.bounds.center + (direction * c.bounds.size.magnitude * 2))
    };
    public static bool CompareLayer(this Collider2D col, LayerMask layerMask) 
        => col.gameObject.CompareLayer(layerMask);
    public static bool CompareLayer(this Collider2D col, string layerName)
        => col.gameObject.CompareLayer(layerName);

    private static Vector2 GetBoxPoint(BoxCollider2D box, Vector2 direction)
    {
        var offset = box.offset;
        var size = box.size * 0.5f;
        var center = (Vector2)box.transform.TransformPoint(offset);

        var rotation = box.transform.rotation;
        var localDirection = Quaternion.Inverse(rotation) * direction;

        Vector2 localPoint = new Vector2(
            localDirection.x > 0 ? size.x : -size.x,
            localDirection.y > 0 ? size.y : -size.y
        );

        float scale = Mathf.Min(
            Mathf.Abs(size.x / localDirection.x),
            Mathf.Abs(size.y / localDirection.y)
        );

        Vector2 worldPoint = center + (Vector2)(rotation * (localDirection * scale));
        return worldPoint;
    }

    private static Vector2 GetCirclePoint(CircleCollider2D circle, Vector2 direction)
    {
        var offset = circle.offset;
        var radius = circle.radius;
        var center = (Vector2)circle.transform.TransformPoint(offset);

        return center + direction.normalized * radius;
    }

    private static Vector2 GetCapsulePoint(CapsuleCollider2D capsule, Vector2 direction)
    {
        // 取得 Capsule 的幾何數據
        var offset = capsule.offset;
        var size = capsule.size * 0.5f; // 半寬半高
        var center = (Vector2)capsule.transform.TransformPoint(offset);

        direction = direction.normalized; // 確保方向是單位向量

        if (capsule.direction == CapsuleDirection2D.Vertical)
        {
            float radius = size.x; // 半徑取寬度一半
            float height = size.y - 2 * radius; // 中心與圓心距離
            Vector2 localDir = new Vector2(direction.x, Mathf.Abs(direction.y)); // 修正方向

            // 垂直 Capsule
            if (direction.y > 0) // 朝上
            {
                Vector2 topCenter = center + Vector2.up * height; // 上圓心
                return topCenter + direction.normalized * radius;
            }
            else if (direction.y < 0) // 朝下
            {
                Vector2 bottomCenter = center + Vector2.down * height; // 下圓心
                return bottomCenter + direction.normalized * radius;
            }
            else // 水平
            {
                return center + direction * radius;
            }
        }
        else // Horizontal Capsule
        {
            float radius = size.y; // 半徑取高度一半
            float width = size.x - 2 * radius; // 中心與圓心距離
            Vector2 localDir = new Vector2(Mathf.Abs(direction.x), direction.y); // 修正方向

            // 水平 Capsule
            if (direction.x > 0) // 朝右
            {
                Vector2 rightCenter = center + Vector2.right * width; // 右圓心
                return rightCenter + direction.normalized * radius;
            }
            else if (direction.x < 0) // 朝左
            {
                Vector2 leftCenter = center + Vector2.left * width; // 左圓心
                return leftCenter + direction.normalized * radius;
            }
            else // 垂直
            {
                return center + direction * radius;
            }
        }
    }

    #region MyRegion

    #endregion
}