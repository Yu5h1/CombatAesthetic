using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game.Character;

public class Patrol : MonoBehaviour
{
    [SerializeField]
    private float _RangeDistance = 2;
    public float RangeDistance => _RangeDistance;

    public Vector2 point { get; private set; }

    public void Reset()
    {
        point = transform.position;
    }
#if UNITY_EDITOR
    ColliderDetector2D detector;
    Patrol patrol;
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            if (!detector)
            {
                detector = GetComponent<ColliderDetector2D>();
                detector.Init();
            }
            if (!patrol)
                patrol = GetComponent<Patrol>();
            if (!detector)
                return;
            var from = transform.TransformPoint(-patrol.RangeDistance, -detector.extents.y);
            var fixedHeight = transform.up * detector.extents.y;
            Gizmos.DrawLine(from, from + fixedHeight);
            var to = transform.TransformPoint(patrol.RangeDistance, -detector.extents.y);
            Gizmos.DrawLine(to, to + fixedHeight);
        }
    }
#endif

}
