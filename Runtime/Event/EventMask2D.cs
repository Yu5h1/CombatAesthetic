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
        => enabled && tagOption.Compare(other.transform.tag) && layers.Contains(other);
}
[System.Serializable]
public class TagOption
{
    public enum ComparisionType
    {
        Equal = 0,
        NotEqual = 1
    }
    [DropDownTag(true)]
    public string tag;

    private string[] _tags;
    public string[] tags 
    { 
        get
        { 
            if (_tags == null)
                _tags = tag.Split(',');
            return _tags;
        }
    }

    public ComparisionType type = ComparisionType.NotEqual;
    public override string ToString() => tag;
    public bool IsUntagged => tag.IsEmpty() || tag.Equals("Untagged");
    public bool Compare(string otherTag) 
    {
        if (IsUntagged)
            return true;
        return type == ComparisionType.Equal ?
            tag.Contains(',') ? otherTag.EqualsAny(tags) : tag == otherTag :
            tag.Contains(',') ? !otherTag.EqualsAny(tags) : tag != otherTag;
    }
}