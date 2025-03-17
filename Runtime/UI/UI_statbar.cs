using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;

public class UI_Statbar : UI_Stat
{
    //[SerializeField]
    //private Image _background;
    //public Image background => _background;

    [SerializeField]
    private Image[] _fills;
    public Image[] fills => _fills;


    // 2nd performance bar [WIP]
    //[SerializeField]
    //private Image _reduce;
    //public Image reduce => _reduce;


    protected override void OnInitializing()
    {
        base.OnInitializing();
                //FindOrCreateImage(out _background,nameof(background));

        //if (!FindOrCreateImage(out _fill,nameof(fill))) {
        //    _fill.type = Image.Type.Filled;
        //    _fill.fillMethod = Image.FillMethod.Horizontal;

        //    Color.RGBToHSV(_fill.color, out float h, out float s, out float v);
        //    _background.color = Color.HSVToRGB(h, s, v * 0.3f);
        //}

        //rectTransform.SetSize(height: rectTransform.sizeDelta.x * 0.1f);
    }
    public override void Refresh(AttributeStat status) {

        if (fills.IsEmpty())
            return;
        if (fills.Length == 1)
        {
            fills[0].fillAmount = status.normal;
            return;
        }
        var clampedNormal = Mathf.Clamp01(status.normal);
        int numFull = Mathf.FloorToInt(clampedNormal * fills.Length);
        float remainingFill = clampedNormal * fills.Length - numFull;

        for (int i = 0; i < fills.Length; i++)
        {
            if (i < numFull)
                fills[i].fillAmount = 1f;
            else if (i == numFull)
                fills[i].fillAmount = remainingFill;
            else
                fills[i].fillAmount = 0f;  // 其餘未達進度的項目設為 0
        }
    } 

    #region Obsolet methods
    private bool FindOrCreateImage(out Image result, string name)
    {
        if (TryGetImageInChildren(name, out result))
            return result;
        if (!transform.TryFind(name, out Transform t))
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

    public static UI_Statbar Create(Transform parent, string name, int index,
            Vector2 size, bool UpDown)
    {
        var result = new GameObject(name).AddComponent<UI_Statbar>();
        result.Init();
        return result;
    } 
    #endregion


}
