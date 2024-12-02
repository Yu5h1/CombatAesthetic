using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public abstract class UI_Behaviour : MonoBehaviour
{
    protected UI_Manager manager => UI_Manager.instance;
    private RectTransform _rectTransform;
    public RectTransform rectTransform => GetComponent<RectTransform>();
}
