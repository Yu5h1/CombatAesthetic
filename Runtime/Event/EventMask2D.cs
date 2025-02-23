using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public abstract class EventMask2D : BaseEvent2D
{
    public Transform owner { get; set; }
    public LayerMask layers;
    public TagOption tagOption;
    protected bool Validate(GameObject other) 
        => enabled && other.transform != owner && tagOption.Compare(other) && layers.Contains(other);

    protected bool NotAllowTriggerExit => GameManager.IsQuit || SceneController.IsSceneTransitioning;
            
    protected virtual void OnDisable() => owner = null;
}
[System.Serializable]
public class TagOption
{
    [Tooltip("Compare root transform.")]
    public bool root = true;
    public enum ComparisionType
    {
        Equal = 0,
        NotEqual = 1
    }
    [DropDownTag(true)]
    public string tag = "Untagged";

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
    public bool Compare(GameObject obj)
    {
        var otherTag = root ? obj.transform.root.tag : obj.tag;
        if (IsUntagged)
            return true;
        return type == ComparisionType.Equal ?
            tag.Contains(',') ? otherTag.EqualsAny(tags) : tag == otherTag :
            tag.Contains(',') ? !otherTag.EqualsAny(tags) : tag != otherTag;
    }
}