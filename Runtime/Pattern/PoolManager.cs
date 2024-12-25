using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;

[DisallowMultipleComponent]
public class PoolManager : SingletonBehaviour<PoolManager>
{
    private static Canvas _canvas;
    public static Canvas canvas { 
        get{
            if (_canvas == null || !_canvas.gameObject.IsBelongToActiveScene())
            {
                _canvas = ResourcesUtility.InstantiateFromResourecs<Canvas>($"UI/PoolManager_Canvas(Camera)");
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

#if UNITY_EDITOR
    [SerializeField]
    private List<Pool> poolItems = new List<Pool>();
#endif

    protected override void Init(){}
    public void PrepareFromResourece<T>(string folderName) where T : Component
    {
        foreach (var item in Resources.LoadAll<T>(folderName))
            Add(item,Pool.Config.Default);
    }

    public bool Exists(string key) => pools.ContainsKey(key);



    public T Spawn<T>(string key, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion),Transform parent = null) where T : Component
        => Exists(key) ? pools[key].Spawn<T>(position, rotation, parent) : null;

    public T Spawn<T>(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion),Transform parent = null) where T : Component
    {
        if (!Exists(typeof(T).Name))
            throw new System.InvalidOperationException($"{typeof(T).Name} does not exist.");
        return Spawn<T>(typeof(T).Name,position, rotation, parent);
    }

    public string GetPoolKey<T>(T obj) where T : Component => obj.GetNameWithOutClone().Substring(obj.name.IndexOf('.') + 1);

    public void Despawn<T>(T obj) where T : Component
    {
        var key = GetPoolKey(obj);
        if ($"{key} does not exists for Despawn".printWarningIf(!Exists(key)))
            return;
        pools[key].Despawn(obj);
    }

    public Pool Add<T>(T source,Pool.Config config) where T : Component
    {
        $"Creating Zero Capacity Pool({source})".printWarningIf(config.Capacity == 0);
        var root = source.GetComponent<RectTransform>() == null ? transform : canvas.transform;
        if (!pools.ContainsKey(source.name) && Pool.TryCreate(source, root, config, out Pool result))
            pools.Add(source.name, result);
#if UNITY_EDITOR
        poolItems = pools.Values.ToList();
#endif
        return pools[source.name];
    }
    public Pool Add<T>(Pool.Config config,System.Action<T> init) where T : Component
    {
        var typeName = typeof(T).Name;
        if ($"{typeName} already exists.".printWarningIf(Exists(typeName)))
            return pools[typeName];
        var source = GameObjectUtility.Create<T>();
        var pool = Add(source, config);
        return pool;
    }
    public Pool Add<T>(System.Action<T> init) where T : Component => Add<T>(Pool.Config.Default,init);

}
