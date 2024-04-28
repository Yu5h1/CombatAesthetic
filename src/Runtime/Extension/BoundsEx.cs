
using UnityEngine;

public static class BoundsEx
{
	public static float GetDiagonalLength2D(this Bounds bounds)
		=> Mathf.Sqrt(bounds.size.x * bounds.size.x + bounds.size.y * bounds.size.y);
}