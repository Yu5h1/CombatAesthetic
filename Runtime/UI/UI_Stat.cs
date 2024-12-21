using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Stat : UIControl
{
    public abstract void Refresh(AttributeStat status);

    protected bool TryGetImageInChildren(string name, out Image image) 
        => rectTransform.TryGetGraphInChildren(name, out image);
}
