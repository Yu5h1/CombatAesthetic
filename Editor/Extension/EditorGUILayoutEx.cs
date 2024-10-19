using UnityEditor;
using UnityEngine;

public static class EditorGUILayoutEx
{

	public static bool TrySlider<T>(this Editor<T> c,out float result,float current,float left,float right) where T : Object
        => (result = EditorGUILayout.Slider(current, left, right)) != current;
}