using UnityEngine;


public static class TransformEx
{
    public static void SetParentAndUnitScale(this Transform t,Transform parent,bool worldPositionStays = true,Vector3 ScaleInTopHierachy = default(Vector3))
    {
        t.SetParent(parent, worldPositionStays);
        if (parent)
        {
            var parentScale = parent.lossyScale;
            t.localScale = new Vector3(
                1 / parentScale.x,
                1 / parentScale.y,
                1 / parentScale.z
            );
        }
        else
            t.localScale = ScaleInTopHierachy == Vector3.zero ? Vector3.one : ScaleInTopHierachy;
    }
}