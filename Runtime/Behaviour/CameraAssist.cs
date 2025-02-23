using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

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
    public void Focus()
        => controller.Focus(GetCenter,  duration, OnCompleted,keepTracking);
    public void StopFocue() => controller.StopFocus(duration, OnCompleted);
 
    private void OnCompleted() => completed?.Invoke();

}
