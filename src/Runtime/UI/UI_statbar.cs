using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;

public class UI_Statbar : UI_Behaviour
{
    [SerializeField]
    private Image _background;
    public Image background => _background;

    [SerializeField]
    private Image _fill;
    public Image fill => _fill;

    [SerializeField]
    private Image _reduce;
    public Image reduce => _reduce;

    private void Reset()
    {
        FindOrCreateImage(nameof(background),out _background);
        if (!FindOrCreateImage(nameof(fill), out _fill)) {
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;

            Color.RGBToHSV(_fill.color, out float h, out float s, out float v);
            _background.color = Color.HSVToRGB(h, s, v * 0.3f);
        }
        rectTransform.SetSize(height: rectTransform.sizeDelta.x * 0.1f);
    }
    public void Method()
    {

    }
    private bool TryGetImageInChildren(string name, out Image image) => rectTransform.TryGetGraphInChildren(name, out image);
    private bool FindOrCreateImage(string name, out Image result)
    {
        if (TryGetImageInChildren(name, out result))
            return result;
        if (!transform.TryFind(name,out Transform t))
            t = new GameObject(name).transform;
        result = t.gameObject.AddComponent<Image>();
        result.rectTransform.SetParent(rectTransform, false);
        result.rectTransform.anchorMin = Vector2.zero;
        result.rectTransform.anchorMax = Vector2.one;
        result.rectTransform.pivot = Vector2.one * 0.5f;
        result.rectTransform.offsetMin = Vector2.zero;
        result.rectTransform.offsetMax = Vector2.zero;
        result.sprite = Resources.Load<Sprite>("Texture/Square");
        return false;
    }
    public void UpdateStat(AttributeStat status) => _fill.fillAmount = status.normal;

    public static UI_Statbar Create(Transform parent,string name, int index,
            Vector2 size, bool UpDown) {
        var result = new GameObject(name).AddComponent<UI_Statbar>();
        result.Reset();
        return result;
    }


}
