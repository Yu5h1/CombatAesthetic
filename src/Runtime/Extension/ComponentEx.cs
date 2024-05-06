using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Tilemaps.Tile;

public static class ComponentEx 
{
    public static bool TryGetComponentInChildren<T>(this Component component,out T result) where T : Component
		=> result = component.GetComponentInChildren<T>();
    public static bool TryGetComponentInChildren<T>(this Component c, string name, out T component) where T : Object
    {
        component = null;
        return c.transform.TryFind(name, out Transform child) && child.TryGetComponent(out component);
    }
    public static T GetOrAdd<T>(this Component c, out T componentParam) where T : Component
	{
		if (!c.TryGetComponent(out componentParam))
			componentParam = c.AddComponent<T>();
		return componentParam;
	}
    public static T GetOrAddIfNull<T>(this Component c, ref T target) where T : Component
		=> target ?? c.GetOrAdd(out target);

	public static bool EqualAnyTag(this Component c, params string[] items)
	{
		foreach (var item in items)
			if (c.CompareTag(item))
				return true;
		return false;
	}

}
