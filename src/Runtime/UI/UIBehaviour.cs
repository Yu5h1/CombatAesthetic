using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public abstract class UIBehaviour : MonoBehaviour
{
    private RectTransform _rectTransform;
    public RectTransform rectTransform => GetComponent<RectTransform>();
}
