//#define DEBUG_MODE

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Yu5h1Lib;

[System.Serializable]
public class Pool
{
    [System.Serializable]
    public struct Config{
        public int PrepareCount;
        public int Capacity;
        public static Config Default => new Config() { PrepareCount = 2, Capacity = 5 };
    }
    public string Name;

    [SerializeField,ReadOnly]
    private Transform _Root;
    public Transform Root { get => _Root; protected set => _Root = value; }
    [SerializeField, ReadOnly]
    private Transform _parent;
    public Transform parent { get => _parent; protected set => _parent = value; }
    [SerializeField, ReadOnly]
    private Component _Source;
    public Component Source { get => _Source; private set => _Source = value; }

    public HashSet<Component> elements { get; private set; } = new HashSet<Component>();
    protected LinkedList<Component> list = new LinkedList<Component>();
    protected Queue<Component> history = new Queue<Component>();
    public int Size => elements.Count;
    public int Capacity;

    public bool UseFIFO;

    public static bool TryCreate<T>(T source, Transform root, Config config, out Pool result) where T : Component
    {
        result = null;
        if (source == null )
            throw new System.InvalidOperationException("Invalid Source for the pool.");
        if (root == null)
            throw new System.InvalidOperationException("Pool require a Root Transform.");
        result = new Pool(){
            Root = root,
            Source = source,
            Name = source.name,
            parent = new GameObject($"{source.name}(Pool)").transform,            
            Capacity = config.Capacity,
        };
        result.parent.hideFlags = HideFlags.NotEditable;
        result.parent.SetParent(root);
        if (source.gameObject.IsBelongToActiveScene())
        {
            source.gameObject.SetActive(false);
            source.gameObject.transform.SetParent(result.parent);
        }
        result.Prepare<T>(config.PrepareCount);
        return true;
    }
    public void Prepare<T>(int count) where T : Component
    {
        if (!Source)
            throw new MissingReferenceException("The source of Pool was missing but you are trying to access it.");
        if (elements.Count >= count)
            return;

        for (int i = Size; i < count; i++)
            Join(Create<T>());
    }
    private T Create<T>() where T : Component
    {
        var element = (T)GameObject.Instantiate(Source);
        return element;
    }
    private bool Join(Component obj)
    {
        if ($"{obj.name} already Exists !".printWarningIf(elements.Contains(obj)))
            return false;
        obj.name = $"{elements.Count}.{Name}";
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(parent,false);
        elements.Add(obj);
        list.AddLast(obj);
        return true;
    }

    public bool ValidateType<T>() where T : Component
        => !$"Spawn type does not match. Pool Type :({Source.GetType()}) request Type : ({typeof(T)})".
            printWarningIf(typeof(T) != Source.GetType());

    private T GetElement<T>() where T : Component
    {
        var result = (T)list.First.Value;
        list.RemoveFirst();
        return result;
    }
    public T Spawn<T>(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null) where T : Component
    {
        if (elements.IsEmpty())
            throw new System.InvalidOperationException("No elements available in the pool.");
        if (!ValidateType<T>())
            return null;
        T obj = null;

        if (list.Count > 0)
            obj = GetElement<T>();
        else
        {
            if (Size < Capacity)
            {
                Prepare<T>(Size + 1);
#if UNITY_EDITOR && DEBUG_MODE
                $"Add Element Size:{Size} Capacity:{Capacity}".print();
#endif
                obj = GetElement<T>();
            }
            else if (UseFIFO)
            {
                if (history.IsEmpty())
                    foreach (var item in elements)
                        history.Enqueue(item);

                var oldest = history.Dequeue();
#if UNITY_EDITOR && DEBUG_MODE
                $"ReUse last element {oldest}".print();
#endif
                Despawn((T)oldest);
                history.Enqueue(oldest);
                obj = GetElement<T>();
            }
            else
            {
                obj = Create<T>();
#if UNITY_EDITOR && DEBUG_MODE
                $"Size({Size}) is out of capacity({Capacity}) Instantiating unmanaged element{obj.name}".print();
#endif
            }
        }
        if (parent)
            obj.transform.SetParent(parent, false);
        obj.transform.position = position;
        if (rotation != default(Quaternion))
            obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);
        return obj;
    }
    public void Despawn<T>(T element) where T : Component
    {
        if (!ValidateType<T>())
            return;
        if (!elements.Contains(element))
        {
            GameObject.Destroy(element.gameObject);
#if UNITY_EDITOR && DEBUG_MODE
            Debug.LogWarning($"{element} does not belone to Pool({parent.name}).The Object will be Destory.");
#endif
            return;
        }
        element.gameObject.SetActive(false);
        element.transform.SetParent(parent, false);
        list.Remove(element);
        list.AddLast(element);
    }
}