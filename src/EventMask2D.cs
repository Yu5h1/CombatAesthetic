using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventMask2D : MonoBehaviourEnhance
{
    public LayerMask layers;
    public string IgnoreTag;

    protected bool Validate(GameObject other) {
        if (!IgnoreTag.IsEmpty() && other.CompareTag(IgnoreTag))
            return false;
        if (!layers.Contains(other))
            return false;
        return true;
    }
}
