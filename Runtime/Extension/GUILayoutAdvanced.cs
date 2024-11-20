using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GUILayoutAdvanced
{
	/// <summary>
	/// return is changed
	/// </summary>
	/// <param name="IsChecked"></param>
	/// <param name="text"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public static bool Toggle(ref bool IsChecked,string text,GUIStyle style, params GUILayoutOption[] options)
	{
		var value = GUILayout.Toggle(IsChecked, text,style ,options);
		if (IsChecked == value)
			return false;
		IsChecked = value;
		return true;
    }
}
