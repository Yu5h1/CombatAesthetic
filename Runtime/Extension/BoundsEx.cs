
using UnityEngine;

namespace Yu5h1Lib
{
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never), System.ComponentModel.Browsable(false)]
	public static class BoundsEx
	{
		public static float GetDiagonalLength2D(this Bounds bounds)
			=> Mathf.Sqrt(bounds.size.x * bounds.size.x + bounds.size.y * bounds.size.y);
	} 
}