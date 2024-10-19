using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public abstract class SingletonComponent<T> : MonoBehaviourEnhance where T : Component
{
	protected static T _instance;
    public static T instance { 
        get {
            if (_instance != null)
                return _instance;
            if (!GameObjectEx.TryFind(out _instance, true))
            {
                if (GameManager.IsQuit)
                {
                    Debug.LogWarning($"Game has exited\r\nSingleton<{typeof(T).Name}> stops creating instance.");
                    return null;
                }
                if (!GameObjectEx.TryInstantiateFromResourecss(out _instance, typeof(T).Name, null, false))
                    _instance = GameObjectEx.Create<T>();
            }

            Init(_instance);
            return _instance; 
        }
    }
    public static bool DoesNotExists => _instance;

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
