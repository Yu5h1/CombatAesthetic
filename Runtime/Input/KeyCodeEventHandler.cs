using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;



public class KeyCodeEventHandler : MonoBehaviour
{
    private KeyCodeEventManager _manager;
    public KeyCodeEventManager manager
    { 
        get{ 
            if (_manager == null)
                _manager = GetComponentInParent<KeyCodeEventManager>();
            if (_manager == null)
                _manager = GetComponent<KeyCodeEventManager>();
            return _manager;
        }
    }
    public KeyCode keyCode;
    public KeyState State;
    public UnityEvent Event;

    private void Awake()
    {
        _manager = GetComponentInParent<KeyCodeEventManager>();
    }
    private void OnEnable()
    {
        if (!manager)
            return;
        manager.handlers.Add(this);
    }
    public void Handle()
    {
        if (State == KeyState.Down && Input.GetKeyDown(keyCode) ||
             State == KeyState.Hold && Input.GetKey(keyCode) ||
             State == KeyState.Up && Input.GetKeyUp(keyCode))
            Event?.Invoke();
    }
    private void OnDisable()
    {
        if (!manager)
            return;
        manager.handlers.Remove(this);
    }
}
