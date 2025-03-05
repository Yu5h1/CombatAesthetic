using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Yu5h1Lib.Game.Character
{
    [System.Serializable]
    public class CollierCastInfo2D
    {
        [SerializeField]
        protected Collider2D _collider;
        public Collider2D collider { get => _collider; internal set => _collider = value; }

        public float distance = 0.2f;
        [Range(1, 10),SerializeField]
        private int _resultsCount = 5;
        public int resultsCount {
            get => _resultsCount;
            set{
                if (_resultsCount == value) return;
                _resultsCount = Mathf.Clamp(value, 1, 10);
                results = new RaycastHit2D[_resultsCount];
            }
        }
        public bool ignoreSiblingColliders = true;

        public ContactFilter2D filter;
        public LayerMask layerMask { get => filter.layerMask; set => filter.layerMask = value; }

        [ReadOnly]
        public RaycastHit2D[] results;

        public RaycastHit2D this[int index] => results[Mathf.Clamp(index, 1, 10)];

        public virtual void Init(){
            filter.useTriggers = false;
            filter.useLayerMask = true;
            results = new RaycastHit2D[resultsCount];
        }
        public int Cast(Vector2 direction)
            => collider ? collider.Cast(direction, filter, results, distance, ignoreSiblingColliders) : 0;
    }
}
