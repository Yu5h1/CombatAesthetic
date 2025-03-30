using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyFrameEventHandler : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _events;

    private void Start() {}
    public void InvokeEvents(){
        if (!isActiveAndEnabled)
            return;
        _events?.Invoke();
    }
}
