using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_AttributeStat : MonoBehaviour
{
    public UI_Statbar[] uI_statbars { get; private set; }
    
    public void Init(AttributeBehaviour attributeStat)
    {
        uI_statbars = new UI_Statbar[attributeStat.Keys.Length];
        for (int i = 0; i < attributeStat.Keys.Length; i++)
        {
            uI_statbars[i] = StatsManager.instance.SpawnStatbar();
            var root = uI_statbars[i].rectTransform;
            root.SetParent(transform, false);
            var pos = transform.localPosition;
            pos.y = i * root.sizeDelta.y;
            root.localPosition = pos;
        }
    }
    public void Despawn()
    {
        foreach (var item in uI_statbars)
            PoolManager.instance.Despawn(item);
        uI_statbars = null;
    }
    public void Perform(AttributeBehaviour attribute)
    {
        Init(attribute);
        StartCoroutine(PerformingStatus());
        Despawn();
    }
    public IEnumerator PerformingStatus()
    {
        yield return new WaitForSeconds(1);
    }
}
