

using UnityEngine;

public static class LayerMaskEx
{
	public static bool Contains(this LayerMask layerMask,GameObject gameObject)
         => ((1 << gameObject.layer) & layerMask.value) != 0;
}