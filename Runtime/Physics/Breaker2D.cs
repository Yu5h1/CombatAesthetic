using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class Breaker2D : MonoBehaviour
{
    private void Start() {}

    public void Break(Collision2D collision)
	{
        if (!isActiveAndEnabled)
            return;
        var key = $"{collision.gameObject.GetOriginalName()}_fragments";
        if ($"{key} does not exist.".printWarningIf(!PoolManager.Exists(key)))
            return;
        collision.gameObject.SetActive(false);
        var t = PoolManager.Spawn<Transform>(key, collision.transform.position, collision.transform.rotation);
        t.localScale = collision.transform.localScale;
            

    }
}
