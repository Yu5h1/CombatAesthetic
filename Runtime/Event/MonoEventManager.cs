
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class MonoEventManager : SingletonBehaviour<MonoEventManager>
{
    protected override void OnInstantiated() {}
    protected override void OnInitializing() {}

    public static Dictionary<Object, UnityAction<Object>> StartEvents;

    // Start is called before the first frame update
    void Start()
    {
        $"MonoEventManager start".print();
        foreach (var item in StartEvents)
        {
            item.Value?.Invoke(item.Key);
        }
    }

    


    //// Update is called once per frame
    //void Update()
    //{
    //    throw new System.NotImplementedException();
    //}
}
