using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStat : UIBehaviour
{
    public Image background;
    public Image fill;

    private void Reset()
    {
        TryGetImageInChildren(nameof(background),out background);
        TryGetImageInChildren(nameof(fill), out fill);
    }

    private bool TryGetImageInChildren(string name, out Image image) => rectTransform.TryGetGraphInChildren(name, out image);

    private Image FindOrCreateImage(string name, Image.Type imgType = Image.Type.Simple, Image.FillMethod fillMethod = Image.FillMethod.Horizontal,
    AttributeType attributeType = AttributeType.None)
    {
        if (TryGetImageInChildren(name, out Image result))
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
        result.type = imgType;
        result.fillMethod = fillMethod;
        result.sprite = Resources.Load<Sprite>("Texture/Square");
        return result;
    }
    public void UpdateStat(AttributeStat status) => fill.fillAmount = status.normal;
}
