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
        // ���o Capsule ���X��ƾ�
        var offset = capsule.offset;
        var size = capsule.size * 0.5f; // �b�e�b��
        var center = (Vector2)capsule.transform.TransformPoint(offset);

        direction = direction.normalized; // �T�O��V�O���V�q

        if (capsule.direction == CapsuleDirection2D.Vertical)
        {
            float radius = size.x; // �b�|���e�פ@�b
            float height = size.y - 2 * radius; // ���߻P��߶Z��
            Vector2 localDir = new Vector2(direction.x, Mathf.Abs(direction.y)); // �ץ���V

            // ���� Capsule
            if (direction.y > 0) // �¤W
            {
                Vector2 topCenter = center + Vector2.up * height; // �W���
                return topCenter + direction.normalized * radius;
            }
            else if (direction.y < 0) // �¤U
            {
                Vector2 bottomCenter = center + Vector2.down * height; // �U���
                return bottomCenter + direction.normalized * radius;
            }
            else // ����
            {
                return center + direction * radius;
            }
        }
        else // Horizontal Capsule
        {
            float radius = size.y; // �b�|�����פ@�b
            float width = size.x - 2 * radius; // ���߻P��߶Z��
            Vector2 localDir = new Vector2(Mathf.Abs(direction.x), direction.y); // �ץ���V

            // ���� Capsule
            if (direction.x > 0) // �¥k
            {
                Vector2 rightCenter = center + Vector2.right * width; // �k���
                return rightCenter + direction.normalized * radius;
            }
            else if (direction.x < 0) // �¥�
            {
                Vector2 leftCenter = center + Vector2.left * width; // �����
                return leftCenter + direction.normalized * radius;
            }
            else // ����
            {
                return center + direction * radius;
            }
        }
    }

    #region MyRegion

    #endregion
}