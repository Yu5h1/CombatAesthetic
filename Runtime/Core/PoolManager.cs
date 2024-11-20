using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : SingletonComponent<PoolManager>
{
    public static int prepareCount = 5;
    private static Canvas _canvas;
    public static Canvas canvas { 
        get{
            if (_canvas == null || !_canvas.gameObject.IsBelongToActiveScene())
            {
                _canvas = GameObjectEx.InstantiateFromResourecs<Canvas>($"UI/PoolManager_Canvas(Camera)");
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = CameraController.instance.camera;
                _canvas.sortingLayerID = SortingLayer.NameToID("Back");
                _canvas.planeDistance = 1;
            }
            return _canvas;
        }
    } 

    private Dictionary<string, Pool> _pools;
    public Dictionary<string, Pool> pools 
    {
        get 
        {
            if (_pools == null)
            {
                _pools = new Dictionary<string, Pool>();
            }
            return _pools;
        }
    }

    [Tooltip("Maximum number of instantiations")]
    public int Max = 3;

    protected override void Init(){}
    public void PrepareFromResourece<T>(string folderName) where T : Component
    {
        foreach (var item in Resources.LoadAll<T>(folderName))
            Add(item, Max);
    }

    public bool Exists(string key) => pools.ContainsKey(key);

    public T Spawn<T>(string key, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion)) where T : Component
        => Exists(key) ? pools[key].Spawn<T>(position, rotation) : null;

    public T Spawn<T>(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion),Action<T> init = null) where T : Component
    {
        if (!Exists(typeof(T).Name))
            Add(prepareCount,init);
        return Spawn<T>(typeof(T).Name,position, rotation);
    }

    public string GetPoolKey<T>(T obj) where T : Component => obj.GetNameWithOutClone().Substring(obj.name.IndexOf('.') + 1);

    public void Despawn<T>(T obj) where T : Component
    {
        var key = GetPoolKey(obj);
        if ($"{key} does not exists for Despawn".printWarningIf(!Exists(key)))
            return;
        pools[key].Despawn(obj);
    }

    public void Add<T>(T source,int count) where T : Component
    {
        var root = source.GetComponent<RectTransform>() == null ? transform : canvas.transform;
        if (!pools.ContainsKey(source.name) && Pool.TryCreate(source, root, count,out Pool result))
            pools.Add(source.name, result);
    }
    public void Add<T>(int count,Action<T> init = null) where T : Component
    {
        var typeName = typeof(T).Name;
        if ($"{typeName} already exists.".printWarningIf(Exists(typeName)))
            return;
        var source = new GameObject(typeName).AddComponent<T>();
        init?.Invoke(source);
        Add(source, count);
    }

    

    public static IEnumerator ChangeSpriteColor0_2s(GameObject target)
    {
        if (!target.TryGetComponent(out SpriteRenderer sprite))
            yield return null;
        else
        {
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            sprite.color = Color.white;
        }
    }

 
}
