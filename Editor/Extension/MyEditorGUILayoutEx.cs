using UnityEditor;
using UnityEngine;
using Yu5h1Lib.EditorExtension;

public static class MyEditorGUILayoutEx
{
	public static bool TrySlider<T>(this Editor<T> c,out float result,float current,float left,float right) where T : Object
        => (result = EditorGUILayout.Slider(current, left, right)) != current;
}