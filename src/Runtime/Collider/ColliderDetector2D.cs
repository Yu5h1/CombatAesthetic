using UnityEngine;
using UnityEngine.Events;

namespace Yu5h1Lib.Game.Character
{
    public class ColliderDetector2D : Rigidbody2DBehaviour
    {
        public new Collider2D collider;
        #region Ground detection parameters
        private ContactFilter2D GroundFilter;
        public LayerMask GroundLayer => GroundFilter.layerMask;
        private RaycastHit2D[] CharacterColliderDownCastResults;
        public RaycastHit2D groundHit { get; private set; }
        public bool IsGrounded { get; private set; }
        [SerializeField,Range(0.00001f,1.0f)]
        private float groundRayDistance = 0.2f;
        [SerializeField, Range(0.00001f, 1.0f)]
        private float groundRayOffset = 0.25f;
        #endregion
        public UnityEvent<bool> OnGroundStateChangedEvent;
        public Vector2 center => collider.bounds.center;
        public Vector2 front => center + new Vector2(collider.bounds.extents.x * transform.forward.z,0);
        public Vector2 bottom => center + new Vector2(0, -collider.bounds.extents.y);
        private int PlatformLayer;
        protected override void Reset()
        {
            if (TryGetComponent(out Controller2D character))
                collider = GetComponent<CapsuleCollider2D>();
        }
        void Awake()
        {
            GroundFilter = new ContactFilter2D()
            {
                useLayerMask = true,
                layerMask = LayerMask.GetMask("Platform", "PhysicsObject")
            };
            if (collider == null && TryGetComponent(out CapsuleCollider2D c))
                collider = c;
            PlatformLayer = LayerMask.NameToLayer("Platform");
        }
        private void FixedUpdate()
        {
            CheckGroundState();
        }
        private void LateUpdate()
        {
            CheckPlatformStandHeight();
        }
        private void OnDrawGizmosSelected()
        {
            if (groundHit)
            {
                var color = Gizmos.color;
                Gizmos.DrawSphere(groundHit.point + groundHit.normal * groundHit.distance , 0.1f);
                // other point
                Gizmos.DrawWireSphere(groundHit.collider.ClosestPoint(center), 0.05f);
                Gizmos.color = Color.yellow;
                // hit point
                Gizmos.DrawWireSphere(groundHit.point, 0.15f);
                 Gizmos.color = color;
            }    
        }
        public void CheckGroundState()
        {
     
            CharacterColliderDownCastResults = new RaycastHit2D[5];
            groundHit = default(RaycastHit2D);
            collider.Cast(Vector2.down, GroundFilter, CharacterColliderDownCastResults, groundRayDistance, true);
            foreach (var hit in CharacterColliderDownCastResults)
            {
                if (!hit)
                    continue;
                /// normal.y > 0.5f = slop angle > 45¢X
                if (hit.normal.y > 0.5f && hit.point.y <= bottom.y + groundRayOffset && hit.distance < 0.05)
                    groundHit = hit;
#if UNITY_EDITOR
                    Debug.DrawLine(center, hit.point, Color.green);
                    Debug.DrawLine(center, hit.point + hit.normal * hit.distance, Color.magenta);
#endif
            }

#if UNITY_EDITOR
            if (groundHit)
                Debug.DrawLine(center, groundHit.point, Color.yellow);
            else
                Debug.DrawLine(center, bottom);
#endif
            if (IsGrounded == groundHit)
                return;
            IsGrounded = groundHit;
            OnGroundStateChanged();
        }
        void OnGroundStateChanged()
        {
            OnGroundStateChangedEvent?.Invoke(IsGrounded);
        }
        private void CheckPlatformStandHeight()
        {
            if (!groundHit || groundHit.collider.gameObject.layer != PlatformLayer)
                return;
            var edgehitPoint = groundHit.point + groundHit.normal * groundHit.distance;
            var otherHitPoint = groundHit.collider.ClosestPoint(center);
            if (Vector2.Distance(edgehitPoint, otherHitPoint) > 0.05f)
            {
                var pos = transform.position;
                pos.y = otherHitPoint.y + collider.bounds.extents.y - collider.offset.y;
                transform.position = pos;
                Debug.Log("Fix incorrect height caused by rigibody momentum");
            }
        }
        public bool CheckEdge()
        {
            if (!collider || !IsGrounded)
                return false;
            var pos = front;
            pos.y -= collider.bounds.extents.y;
            var hitground = Physics2D.Raycast(pos,-transform.up, 1, GroundLayer.value);
            pos += Vector2.up * 0.1f;
            if (hitground)
            {
                Debug.DrawRay(pos, -transform.up * (hitground.distance + 0.1f), Color.white, Time.deltaTime);
                return false;
            }
            Debug.DrawRay(pos, -transform.up , Color.white, Time.deltaTime);
            return true;
        }
    }
}
