using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_statbar : UIBehaviour
{
    public Image background;
    public Image fill;

    private void Reset()
    {
        FindOrCreateImage(nameof(background),out background);
        if (!FindOrCreateImage(nameof(fill), out fill)) {
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;

            Color.RGBToHSV(fill.color, out float h, out float s, out float v);
            background.color = Color.HSVToRGB(h, s, v * 0.3f);
        }
        rectTransform.SetSize(height: rectTransform.sizeDelta.x * 0.1f);
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

    public void UpdateStat(AttributeStat status) => fill.fillAmount = status.normal;


}
