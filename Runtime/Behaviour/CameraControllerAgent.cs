using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class CameraControllerAgent : MonoBehaviour 
{
    public static CameraController controller => CameraController.instance;

    //[SerializeField]
    //private UnityEvent _performanceBegin;
    //public event UnityAction performanceBegin
    //{
    //    add => _performanceBegin.AddListener(value);
    //    remove => _performanceBegin.RemoveListener(value);
    //}

    [SerializeField]
    private UnityEvent _performanceEnd;
    public event UnityAction performanceEnd
    {
        add => _performanceEnd.AddListener(value);   
        remove => _performanceEnd.RemoveListener(value);
    }
    // For UnityEvent
    public void StopPerformance(float duration) => CallStopPerformance(0,duration,false);

    public static void CallStopPerformance(float delay,float duration,bool keepTracking)
    {
        controller.StopPerformance(new CameraController.AnimatedInfo()
        {
            delay = delay,
            duration = duration,
            keepTracking = keepTracking,
        }, InvokeAllPerformanceEndEvents);
    }
    private static void InvokeAllPerformanceEndEvents()
    {
        foreach (var agent in GameObject.FindObjectsOfType<CameraControllerAgent>(true))
            agent._performanceEnd?.Invoke();
    }

}
