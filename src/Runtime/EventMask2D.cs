using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EventMask2D : MonoBehaviourEnhance
{
    public LayerMask layers;
    public TagOption tagOption;
    protected bool Validate(GameObject other) {
        if (!tagOption.Compare(other.tag))
            return false;
        if (!layers.Contains(other))
            return false;
        return true;
    }
}
[System.Serializable]
public class TagOption
{
    public enum ComparisionType
    {
        Equal = 0,
        NotEqual = 1
    }
    [DropDownTag]
    public string tag;
    public ComparisionType type = ComparisionType.NotEqual;
    public override string ToString() => tag;
    public bool IsUntagged => tag.IsEmpty() || tag.Equals("Untagged");
    public bool Compare(string otherTag) => IsUntagged ? true : (type == ComparisionType.Equal ? tag == otherTag : otherTag != tag);
}