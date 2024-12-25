using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class KeyCodeEventManager : MonoBehaviour
{
    public HashSet<KeyCodeEventHandler> handlers = new HashSet<KeyCodeEventHandler>();

    private void Update()
    {
        foreach (var handler in handlers)
            handler.Handle();
    }
}
