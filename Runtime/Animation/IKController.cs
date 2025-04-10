using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    public Transform root;
    public Transform target;
    public float distance;
    public Vector2 offset;
    private void Start() {}
    // Update is called once per frame
    void Update()
    {
        if (!root || !target)
            return;
        target.position = root.position + (Vector3)offset + Vector3.up * distance;
    }
}
