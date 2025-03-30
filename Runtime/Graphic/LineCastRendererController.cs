using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib.Runtime;

namespace Yu5h1Lib
{
    public class LineCastRendererController : LineRendererController
    {
        [SerializeField, Range(0, 10)]
        private int segmentsPerSegment = 5;
        [SerializeField]
        private List<Transform> targets;

        [SerializeField]
        private TagLayerMask tagLayerMask;

        [SerializeField]
        private MinMax depthRange;


        #region Events
        [SerializeField]
        private UnityEvent _HitStateChanged;
        public event UnityAction HitStateChanged
        {
            add => _HitStateChanged.AddListener(value);
            remove => _HitStateChanged.RemoveListener(value);
        }

        #endregion

        protected Vector3[] positionsCache;
        private bool _IsHit;
        private bool IsHit
        {
            get => _IsHit;
            set
            {
                if (_IsHit == value)
                    return;
                _IsHit = value;
                OnHitStateChanged();
            }
        }

        public RaycastHit2D hitInfo { get; private set; }

        private void FixedUpdate()
        {
            Refresh();
        }
        [ContextMenu(nameof(Refresh))]
        public override void Refresh()
        {
            if (!IsConnecting || targets.IsEmpty() || targets.Count < 2)
                return;

            bool shouldLoop = lineRenderer.loop && targets.Count > 2;

            int segmentCount = targets.Count - (shouldLoop ? 0 : 1);
            int requiredSize = targets.Count;

            if (segmentsPerSegment > 0)
                requiredSize += segmentCount * segmentsPerSegment;

            if (positionsCache == null || positionsCache.Length != requiredSize)
                positionsCache = new Vector3[requiredSize];

            int index = 0;
            hitInfo = default(RaycastHit2D);
            for (int i = 0; i < targets.Count - 1; i++)
            {
                hitInfo = ProcessSegment(targets[i].position, targets[i + 1].position, ref index);
                if (hitInfo)
                    break;
            }
            //var lastPos = hitInfo ? (Vector3)hitInfo.point : targets[targets.Count - 1].position;

            var lastPos = targets[targets.Count - 1].position;

            if (shouldLoop)
            {
                if (!hitInfo)
                    ProcessSegment(lastPos, targets[0].position, ref index);

            }
            else
                positionsCache[positionsCache.Length - 1] = lastPos;

            IsHit = hitInfo;

            lineRenderer.positionCount = requiredSize;
            lineRenderer.SetPositions(positionsCache);
        }
        RaycastHit2D ProcessSegment(Vector3 start, Vector3 end, ref int index)
        {
            positionsCache[index++] = start;
            var result = default(RaycastHit2D);

            if (!IsPerforming)
            {
                var hit = depthRange.Length > 0 ?
                    Physics2D.Linecast(start, end, tagLayerMask.layers, depthRange.Min, depthRange.Max) :
                    Physics2D.Linecast(start, end, tagLayerMask.layers);
#if UNITY_EDITOR
                if (hit)
                    Debug.DrawLine(start, hit.point);
#endif
                if (hit && tagLayerMask.Validate(this,hit.transform))
                    result = hit;
            }


            if (segmentsPerSegment > 0)
            {
                var interval = 1.0f / (segmentsPerSegment + 1);
                var t = interval;
                for (int j = 0; j < segmentsPerSegment; j++)
                {
                    positionsCache[index++] = Vector3.Lerp(start, end, t);
                    t += interval;
                }
            }

            return result;
            //if (hit && tagOption.Compare(hit.transform.gameObject))
            //    return hit;
            //return default(RaycastHit2D);
        }
        #region Event

        protected void OnHitStateChanged()
        {
            _HitStateChanged?.Invoke();
        }
        public void BreakIfHited()
        {
            if (IsHit)
                IsConnecting = false;
        }
        private void OnDestroy()
        {
        }
        #endregion
        void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            if (targets != null)
            {
                foreach (var target in targets)
                {
                    if (target != null)
                        Gizmos.DrawSphere(target.position, 0.1f);
                }
            }
        }

        
    }

}