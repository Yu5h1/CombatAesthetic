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
    //public AnimationCurve curve;

    [SerializeField]
    private UnityEvent completed;
    //[SerializeField]
    //private UnityEvent stopped;

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
    public void StopPerformance() => StopPerformance(0);
    public void StopPerformance(float delay)
        => controller.StopPerformance(new AnimatedInfo() { delay = delay, duration = 0.5f, keepTracking = false }, null);

    private void OnCompleted() => completed?.Invoke();
    //private void OnStopped() => stopped?.Invoke();

}
