using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonComponent<T> : MonoBehaviour where T : Component
{
	protected static T _instance;
    public static T instance { 
        get {
            if (_instance != null)
                return _instance;
            if (GameObjectEx.TryFind(out _instance, true))
                Init(_instance);
            else if (GameObjectEx.TryInstantiateFromResourecss(out _instance,typeof(T).Name,null,false))
                Init(_instance);
            else
                _instance = GameObjectEx.Create<T>();
            Init(_instance);
            return _instance; 
        }
    }
    protected virtual void Init() { }
    private static void Init(T component)
    {
        if (component is SingletonComponent<T> s)
            s.Init();
    }
    public static void RemoveInstanceCache() {
        _instance = null;
    }
}
