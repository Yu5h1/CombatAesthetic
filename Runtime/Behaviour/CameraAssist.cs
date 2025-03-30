using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using AnimatedInfo = CameraController.AnimatedInfo;
public class CameraAssist : MonoBehaviour
{
    CameraController controller => CameraController.instance;
    private Vector3 GetPosition() => controller.transform.position;
    public float duration = 1f;
    public Vector3 offset;
    public Transform[] targets;
    public bool keepTracking = false;
    //public bool allowStop = true;
    //public AnimationCurve curve;

    [SerializeField]
    private UnityEvent completed;

    private Vector3[] positions;
    private Vector3 GetCenter()
    {
        if (positions.IsEmpty())
            positions = new Vector3[targets.Length];
        else if (targets.Length != positions.Length)
            Array.Resize(ref positions, targets.Length);
        for (int i = 0; i < targets.Length; i++)
            positions[i] = targets[i].position + offset;
        return controller.GetCenter(positions);
    }

    private void Start() {}

    public void Focus()
        => Focus(0);
    public void Focus(float delay)
        => controller.Focus(new AnimatedInfo(){ delay = delay,duration = duration,keepTracking = keepTracking }, GetCenter, OnCompleted);

    public void Focus(bool allowStop)
    {
        controller.allowStopPerformance = allowStop;
        Focus();
    }

    public void StopPerformance() => StopPerformance(0);
    public void StopPerformance(float delay)
        => CameraControllerAgent.CallStopPerformance(delay, 0.5f, false);

    public void StopPerformance(bool force)
    {
        if (force)
            controller.allowStopPerformance = true; ;
        StopPerformance(0);
    }



    private void OnCompleted() => completed?.Invoke();

    public void AddTarget(Collider2D other)
       => controller.AddTarget(other.transform);
    public void RemoveTarget(Collider2D other)
        => controller.RemoveTarget(other.transform);

    public void EnableStopPerformance()
        => controller.allowStopPerformance = true;
    public void DisableStopPerformance()
        => controller.allowStopPerformance = false;
}
