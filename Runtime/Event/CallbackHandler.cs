using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class CallbackHandler 
{
    private UnityAction _callback;
    private List<UnityEvent> Events = new List<UnityEvent>();

    public bool IsEmpty => _callback == null && Events.IsEmpty();

    public void Clear()
    {
        _callback = null; 
        Events.Clear();
    }
    public void Register(params UnityAction[] actions)
    {
        if (actions.IsEmpty())
            return;
        foreach (var action in actions)
        {
            if (_callback == null || !_callback.GetInvocationList().Contains(action))
                _callback += action;
        }
    }

    public void Register(params UnityEvent[] events)
    {
        if (events.IsEmpty())
            return;
        foreach (var e in events)
            Events.Add(e);
    }
    public void Unregister(params UnityAction[] actions)
    {
        if (actions.IsEmpty())
            return;
        foreach (var action in actions)
            _callback -= action;
    }

    public void Unregister(params UnityEvent[] events)
    {
        if (events.IsEmpty())
            return;
        foreach (var e in events)
            Events.Remove(e);
    }
    public void Invoke()
    {
        _callback?.Invoke();
        foreach (var e in Events)
            e?.Invoke();
    }
}
