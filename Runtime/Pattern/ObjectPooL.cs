using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using Yu5h1Lib;
using static ResourcesEx;

public class Pool
{
    public string Name;
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
        Name = source.name;
        parent = new GameObject($"{Name}(Pool)").transform;
        parent.SetParent(root);

        if (source.gameObject.IsBelongToActiveScene())
        {
            source.gameObject.SetActive(false);
            source.gameObject.transform.SetParent(parent);
            //Join(source);
        }

        Prepare(count);
    }

    public static bool TryCreate<T>(T source, Transform root, int count,out Pool result) where T : Component
    {
        result = null;
        if (source == null || root == null || count <= 0)
            return false;
        result = new Pool(source, root, count);
        return true;
    }
    public void Prepare(int count)
    {
        if (!Source)
            throw new MissingReferenceException("The source of Pool was missing but you are trying to access it.");
        if (objectQueue.Count >= count)
            return;

        for (int i = objectQueue.Count; i < count; i++)
            Join(GameObject.Instantiate(Source, parent));
    }
    private bool Join(Component obj)
    {
        if ($"{obj.name} already Exists !".printWarningIf(objectSet.Contains(obj)))
            return false;
        obj.name = $"{objectSet.Count}.{Name}";
        obj.gameObject.SetActive(false);
        objectSet.Add(obj);
        objectQueue.Enqueue(obj);
        return true;
    }

    public bool IsTypeMatch<T>()
        => !$"Spawn type does not match. Pool Type :({Source.GetType()}) request Type : ({typeof(T)})".
            printWarningIf(typeof(T) != Source.GetType());

    public T Spawn<T>(Vector3 position, Quaternion rotation = default(Quaternion)) where T : Component
    {
        if (!IsTypeMatch<T>())
            return null;
        if (!objectQueue.TryGet(d => !d.gameObject.activeSelf, out Component obj))
        {
            obj = objectQueue.Dequeue();
            Despawn((T)obj);
            objectQueue.Enqueue(obj);
        }
        obj.transform.position = position;
        if (rotation != default(Quaternion))
            obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        return (T)obj;
    }
    public void Despawn<T>(T obj) where T : Component
    {
        if (!IsTypeMatch<T>())
            return;
        if (!objectSet.Contains(obj))
        {
            Debug.LogWarning($"{obj} does not belone to Pool({parent.name})");
            return;
        }
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(parent, false);
    }
}