using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class LineGroup : MonoBehaviour
{

    [SerializeField]
    private LineRendererController[] _lineControllers;
    public LineRendererController[] lineControllers => _lineControllers;

    public UnityEvent AllConnected;
    public UnityEvent AllDisconnected;
    // Start is called before the first frame update
    void Start()
    {
        if (!lineControllers.IsEmpty())
        for (int i = 0; i < lineControllers.Length; i++)
        {
            lineControllers[i].HitStateChanged += HitStateChanged;
        }
    }

    private bool IsConnecting(LineRendererController controller) => controller.IsConnecting;
    private void HitStateChanged()
    {
        if (lineControllers.All( l => l.IsConnecting))
            AllConnected?.Invoke();
        else if (lineControllers.All(l => !l.IsConnecting))
            AllDisconnected?.Invoke(); 
    }
}
