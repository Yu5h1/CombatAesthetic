using UnityEditor;
using UnityEngine;
using Yu5h1Lib.EditorExtension;

public static class MyEditorGUILayoutEx
{
	public static bool TrySlider<T>(this Editor<T> c,string label,float current,float left,float right, out float result) where T : Object
        => (result = EditorGUILayout.Slider(label, current, left, right)) != current;
}