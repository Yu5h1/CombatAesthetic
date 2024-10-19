using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public abstract class EventMask2D : BaseEvent2D
{
    public LayerMask layers;
    public TagOption tagOption;
    private void OnEnable() { }
    protected bool Validate(GameObject other) 
        => enabled && tagOption.Compare(other.tag) && layers.Contains(other);
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