using System.Collections;
using UnityEngine;
using Yu5h1Lib;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Attribute : UIControl
{
    public UI_Stat[] uI_stats;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        TryGetComponent(out canvasGroup);
    }
    public void Prepare(AttributeBehaviour attribute)
    {
        uI_stats = new UI_Stat[attribute.Keys.Length];

        for (int i = 0; i < attribute.Keys.Length; i++)
            if (transform.TryFind(attribute.Keys[i], out Transform t))
                if (t.TryGetComponent(out UI_Stat ui_Stat))
                {
                    uI_stats[i] = ui_Stat;
                    uI_stats[i].Refresh(attribute.stats[i]);
                }
    }
    public void UpdateAttribute(AttributeBehaviour attribute)
    {
        if (!attribute)
            return;
        for (int i = 0; i < attribute.Keys.Length; i++)
            uI_stats[i]?.Refresh(attribute.stats[i]);
    }
    public void Despawn()
    {
        foreach (var item in uI_stats)
            PoolManager.instance.Despawn(item);
        uI_stats = null;
    }

    public void SetVisible(bool visible,params AttributeType[] attributes)
    {
        if (attributes.IsEmpty())
            return;
        foreach (var att in attributes)
        {
            if (transform.TryFind($"{att}", out Transform t))
                t.gameObject.SetActive(visible);
        }
    }
    public void Show(params AttributeType[] attributes) => SetVisible(true, attributes);
    public void Hide(params AttributeType[] attributes) => SetVisible(false, attributes);
    

    //deprecated implimentation
    //public StatProperty_Deprecated.VisualItem[] CreateVisualItems(RectTransform parent, Vector2 size, bool UpDown)
    //=> Keys.Select((key, order) => new StatProperty_Deprecated.VisualItem(parent, System.Enum.Parse<AttributeType>(key), order, size, UpDown)).ToArray();
    public void Perform(AttributeBehaviour attribute,AffectType affectType,AttributeType flags,float amount)
    {
        if (attribute.Keys.IsEmpty())
            return;
        uI_stats = new UI_Statbar[attribute.Keys.Length];
        //for (int i = 0; i < attribute.Keys.Length; i++)
        //{
        //    uI_statbars[i] = StatsManager.instance.SpawnStatbar();
        //    var root = uI_statbars[i].rectTransform;
        //    root.SetParent(transform, false);
        //    var pos = transform.localPosition;
        //    pos.y = i * root.sizeDelta.y;
        //    root.localPosition = pos;
        //}
        StartCoroutine(PerformingStatus());
        Despawn();
    }
    public IEnumerator PerformingStatus()
    {
        yield return new WaitForSeconds(1);
    }
}
