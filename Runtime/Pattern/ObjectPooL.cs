using UnityEngine;
using UnityEngine.Pool;


public class Pool
{
    public string Name;
    public bool resizeable = true;
    public Transform Root { get; protected set; }
    public Transform parent { get; protected set; }
    public Component Source { get; private set; }

    private ObjectPool<Component> pool;

    public int Size => pool.CountAll;

    private Pool(Component source, Transform root, int count)
    {
        Root = root;
        Source = source;
        Name = source.name;
        parent = new GameObject($"{Name}(Pool)").transform;
        parent.SetParent(root);

        if (source.gameObject.IsBelongToActiveScene())
        {
            source.gameObject.SetActive(false);
            source.gameObject.transform.SetParent(parent);
        }
        Prepare(count);
    }

    public static bool TryCreate<T>(T source, Transform root, int count, out Pool result) where T : Component
    {
        result = null;
        if (source == null || root == null || count <= 0)
            return false;
        result = new Pool(source, root, count);
        return true;
    }

    public void Prepare(int maxSize)
    {
        if (!Source)
            throw new MissingReferenceException("The source of Pool was missing but you are trying to access it.");
        if (pool != null && pool.CountActive >= maxSize) return;

        pool = new ObjectPool<Component>(
            createFunc: () => GameObject.Instantiate(Source, parent), 
            actionOnGet: obj => obj.gameObject.SetActive(true), 
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => GameObject.Destroy(obj.gameObject),
            maxSize: maxSize 
        );
    }

    public bool ValidateType<T>()
        => !$"Spawn type does not match. Pool Type :({Source.GetType()}) request Type : ({typeof(T)})".
            printWarningIf(typeof(T) != Source.GetType());

    public T Spawn<T>(Vector3 position, Quaternion rotation = default(Quaternion), Transform parent = null) where T : Component
    {
        if (!ValidateType<T>())
            return null;
        Component obj = pool.Get();  // 從池中獲取物件
        if (parent)
            obj.transform.SetParent(parent, false);
        obj.transform.position = position;
        if (rotation != default(Quaternion))
            obj.transform.rotation = rotation;

        return (T)obj;
    }
    public void Despawn<T>(T obj) where T : Component
    {
        if (!ValidateType<T>())
            return;
        pool.Release(obj);
    }
}

//public class Pool 
//{
//    public string Name;
//    public bool resizeable = true;
//    public Transform Root { get; protected set; }
//    public Transform parent { get; protected set; }
//    public Component Source { get; private set; }

//    public HashSet<Component> elements { get; private set; } = new HashSet<Component>();
//    protected LinkedList<Component> queue= new LinkedList<Component>();
//    public int Size => elements.Count;
//    private Pool(Component source, Transform root, int count) {
//        Root = root;
//        Source = source;
//        Name = source.name;
//        parent = new GameObject($"{Name}(Pool)").transform;
//        parent.SetParent(root);

//        if (source.gameObject.IsBelongToActiveScene())
//        {
//            source.gameObject.SetActive(false);
//            source.gameObject.transform.SetParent(parent);
//        }
//        Prepare(count);
//    }
//    public static bool TryCreate<T>(T source, Transform root, int count,out Pool result) where T : Component
//    {
//        result = null;
//        if (source == null || root == null || count <= 0)
//            return false;
//        result = new Pool(source, root, count);
//        return true;
//    }
//    public void Prepare(int count)
//    {
//        if (!Source)
//            throw new MissingReferenceException("The source of Pool was missing but you are trying to access it.");
//        if (elements.Count >= count)
//            return;

//        for (int i = elements.Count; i < count; i++)
//            Join(GameObject.Instantiate(Source, parent));
//    }
//    private bool Join(Component obj)
//    {
//        if ($"{obj.name} already Exists !".printWarningIf(elements.Contains(obj)))
//            return false;
//        obj.name = $"{elements.Count}.{Name}";
//        obj.gameObject.SetActive(false);
//        elements.Add(obj);
//        queue.AddLast(obj);
//        return true;
//    }

//    public bool ValidateType<T>()
//        => !$"Spawn type does not match. Pool Type :({Source.GetType()}) request Type : ({typeof(T)})".
//            printWarningIf(typeof(T) != Source.GetType());

//    public T Spawn<T>(Vector3 position, Quaternion rotation = default(Quaternion),Transform parent = null) where T : Component
//    {
//        if (!ValidateType<T>())
//            return null;
//        if (queue.Count <= 0)
//        {
//            Component first = null;
//            if (resizeable)
//                Prepare(Size + 1);
//            else
//            {
//                first = queue.First.Value;
//                queue.RemoveFirst();
//                Despawn(first);
//            }
//#if UNITY_EDITOR
//            (resizeable ? $"Resize Pool({Name}) {Size}" : $"ReUse last element {first}").print();
//#endif
//        }
//        var obj = queue.First.Value;
//        queue.RemoveFirst();
//        if (parent)
//            obj.transform.SetParent(parent, false);
//        obj.transform.position = position;
//        if (rotation != default(Quaternion))
//            obj.transform.rotation = rotation;
//        obj.gameObject.SetActive(true);
//        return (T)obj;
//    }
//    public void Despawn<T>(T obj) where T : Component
//    {
//        if (!ValidateType<T>())
//            return;
//        if (!elements.Contains(obj))
//        {
//            Debug.LogWarning($"{obj} does not belone to Pool({parent.name})");
//            return;
//        }
//        obj.gameObject.SetActive(false);
//        obj.transform.SetParent(parent, false);
//        queue.Remove(obj);
//        queue.AddLast(obj);
//    }
//}