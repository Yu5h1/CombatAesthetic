using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameObjectEx
{
    public static bool InstantiateWithCloneSuffix = false;
    public static string GetNameWithOutClone<T>(this T obj) where T : Object
    {
        var name = obj.name;
        if (name.EndsWith("(Clone)"))
            name = name.Remove(name.Length - "(Clone)".Length);
        return name;
    }
    public static bool TryInstantiateFromResourecss<T>(out T result,string path, Transform parent = null) where T : Object
    {
        result = null;
        if ($"TryInstantiateFromResourecss {path} does not exist!".printWarningIf(!ResourcesUtility.TryLoad(path, out T source)))
            return false;
        result = parent == null ? GameObject.Instantiate(source) : GameObject.Instantiate(source, parent);
        if (!InstantiateWithCloneSuffix)
            result.name = result.GetNameWithOutClone();
        return true;
    }
    public static T InstantiateFromResourecs<T>(string path, Transform parent = null) where T : Object
        => TryInstantiateFromResourecss(out T result, path, parent) ? result : null;
    public static T Create<T>() where T : Component
        => new GameObject(typeof(T).Name).AddComponent<T>();
    public static T Create<T>(Transform parent) where T : Component
    {
        var result = Create<T>();
        if (parent)
            result.transform.SetParent(parent, false);
        return result;
    }
    public static bool TryFind<T>(out T result, bool includeInactive = true) where T : Component
        => (result = GameObject.FindObjectOfType<T>(includeInactive)) != null;
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

    public static bool CompareLayer(this GameObject obj,LayerMask layerMask) => ((1 << obj.layer) & layerMask.value) != 0;

    public static bool CompareLayer(this GameObject obj, string layerName) 
        => ((1 << obj.layer) & LayerMask.GetMask(layerName)) != 0;

    public static bool EqualAnyLayer(this GameObject obj,params string[] layerNames)
    {
        foreach (var item in layerNames)
            if (obj.layer.Equals(LayerMask.NameToLayer(item)))
                return true;
        return false;
    }
    public static bool IsBelongToActiveScene(this GameObject gameObject) => gameObject.scene == SceneManager.GetActiveScene();
}