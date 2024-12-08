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
    public static bool TryFind(this Transform t,string name, out Transform result)
        => result = t.Find(name);
    #region 2D
    public static Vector3 TransformPoint(this Transform t,float x,float y) => t.TransformPoint(new Vector3(x, y));
    #endregion
}