using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyFrameEventHandler : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _events;

    public void InvokeEvents(){
        _events?.Invoke();
    }
}
