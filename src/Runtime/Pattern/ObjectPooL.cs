using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using static ResourcesEx;

public class Pool
{
    
    public Transform Root { get; protected set; }
    public Transform parent { get; protected set; }
    public Component Source { get; private set; }

    protected HashSet<Component> objectSet = new HashSet<Component>();
    protected Queue<Component> objectQueue = new Queue<Component>();
    public IEnumerable<Component> Items => objectQueue;
    public int Count => objectQueue.Count;
    private Pool(Component source, Transform root, int count) {
        Root = root;
        Source = source;
        parent = new GameObject($"{source.name}(Pool)").transform;
        parent.SetParent(root);
        Prepare(count);
    }

    public static bool TryCreateCreate<T>(T source, Transform root, int count,out Pool result) where T : Component
    {
        result = null;
        if (source == null || root == null || count <= 0)
            return false;
        result = new Pool(source, root, count);
        return true;
    }
    public Component Instantiate(int index)
    {
        var obj = GameObject.Instantiate(Source, parent);
        obj.name = $"{index}.{obj.name}";
        obj.gameObject.SetActive(false);
        objectSet.Add(obj);
        objectQueue.Enqueue(obj);
        return obj;
    }
    public void Prepare(int count)
    {
        if (!Source)
            throw new MissingReferenceException("The source of Pool was missing but you are trying to access it.");
        if (objectQueue.Count >= count)
            return;

        for (int i = objectQueue.Count; i < count; i++)
            Instantiate(i);
    }
    public bool IsTypeMatch<T>() {
        if (typeof(T) == Source.GetType())
            return true;
        Debug.LogWarning($"Spawn type does not match. Pool Type :({Source.GetType()}) request Type : ({typeof(T)})");
        return false;
    }
    public T Spawn<T>(Vector3 position, Vector3 forward = default(Vector3)) where T : Component
    {
        if (!IsTypeMatch<T>())
            return null;
        if (!objectQueue.TryFind(d => !d.gameObject.activeSelf, out Component obj))
        {
            obj = objectQueue.Dequeue();
            Despawn((T)obj);
            objectQueue.Enqueue(obj);
        }
        obj.transform.position = position;
        if (forward != default(Vector3))
            obj.transform.forward = forward;
        obj.gameObject.SetActive(true);
        return (T)obj;
    }
    public void Despawn<T>(T obj) where T : Component
    {
        if (!IsTypeMatch<T>())
            return;
        if (!objectSet.Contains(obj))
        {
            Debug.LogWarning($"{obj} does not belone to this - {parent.name}");
            return;
        }
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(parent, false);
    }
}