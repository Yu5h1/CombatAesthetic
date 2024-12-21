using UnityEngine;


public static class TransformEx
{
    public static void SetParentAndUnitScale(this Transform t,Transform parent,bool worldPositionStays = true)
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
            t.localScale = Vector3.one;
    }
}