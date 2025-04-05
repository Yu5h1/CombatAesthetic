using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

[DisallowMultipleComponent]
public class MonoEventHandler : MonoBehaviour
{
    public UnityEvent OnEnableEvent;
    public UnityEvent OnDisableEvent;

    private void OnEnable()
    {
        if (!enabled)
            return;
        OnEnableEvent?.Invoke();
    }
    private void OnDisable()
    {
        if (!enabled)
            return;
        OnDisableEvent?.Invoke();
    }
}
