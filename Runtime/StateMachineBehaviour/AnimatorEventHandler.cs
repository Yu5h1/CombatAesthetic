
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Serializable = System.SerializableAttribute;

public class AnimatorEventHandler : MonoBehaviour
{
    [Serializable]
    public class EventInfo
	{
		[Range(0,1)]
		public float time;
		public UnityEvent Events;
	}
	[Serializable]
	public class Events
	{
		public string ClipName;
		public List<EventInfo> keyEvents;
	}
	public List<Events> AnimationEvents;

    private void Start()
    {
			
    }
}
