using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class MonoBehaviourEnhance : MonoBehaviour
{
    public void Log(string message)
    {
        Debug.Log(message);
    }
    public void Despawn()
    {
        PoolManager.instance.Despawn(gameObject.transform);
    }
    public void DestoryOnLoad(GameObject obj)
    {
        SceneManager.MoveGameObjectToScene(obj,SceneManager.GetActiveScene());
    }
}
//public abstract class MonoBehaviourEnhance<T> : MonoBehaviour where T : Component
//{
//    private T _component;
//    public T component => _component;
//    public bool Initinalized { get; private set; }

//    private void Initinalize()
//    {
//        if (Initinalized)
//            return;
//        TryGetComponent(out _component);
//        Init();
//        Initinalized = true;
//    }
//    /// <summary>
//    /// Init Can only be called once. Default call will be in OnEnable
//    /// </summary>
//    protected abstract void Init();

//    protected virtual void OnEnable()
//    {
//        Initinalize();
//    }
//}