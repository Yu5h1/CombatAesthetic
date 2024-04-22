using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ResourcesEx;

public class Pool
{
    public Transform Root { get; protected set; }
    public Transform parent { get; protected set; }
    public Component Source { get; private set; }
    protected List<Component> objectList = new List<Component>();
    public IEnumerable<Component> Items => objectList;
    public int Count => objectList.Count;
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
    public void Prepare(int count)
    {
        if (!Source)
            throw new MissingReferenceException("The source of Pool was missing but you are trying to access it.");
        if (objectList.Count >= count)
            return;

        for (int i = objectList.Count; i < count; i++)
        {
            var obj = GameObject.Instantiate(Source, parent);
            obj.name = $"{i}.{obj.name}";
            obj.gameObject.SetActive(false);
            objectList.Add(obj);
        }
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
        if (!objectList.TryFind(d => !d.gameObject.activeSelf, out Component obj))
        {
            Despawn(objectList.Last());
            obj = objectList.First();
        }
        obj.transform.position = position;
        if (forward != default(Vector3))
            obj.transform.forward = forward;
        obj.gameObject.SetActive(true);
        return (T)obj;
    }
    public void Despawn<T>(T target) where T : Component
    {
        if (!IsTypeMatch<T>())
            return;
        if (!objectList.Contains(target))
        {
            Debug.LogWarning($"{target} does not belone to this - {parent.name}");
            return;
        }
        target.gameObject.SetActive(false);
        target.transform.SetParent(parent, false);
        target.transform.SetSiblingIndex(0);
        objectList.Swap(target, 0);
    }
}