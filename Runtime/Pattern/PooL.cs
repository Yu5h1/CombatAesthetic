////#define DEBUG_MODE
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.PlayerLoop;
//using Type = System.Type;

//namespace Yu5h1Lib
//{
//    [System.Serializable]
//    public class PooL
//    {
//        public class Map
//        {
//            public Object Source { get; private set; }
//            public Object Element { get; private set; }
//            public Map(Object source, Object element)
//            { 
//                Source = source;
//                Element = element;                
//            }

//            private static Dictionary<Object, Map> _Maps;
//            public static Dictionary<Object, Map> elementMaps
//            {
//                get
//                {
//                    if (_Maps == null)
//                        _Maps = new Dictionary<Object, Map>();
//                    return _Maps;
//                }
//            }
//            public void Register(Object element)
//            {
//                elementMaps.Add(element, this);
//            }
//        }
//        [System.Serializable]
//        public struct Config
//        {
//            public int PrepareCount;
//            public int Capacity;
//            public static Config Default => new Config() { PrepareCount = 2, Capacity = 5 };
//        }
//        public string Name;

//        protected Object _Source;
//        public Object Source { get => _Source; private set => _Source = value; }

//        [SerializeField,ReadOnly]
//        private HashSet<Object> _elements = new HashSet<Object>();
//        public HashSet<Object> elements { get => _elements; private set => _elements = value; }
//        protected LinkedList<Object> list = new LinkedList<Object>();
//        protected Queue<Object> history = new Queue<Object>();
//        public int Size => elements.Count;
//        public int Capacity;

//        public bool UseFIFO;

//        public event System.Action<Object> Init;
//        public static bool TryCreate<T>(T source, Transform root, Config config, out ComponentPool result, System.Action<Component> init) where T : Component
//        {
//            result = null;
//            if (source == null)
//                throw new System.InvalidOperationException("Invalid Source for the pool.");
//            if (root == null)
//                throw new System.InvalidOperationException("Pool require a Root Transform.");
//            result = new ComponentPool()
//            {
//                Name = source.name,
//                Capacity = config.Capacity,
//            };
//            result.Init += init;
//            result.parent.hideFlags = HideFlags.NotEditable;
//            result.parent.SetParent(root);
//            if (source.gameObject.IsBelongToActiveScene())
//            {
//                source.gameObject.SetActive(false);
//                source.gameObject.transform.SetParent(result.parent);
//            }
//            result.Prepare<T>(config.PrepareCount);
//            return true;
//        }
//        public void Prepare<T>(int count) where T : Component
//        {
//            if (!Source)
//                throw new MissingReferenceException("The source of Pool was missing but you are trying to access it.");
//            if (elements.Count >= count)
//                return;

//            for (int i = Size; i < count; i++)
//                Join(Create<T>());
//        }
//        protected virtual T Create<T>() where T : Object
//        {
//            var member = (T)GameObject.Instantiate(Source);
//            new Map(Source, member).Register(member);
//            Init?.Invoke(member);
//            return member;
//        }
//        private bool Join(Object obj)
//        {
//            if ($"{obj.name} already Exists !".printWarningIf(elements.Contains(obj)))
//                return false;
//            obj.name = $"{elements.Count}.{Name}";
//            elements.Add(obj);
//            list.AddLast(obj);
//            return true;
//        }
//        public bool ValidateType(Type type) 
//         => !$"Spawn type does not match. Pool Type :({Source.GetType()}) request Type : ({type})".
//             printWarningIf(type != Source.GetType());

//        public bool ValidateType<T>() where T : Component
//            => ValidateType(typeof(T));

//        private T GetElement<T>() where T : Component
//        {
//            var result = (T)list.First.Value;
//            list.RemoveFirst();
//            return result;
//        }
//        public T Spawn<T>(Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null) where T : Component
//        {
//            if (elements.IsEmpty())
//                throw new System.InvalidOperationException("No elements available in the pool.");
//            if (!ValidateType<T>())
//                return null;
//            T obj = null;

//            if (list.Count > 0)
//                obj = GetElement<T>();
//            else
//            {
//                if (Size < Capacity)
//                {
//                    Prepare<T>(Size + 1);
//#if UNITY_EDITOR && DEBUG_MODE
//                $"Add Element Size:{Size} Capacity:{Capacity}".print();
//#endif
//                    obj = GetElement<T>();
//                }
//                else if (UseFIFO)
//                {
//                    if (history.IsEmpty())
//                        foreach (var item in elements)
//                            history.Enqueue(item);

//                    var oldest = history.Dequeue();
//#if UNITY_EDITOR && DEBUG_MODE
//                $"ReUse last element {oldest}".print();
//#endif
//                    Despawn((T)oldest);
//                    history.Enqueue(oldest);
//                    obj = GetElement<T>();
//                }
//                else
//                {
//                    obj = Create<T>();
//#if UNITY_EDITOR && DEBUG_MODE
//                $"Size({Size}) is out of capacity({Capacity}) Instantiating unmanaged element{obj.name}".print();
//#endif
//                }
//            }
//            if (parent)
//                obj.transform.SetParent(parent, false);
//            obj.transform.position = position;
//            if (rotation != default(Quaternion))
//                obj.transform.rotation = rotation;
//            obj.gameObject.SetActive(true);
//            return obj;
//        }
//        public void Despawn(Object element)
//        {
//            if (!ValidateType(element.GetType()))
//                return;
//            if (!elements.Contains(element))
//            {
//                Destroy(element);
//#if UNITY_EDITOR && DEBUG_MODE
//            Debug.LogWarning($"{element} does not belone to Pool({parent.name}).The Object will be Destory.");
//#endif
//                return;
//            }
//            list.Remove(element);
//            list.AddLast(element);
//        }
//        protected virtual void Destroy(Object element)
//        {
//            GameObject.Destroy(element);
//        }
//    }
//}