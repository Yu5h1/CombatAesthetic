using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yu5h1Lib
{
    public class ResourcesManager : SingletonBehaviour<ResourcesManager>
    {


        protected override void Init()
        {
            
        }


        private Dictionary<string, Object> resourceCache = new Dictionary<string, Object>();

        public void PreloadResource<T>(string path) where T : Object
        {
            if (!resourceCache.ContainsKey(path))
            {
                T resource = Resources.Load<T>(path);
                if (resource != null)
                {
                    resourceCache[path] = resource;
                }
                else
                {
                    Debug.LogWarning($"[ResourcesManager] 資源加載失敗: {path}");
                }
            }
        }
        public T GetResource<T>(string path) where T : Object
        {
            if (resourceCache.TryGetValue(path, out Object cachedResource))
            {
                return cachedResource as T;
            }

            T resource = Resources.Load<T>(path);
            if (resource != null)
            {
                resourceCache[path] = resource;
                return resource;
            }
            Debug.LogWarning($"[ResourcesManager] 找不到資源: {path}");
            return null;
        }

        public void UnloadResource(string path)
        {
            if (resourceCache.ContainsKey(path))
            {
                resourceCache.Remove(path);
                Resources.UnloadUnusedAssets();
                Debug.Log($"[ResourcesManager] 釋放資源: {path}");
            }
        }

        public void ClearCache()
        {
            resourceCache.Clear();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            Debug.Log("[ResourcesManager] 已清除所有資源");
        }

    }

}