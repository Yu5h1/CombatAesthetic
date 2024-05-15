using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public abstract class EventMask2D : MonoBehaviourEnhance
{
    public LayerMask layers;
    public TagOption tagOption;
    private void OnEnable() { }
    protected bool Validate(GameObject other) 
        => enabled && tagOption.Compare(other.tag) && layers.Contains(other);
    public void PlayAudio()
    {
        if (TryGetComponent(out AudioSource audioSource))
            GameManager.instance.PlayAudio(audioSource);
    }
    public void Spawn(string name)
    {
        PoolManager.instance.Spawn<Transform>(name, transform.position, transform.rotation);
    }
    public void Prompt(string line)
    {
        GameManager.ui_Manager.Dialog_UI.lines = new string[] { line };
        GameManager.ui_Manager.Dialog_UI.gameObject.SetActive(true);
    }
    public void Prompt(string[] lines)
    {
        GameManager.ui_Manager.Dialog_UI.lines = lines;
        GameManager.ui_Manager.Dialog_UI.gameObject.SetActive(true);
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