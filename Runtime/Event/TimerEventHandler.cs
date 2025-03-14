using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

[DisallowMultipleComponent]
public class TimerEventHandler : MonoBehaviour
{
    public TimerEvent[] events;

    public bool Initinalized { get; private set; }
    private void Init()
    {
        if (Initinalized)
            return;
        for (int i = 0; i < events.Length; i++)
            events[i].Register();
        Initinalized = true;
    }

    private void Start() { }
    private void OnEnable()
    {
        Init();
        for (int i = 0; i < events.Length; i++)
            events[i].Start();
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < events.Length; i++) 
            events[i].Tick();
    }
    private void OnDisable()
    {
        for (int i = 0; i < events.Length; i++)
            events[i].Stop();
    }

}
