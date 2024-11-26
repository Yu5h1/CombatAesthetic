using UnityEngine;

public static class ResourcesEx
{
    public static bool TryLoad<T>(string path, out T result) where T : Object
    {
        result = Resources.Load<T>(path);
        return result;
    }
}