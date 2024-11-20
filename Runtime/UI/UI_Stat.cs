using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Stat : UI_Behaviour
{
    public abstract void UpdateStat(AttributeStat status);

    protected bool TryGetImageInChildren(string name, out Image image) 
        => rectTransform.TryGetGraphInChildren(name, out image);
}
