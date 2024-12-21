using UnityEngine;
using Yu5h1Lib;

[RequireComponent(typeof(RectTransform))]
public abstract class UIControl : MonoBehaviour
{
    protected UI_Manager manager => UI_Manager.instance;
    [SerializeField,ReadOnly]
    private RectTransform _rectTransform;
    public RectTransform rectTransform => _rectTransform ?? (_rectTransform = GetComponent<RectTransform>());


}
