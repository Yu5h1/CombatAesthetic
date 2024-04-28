using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
