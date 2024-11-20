using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    [System.Serializable]
    public class CollierCastInfo2D
    {
        [SerializeField]
        protected Collider2D _collider;
        public Collider2D collider { get => _collider; internal set => _collider = value; }
        public ContactFilter2D filter;
        public LayerMask layerMask { get => filter.layerMask; set => filter.layerMask = value; }
        public RaycastHit2D[] results;
        [Range(0, 1.0f)]
        public float distance = 0.2f;
        [Range(1, 10)]
        public int resultsCount = 5;
        public bool ignoreSiblingColliders = true;

        public RaycastHit2D this[int index] => results[index];


        public int Cast(Vector2 direction)
        {
            results = new RaycastHit2D[resultsCount];
            if (!collider)
                return 0;
            return collider.Cast(direction, filter, results, distance, ignoreSiblingColliders);
        }
    }
}
