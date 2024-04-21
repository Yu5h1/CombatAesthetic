using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ResourcesEx;

public class Pool<T> where T : Component
{
    public Transform Root { get; private set; }
    public T Source { get; private set; }
    public Transform parent { get; private set; }
    private List<T> objectList = new List<T>();
    public IEnumerable<T> Items => objectList;
    public int Count => objectList.Count;

    public Pool(Transform root, T source,int count)
    {
        Root = root;
        Source = source;
        parent = new GameObject($"{source.name}(Pool)").transform;
        parent.SetParent(root);
        Prepare(count);
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
    public T Spawn(Vector3 position, Vector3 forward = default(Vector3))
    {
        if (!objectList.TryFind(d => !d.gameObject.activeSelf, out T obj))
        {
            Despawn(objectList.Last());
            obj = objectList.First();
        }
        obj.transform.position = position;
        if (forward != default(Vector3))
            obj.transform.forward = forward;
        obj.gameObject.SetActive(true);
        return obj;
    }
    public void Despawn(T target)
    {
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