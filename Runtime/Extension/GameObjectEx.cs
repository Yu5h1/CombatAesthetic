using UnityEngine;
using UnityEngine.SceneManagement;
using static Yu5h1Lib.GameObjectUtility;
using static Yu5h1Lib.ResourcesUtility;

public static class GameObjectEx
{
    public static T FindOrCreate<T>(out T result, System.Func<T> fnCreate, bool includeInactive = true, System.Action<T> Init = null)
    where T : Component
    {
        if (TryFind(out result, includeInactive))
        {
            Init?.Invoke(result);
            return result;
        }
        result = fnCreate();
        Init?.Invoke(result);
        return result;
    }
    public static T FindOrCreate<T>(out T result, bool includeInactive = true, System.Action<T> Init = null) where T : Component
       => FindOrCreate(out result, Create<T>, includeInactive, Init);
    public static T FindOrInstantiateFromResourecs<T>(string path, out T result, bool includeInactive = true, System.Action<T> OnCreated = null)
    where T : Behaviour
    {
        if (TryFind(out result, includeInactive))
            return result;
        result = InstantiateFromResourecs<T>(path);
        OnCreated?.Invoke(result);
        return result;
    }
    #region IfNUll
    public static T InstantiateFromResourecsIfNull<T>(ref T target, string path) where T : Object
        => target ?? (target = InstantiateFromResourecs<T>(path));
    public static T InstantiateFromResourecsIfNull<T>(ref T target,string path, Transform parent) where T : Object
        => target ?? (target = InstantiateFromResourecs<T>(path, parent));
    public static T FindOrCreateIfNull<T>(ref T result, bool includeInactive = true, System.Action<T> Init = null) where T : Component
        => result ?? (result = FindOrCreate(out result, Create<T>, includeInactive, Init));
    public static T FindOrInstantiateFromResourecsIfNull<T>(string path, ref T result, bool includeInactive = true, System.Action<T> OnCreated = null)
    where T : Behaviour
        => result ?? FindOrInstantiateFromResourecs(path, out result, includeInactive, OnCreated);
    #endregion



    public static bool EqualAnyLayer(this GameObject obj,params string[] layerNames)
    {
        foreach (var item in layerNames)
            if (obj.layer.Equals(LayerMask.NameToLayer(item)))
                return true;
        return false;
    }
    public static bool IsBelongToActiveScene(this GameObject gameObject) 
        => gameObject.scene == SceneManager.GetActiveScene();
}