using UnityEngine;
using UnityEngine.UI;

public class UI_StatCounter : UI_Stat
{
    [SerializeField]
    private int _elementCount = 3;
    public int elementCount => _elementCount;

    [SerializeField]
    private Sprite _SpriteSource;
    public Sprite spriteSource => _SpriteSource;

    [SerializeField]
    private Image[] elements;

    [SerializeField]
    

    [ContextMenu("PrepareElements")]
    private void PrepareElements()
    {
        elements = new Image[elementCount];
        for (int i = 0; i < elementCount; i++)
        {
            FindOrCreateElement($"element {i}", out elements[i]);
            var rt = elements[i].transform as RectTransform;
            var pos = rt.position;
            pos.x = spriteSource.rect.width * i;
            rt.position = pos;
        }
    }

    public override void UpdateStat(AttributeStat status)
    {
        throw new System.NotImplementedException();
    }

    private bool FindOrCreateElement(string name, out Image result)
    {
        Transform t = null;
        if (TryGetImageInChildren(name, out result))
        {
            t = result.transform;
        }
        if (!t && !transform.TryFind(name, out t))
            t = new GameObject(name).transform;

        if (!result)
            result = t.gameObject.AddComponent<Image>();

        result.rectTransform.SetParent(rectTransform, false);
        result.sprite = _SpriteSource;

        return result;
    }
}
