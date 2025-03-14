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
                lineControllers[i].connected += OnAllConnected;
                lineControllers[i].disconnected += OnAllDisconnected;
        }
    }

    private bool IsConnecting(LineRendererController controller) => controller.IsConnecting;

    private void OnAllConnected()
    {
        if (lineControllers.All(l => l.IsConnecting))
            AllConnected?.Invoke();
    }
    private void OnAllDisconnected()
    {
        if (lineControllers.All(l => !l.IsConnecting))
            AllDisconnected?.Invoke(); 
    }
    public void ConnectAll()
    {
        foreach (var line in lineControllers)
            line.IsConnecting = true;
    }
    public bool IsNotPerforming()
        => lineControllers.All(l => !l.IsPerforming);
}
