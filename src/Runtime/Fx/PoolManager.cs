using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : SingletonComponent<PoolManager>
{
    private StatsManager _statsManager;
    public static StatsManager statsManager => instance._statsManager;
    private Canvas _canvas;
    public static Canvas canvas => instance._canvas;

    public Dictionary<string, Pool> pools { get; private set; }

    [Tooltip("Maximum number of instantiations")]
    public int Max = 3;

    protected override void Init(){

        if (SceneController.IsLevelScene)
        {
            if (_canvas == null)
            {
                _canvas = GameObjectEx.InstantiateFromResourecs<Canvas>($"UI/PoolManager_Canvas");
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
                _canvas.worldCamera = CameraController.instance.camera;
                _canvas.sortingLayerID = SortingLayer.NameToID("Back");
                _canvas.planeDistance = 1;
                _canvas.GetOrAdd(out _statsManager);
            }
            PrepareFromResourece<Transform>("Prefab/Fx");
        }
    }
    public bool Exists(string key) {
        if (!pools.ContainsKey(key))
        {
            Debug.Log($"pool \'{key}\' does not exists");
            return false;
        }
        return true;
    }
    public T Spawn<T>(string key, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion)) where T : Component
        => Exists(key) ? pools[key].Spawn<T>(position, rotation) : null;


    public void Despawn<T>(T obj) where T : Component
    {
        var key = obj.GetNameWithOutClone().Substring(obj.name.IndexOf('.') + 1);
        if (!Exists(key))
            return;
        pools[key].Despawn(obj);
    }
    public void PrepareFromResourece<T>(string folderName) where T : Component
    {
        pools = pools ?? new Dictionary<string, Pool>();
        foreach (var item in Resources.LoadAll<T>(folderName))
            Add(item, Max);
    }
    public void Add<T>(T source,int count) where T : Component
    {
        var root = source.GetComponent<RectTransform>() == null ? transform : canvas.transform;
        if (!pools.ContainsKey(source.name) && Pool.TryCreateCreate(source, root, count,out Pool result))
            pools.Add(source.name, result);
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
