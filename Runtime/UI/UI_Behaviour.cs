using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public abstract class UI_Behaviour : MonoBehaviour
{
    private RectTransform _rectTransform;
    public RectTransform rectTransform => GetComponent<RectTransform>();
}
