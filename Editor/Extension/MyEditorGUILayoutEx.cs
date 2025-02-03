using UnityEditor;
using UnityEngine;
using Yu5h1Lib.EditorExtension;

public static class MyEditorGUILayoutEx
{
	public static bool TrySlider<T>(this Editor<T> c,string label,float current,float left,float right, out float result) where T : Object
        => (result = EditorGUILayout.Slider(label, current, left, right)) != current;

    public static bool TrySlider<T>(this Editor<T> c, string label,ref float current, float left, float right) where T : Object
    {
        var val = EditorGUILayout.Slider(label, current, left, right);
        if (val == current)
            return false;
        current = val;
        return true;
    }
}