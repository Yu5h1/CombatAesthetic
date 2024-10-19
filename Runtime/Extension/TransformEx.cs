using UnityEngine;


public static class TransformEx
{
	public static bool TryFind(this Transform t,string name, out Transform result)
        => result = t.Find(name);
    #region 2D
    public static Vector3 TransformPoint(this Transform t,float x,float y) => t.TransformPoint(new Vector3(x, y));
    #endregion
}