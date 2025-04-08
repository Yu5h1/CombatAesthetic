using UnityEngine;
using UnityEngine.Pool;

public static class MeshPool
{
    private static bool collectionChecks = true;
    public static int defaultCapacity = 3;
    private static int maxPoolSize = 6;

    private static ObjectPool<Mesh> _container;
    //private static readonly object lockObject = new object();

    public static ObjectPool<Mesh> container
    {
        get
        {
            if (_container == null)
            {
                //lock (lockObject)
                //{
                if (_container == null)
                {
                    _container = new ObjectPool<Mesh>(
                        CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject,
                        collectionChecks, defaultCapacity, maxPoolSize
                    );
                }
                //}
            }
            return _container;
        }
    }

    private static Mesh CreatePooledItem() => new Mesh() { name = $"{container.CountAll}.{nameof(Mesh)}" };
 
    private static void OnDestroyPoolObject(Mesh mesh) => Object.Destroy(mesh);
    private static void OnReturnedToPool(Mesh mesh){}
    private static void OnTakeFromPool(Mesh mesh) {}

    public static Mesh Get() => container.Get();
    public static void Release(Mesh mesh) //=> container.Release(mesh);
    {
        container.Release(mesh);
    }
    public static void Dispose()
    {
        if (_container == null)
            return;
        _container.Dispose();
        _container = null;
    }
}
