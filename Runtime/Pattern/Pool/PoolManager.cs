using Type = System.Type;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;
using Source = UnityEngine.Component;
using System.IO;
using System;

[DisallowMultipleComponent]
public class PoolManager : SingletonBehaviour<PoolManager>
{
    [System.Serializable]
    public class Item
    {
        public Component source;
        public ComponentPool.Config config;        
    }
    public List<Item> items;

    protected override void OnInstantiated() { }
    protected override void OnInitializing() { }

    private void Start()
    {
        if (!isActiveAndEnabled)
            return;
        if (items.IsEmpty())
            return;
        foreach (var item in items)
            Add(item.source, item.config);
    }

    private void OnDestroy()
    {
        TypeMaps.Clear();
        NameMaps.Clear();
        element_source_Maps.Clear();
    }
    #region Static
    private static Canvas _canvas;
    public static Canvas canvas
    {
        get
        {
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

    private Dictionary<Source, ComponentPool> _pools;
    public static Dictionary<Source, ComponentPool> pools
    {
        get
        {
            if (instance._pools == null)
                instance._pools = new Dictionary<Source, ComponentPool>();
            return instance._pools;
        }
    }

    public static Dictionary<Type, Source> TypeMaps { get; private set; } = new Dictionary<Type, Source>();
    public static Dictionary<string, Source> NameMaps { get; private set; } = new Dictionary<string, Source>();
    public static Dictionary<Component, Source> element_source_Maps { get; private set; } = new Dictionary<Source, Source>();

    public void PrepareFromResourece(string folderName)
    {
        foreach (var item in Resources.LoadAll<Transform>(folderName))
            Add(item, ComponentPool.Config.Default);
    }
    public bool TryPrepareFromResourece(string folderName,string item,Action<ComponentPool> onCreated)
        => Add(Resources.Load<Transform>(PathInfo.Combine(folderName, item)), ComponentPool.Config.Default, onCreated) != null;

    public static bool ExistsKey(Source source) => pools.ContainsKey(source);
    public static bool Exists(Type type) => TypeMaps.ContainsKey(type);
    public static bool Exists<T>() => Exists(typeof(T));
    public static bool Exists(string name) => NameMaps.ContainsKey(name);
    public static bool ExistsSource(Component element) => element_source_Maps.ContainsKey(element);

    public static T Spawn<T>(Source key, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null) where T : Component
    {
        if (!ExistsKey(key))
            throw new System.InvalidOperationException($"No Pool with source:{key} can be found.");
        return pools[key].Spawn<T>(position, rotation, parent);
    }

    public static T Spawn<T>(string name, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null) where T : Component
    {
        if (!Exists(name))
            throw new System.InvalidOperationException($"Pool({name}) does not exist.");
        return Spawn<T>(NameMaps[name], position, rotation, parent);
    }
    public static T Spawn<T>(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null) where T : Component
    {
        if (!Exists(typeof(T)))
            throw new System.InvalidOperationException($"TypePool({typeof(T).Name}) does not exist.");
        return Spawn<T>(TypeMaps[typeof(T)], position, rotation, parent);
    }
    public static void Despawn(Component element,Source source)
    {
        if (source == null)
            throw new System.NullReferenceException("PoolManager Despawn Failed ! source was undefined. ");
        if ($"The Pool({source}) was undefined.Despawn failed".printWarningIf(!ExistsKey(source)))
            return;
        pools[source].Despawn(element);
    }
    public static void Despawn(Component element)
    {
        if (element == null)
            throw new System.NullReferenceException("PoolManager Despawn Failed ! ");
        if ($"No Pool found for \"{element}\".Despawn Failed !".printErrorIf(!ExistsSource(element)))
            return;
        Despawn(element,element_source_Maps[element]);
    }
    public static ComponentPool Add<T>(T source, ComponentPool.Config config,Action<ComponentPool> onCreated = null) where T : Component
    {
        if ($"Creating Zero Capacity Pool({source})".printWarningIf(config.Capacity == 0))
            return null;
        if ("It doesn't make sense to create a pool for null source".printWarningIf(source == null))
            return null;
        var root = source.GetComponent<RectTransform>() == null ? instance.transform : canvas.transform;
        if (!pools.ContainsKey(source) && ComponentPool.TryCreate(source, root, config, out ComponentPool pool))
        {
            pools.Add(source, pool);
            NameMaps.Add(source.name, source);
            onCreated?.Invoke(pool);
        }
        return pools[source];
    }
    public static ComponentPool Add<T>(ComponentPool.Config config) where T : Component
    {
        var typeName = typeof(T).Name;
        if ($"{typeName} already exists.".printWarningIf(Exists(typeof(T))))
            return pools[TypeMaps[typeof(T)]];
        var source = GameObjectUtility.Create<T>();
        var pool = Add(source, config);
        TypeMaps.Add(typeof(T), source);
        return pool;
    } 

    public static ComponentPool Add<T>() where T : Component => Add<T>(ComponentPool.Config.Default);
    #endregion

}
