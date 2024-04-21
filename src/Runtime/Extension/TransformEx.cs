using UnityEngine;


public static class TransformEx
{
	public static bool TryFind(this Transform t,string name, out Transform result)
        => result = t.Find(name);
	public static bool TryGetComponentInChildren<T>(this Transform t,string name,out T component) where T : Object
	{
        component = null;
        return t.TryFind(name, out Transform child) && child.TryGetComponent(out component);
    }
    
}