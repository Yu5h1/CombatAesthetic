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

    public Dictionary<string, Pool<Transform>> pools { get; private set; }
    public int Count = 3;

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
            PrepareFromResourece("Fx");
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
    public Transform Spawn(string key, Vector3 position, Vector3 forward = default(Vector3))
        => Exists(key) ? pools[key].Spawn(position, forward) : null;


    public void Despawn(GameObject obj)
    {
        var key = obj.GetNameWithOutClone().Substring(obj.name.IndexOf('.') + 1);
        if (!Exists(key))
            return;
        pools[key].Despawn(obj.transform);
    }
    public void PrepareFromResourece(string folderName)
    {
        pools = pools ?? new Dictionary<string, Pool<Transform>>();
        foreach (var item in Resources.LoadAll<GameObject>(folderName))
            Add(item, Count);
    }
    public void Add(GameObject source,int count)
    {
        if (!pools.ContainsKey(source.name))
            pools.Add(source.name, new Pool<Transform>(transform, source.transform, count));
    }
    public void Add_UI(GameObject source, int count)
    {
        if (!pools.ContainsKey(source.name))
            pools.Add(source.name, new Pool<Transform>(canvas.transform, source.transform, count));
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
