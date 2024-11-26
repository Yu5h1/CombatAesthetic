using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public abstract class UI_Behaviour : MonoBehaviour
{
    private RectTransform _rectTransform;
    public RectTransform rectTransform => GetComponent<RectTransform>();

    public UnityEvent<bool> overrideEngage;
    public UnityEvent<bool> overrideDismiss;

    public virtual void Engage()
    {
        if (!overrideEngage.IsEmpty())
            overrideEngage.Invoke(true);
        else if (!gameObject.activeSelf)
            gameObject.SetActive(true);

    }
    public virtual void Dismiss()
    {
        if (!overrideDismiss.IsEmpty())
            overrideDismiss.Invoke(false);
        else if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
