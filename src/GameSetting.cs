using SerializableAttribute = System.SerializableAttribute;
using UnityEngine;

public class GameSetting : ScriptableObject
{
	[Serializable]
    public class UIOptions
	{
		public bool FadeTransition;

	}
	public UIOptions UI;
}
