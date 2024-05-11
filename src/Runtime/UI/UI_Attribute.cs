using DG.Tweening;
using System.Collections;

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Attribute : UI_Behaviour
{
    [SerializeField]
    private UI_Statbar[] _uI_statbars;
    public UI_Statbar[] uI_statbars => _uI_statbars;

    private CanvasGroup canvasGroup;
    private void Reset()
    {
        _uI_statbars = GetComponentsInChildren<UI_Statbar>();
    }
    private void Awake()
    {
        TryGetComponent(out canvasGroup);
    }
    public void UpdateAttribute(AttributeBehaviour attribute)
    {
        if (!attribute || uI_statbars.Length != attribute.Keys.Length)
            return;
        for (int i = 0; i < attribute.Keys.Length; i++)
            uI_statbars[i].UpdateStat(attribute.stats[i]);
    }
    public void Despawn()
    {
        foreach (var item in uI_statbars)
            PoolManager.instance.Despawn(item);
        _uI_statbars = null;
    }
    //deprecated implimentation
    //public StatProperty_Deprecated.VisualItem[] CreateVisualItems(RectTransform parent, Vector2 size, bool UpDown)
    //=> Keys.Select((key, order) => new StatProperty_Deprecated.VisualItem(parent, System.Enum.Parse<AttributeType>(key), order, size, UpDown)).ToArray();
    public void Perform(AttributeBehaviour attribute,AffectType affectType,AttributeType flags,float amount)
    {
        if (attribute.Keys.IsEmpty())
            return;
        _uI_statbars = new UI_Statbar[attribute.Keys.Length];
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

    public void FadeIn(float duration = 0.5f)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1,duration);
    }
    public void FadeOut(float duration = 0.5f)
    {
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0, duration);
    }
}
