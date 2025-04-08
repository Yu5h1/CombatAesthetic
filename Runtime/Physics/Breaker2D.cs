using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class Breaker2D : MonoBehaviour
{

    public UnityEvent<Transform> _breakChild;

    private void Start() {}
    public void Break(Collision2D collision)
	{
        if (!isActiveAndEnabled)
            return;
        var key = $"{collision.gameObject.GetOriginalName()}_fragments";
        if ($"{key} does not exist.".printWarningIf(!PoolManager.Exists(key)))
            return;
        if (collision.gameObject.CompareTag(ColliderDetector2D.MovingPlatformTag))
        {
            for (int i = collision.transform.childCount - 1; i >= 0; i--)
            {
                _breakChild?.Invoke(collision.transform.GetChild(i));
                collision.transform.GetChild(i).parent = null;
            }
        }
        collision.gameObject.SetActive(false);

        var t = PoolManager.Spawn<Transform>(key, collision.transform.position, collision.transform.rotation);
        t.localScale = collision.transform.localScale;
            

    }
}
