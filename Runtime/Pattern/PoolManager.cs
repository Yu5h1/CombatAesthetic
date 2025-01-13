using Type = System.Type;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;
using SourceKey = UnityEngine.Component;
using ElementKey = UnityEngine.Component;

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

    private Dictionary<SourceKey, ComponentPool> _pools;
    public static Dictionary<SourceKey, ComponentPool> pools
    {
        get 
        {
            if (instance._pools == null)
                instance._pools = new Dictionary<SourceKey, ComponentPool>();
            return instance._pools;
        }
    }

    private Dictionary<Type, SourceKey> _TypeMaps;
    public static Dictionary<Type, SourceKey> TypeMaps
    {
        get
        {
            if (instance._TypeMaps == null)
                instance._TypeMaps = new Dictionary<Type, SourceKey>();
            return instance._TypeMaps;
        }
    }

    private Dictionary<string, SourceKey> _NameMaps;
    public static Dictionary<string, SourceKey> nameMaps
    {
        get
        {
            if (instance._NameMaps == null)
                instance._NameMaps = new Dictionary<string, SourceKey>();
            return instance._NameMaps;
        }
    }

    private static Dictionary<Component, ComponentPool.Map> elementMaps => ComponentPool.Map.elementMaps;
    protected override void Init(){}
    public void PrepareFromResourece(string folderName) 
    {
        foreach (var item in Resources.LoadAll<Transform>(folderName))
            Add(item,ComponentPool.Config.Default,null);
    }
    public static bool ExistsKey(SourceKey source) => pools.ContainsKey(source);
    public static bool Exists(Type type) => TypeMaps.ContainsKey(type);
    public static bool Exists<T>() => Exists(typeof(T));
    public static bool Exists(string name) => nameMaps.ContainsKey(name);
    public static bool ExistsElement(Component element) => elementMaps.ContainsKey(element);

    public static T Spawn<T>(SourceKey key, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion),Transform parent = null) where T : Component
    {
        if (!ExistsKey(key))
            throw new System.InvalidOperationException($"No Pool with source:{key} can be found.");
        return pools[key].Spawn<T>(position, rotation, parent);
    }

    public static T Spawn<T>(string name, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null) where T : Component
    {
        if (!Exists(name))
            throw new System.InvalidOperationException($"Pool({name}) does not exist.");
        return Spawn<T>(nameMaps[name],position, rotation, parent);
    }
    public static T Spawn<T>(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion),Transform parent = null) where T : Component
    {
        if (!Exists(typeof(T)))
            throw new System.InvalidOperationException($"TypePool({typeof(T).Name}) does not exist.");
        return Spawn<T>(TypeMaps[typeof(T)],position, rotation, parent);
    }
    public static void Despawn(ComponentPool.Map map) 
    {
        if (map == null)
            throw new System.NullReferenceException("PoolManager Despawn Failed ! ");
        if ($"The Pool({map.Source}) does not exist.Canceling despawn action".printWarningIf(!ExistsKey(map.Source)))
            return;
        pools[map.Source].Despawn(map.Element);
    }
    public static void Despawn(Component element)
    {
        if (element == null)
            throw new System.NullReferenceException("PoolManager Despawn Failed ! ");
        if ($"No Pool found for \"{element}\".Canceling despawn action".printWarningIf(!ExistsElement(element)))
            return;
        Despawn(elementMaps[element]);
    }
    public static ComponentPool Add<T>(T source,ComponentPool.Config config, System.Action<Component> init) where T : Component
    {
        if (source == null)
            throw new System.InvalidOperationException("It doesn't make sense to create a pool for null source");
        $"Creating Zero Capacity Pool({source})".printWarningIf(config.Capacity == 0);
        var root = source.GetComponent<RectTransform>() == null ? instance.transform : canvas.transform;
        if (!pools.ContainsKey(source) && ComponentPool.TryCreate(source, root, config, out ComponentPool result, init))
        {
            pools.Add(source, result);
            nameMaps.Add(source.name, source);
        }
        return pools[source];
    }
    public static ComponentPool Add<T>(ComponentPool.Config config,System.Action<Component> init) where T : Component
    {
        var typeName = typeof(T).Name;
        if ($"{typeName} already exists.".printWarningIf(Exists(typeof(T))))
            return pools[TypeMaps[typeof(T)]];
        var source = GameObjectUtility.Create<T>();
        var pool = Add(source, config, init);
        TypeMaps.Add(typeof(T), source);
        return pool;
    }
    public ComponentPool Add<T>(System.Action<Component> init) where T : Component => Add<T>(ComponentPool.Config.Default,init);

    private void OnDestroy()
    {
        elementMaps.Clear();
    }
}
