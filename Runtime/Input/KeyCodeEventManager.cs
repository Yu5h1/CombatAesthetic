using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class KeyCodeEventManager : SingletonBehaviour<KeyCodeEventManager>
{
    public HashSet<KeyCodeEventHandler> handlers = new HashSet<KeyCodeEventHandler>();

    protected override void Init()
    {
        
    }
    private void Update()
    {
        foreach (var handler in handlers)
            handler.Handle();
    }
}
