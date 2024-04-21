using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Yu5h1Lib;
using UnityEngine.Events;
using System;

public abstract class SingletonComponent<T> : MonoBehaviour where T : Component
{
	private static T _instance;
	public static T instance => GameObjectEx.FindOrCreateIfNull(ref _instance, Init:Init);
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
