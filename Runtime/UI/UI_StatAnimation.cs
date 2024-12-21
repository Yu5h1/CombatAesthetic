using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;

public class UI_StatAnimation : UI_Stat
{
    [SerializeField]
    private Image image;
    public bool reverse;

    [SerializeField]
    private Sprite[] _Sprites;
    public Sprite[] sprites => _Sprites;
    public int LastIndex => sprites.Length - 1;
    private void Reset()
    {
        TryGetComponent(out image);
    }
    public override void Refresh(AttributeStat status)
    {
        if (!image || sprites.IsEmpty())
            return;
        if (status.normal <= 0)
        {
            if (image.sprite != null)
                image.sprite = null;
            image.SetAlpha(0);
            return;
        }
        image.SetAlpha(1);
        int index = Mathf.Clamp(Mathf.CeilToInt(status.normal * sprites.Length), 0, LastIndex);
        if (reverse)
            index = LastIndex - index;
        var sprite = sprites[index];
        if (image.sprite != sprite)
            image.sprite = sprite;
    }
}
