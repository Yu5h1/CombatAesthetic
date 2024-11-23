using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneEx 
{
    public static T[] FindObjectsByType<T>(this Scene scene) where T : Object
    {
        var results = new List<T>();
        foreach (var rootObj in scene.GetRootGameObjects())
            results.AddRange(rootObj.GetComponentsInChildren<T>(true));
        return results.ToArray();
    }

    public static bool IsSceneInBuild(this Scene scene) => scene.buildIndex >= 0;
}
