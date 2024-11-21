using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public void Despawn()
    {
        PoolManager.instance.Despawn(gameObject.transform);
    }
}
