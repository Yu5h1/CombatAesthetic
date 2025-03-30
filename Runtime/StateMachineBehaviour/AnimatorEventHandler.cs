
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using Serializable = System.SerializableAttribute;

public class AnimatorEventHandler : BaseMonoBehaviour
{
    [SerializeField, ReadOnly]
    private Animator animator;
    //   [Serializable]
    //   public class EventInfo
    //{
    //	[Range(0,1)]
    //	public float time;
    //	public UnityEvent Events;
    //}
    //[Serializable]
    //public class Events
    //{
    //	public string ClipName;
    //	public List<EventInfo> keyEvents;
    //}
    //public List<Events> AnimationEvents;

    public UnityEvent<AnimatorStateInfo> stateEnter;
    public UnityEvent<AnimatorStateInfo> stateExit;

    protected override void OnInitializing()
    {
		this.GetComponent(ref animator);
    }

    private void Start()
    {
			
    }
	public void Enter(AnimatorStateInfo info)
	{
		stateEnter?.Invoke(info);
    }
    public void Exit(AnimatorStateInfo info)
    {
        stateExit?.Invoke(info);
    }
	public void Log(AnimatorStateInfo info)
	{
		$"{info.shortNameHash} state".print();
	}
}
