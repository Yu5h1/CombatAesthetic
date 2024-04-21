using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Tilemaps.Tile;

public static class ComponentEx 
{
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
