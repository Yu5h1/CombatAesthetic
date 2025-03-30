using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Yu5h1Lib.Game.Character
{
    public class ColliderDetector2D : BaseColliderDetector2D
    {
        #region Layers
        public static LayerMask PhysicsObject => LayerMask.GetMask("PhysicsObject");
        public static LayerMask GravitationalObject => LayerMask.GetMask("GravitationalObject");
        #endregion
        public const string MovingPlatformTag = "MovingPlatform";

        public bool IsValid() => collider && enabled;
        #region Ground detection parameters
        public RaycastHit2D groundHit { get; private set; }
        private bool _IsGrounded;
        public bool IsGrounded 
        { 
            get => _IsGrounded;
            private set {
                if (IsGrounded == value)
                    return;
                _IsGrounded = value;
                OnGroundStateChanged();
            }
        }
        [SerializeField, Range(0.00001f, 1.0f)]
        private float groundRayOffset = 0.25f;
        [SerializeField, Range(0.01f, 1.0f)]
        private float groundDistanceThreshold = 0.05f;
        #endregion
        //public CollierCastInfo2D forwardCastInfo = new CollierCastInfo2D();

        [SerializeField]
        private UnityEvent<bool> _GroundStateChanged;
        public event UnityAction<bool> GroundStateChanged
        { 
            add => _GroundStateChanged.AddListener(value);
            remove => _GroundStateChanged.RemoveListener(value);
        }

        //[SerializeField]
        //private ColliderScanner2D _scanner;
        //public ColliderScanner2D scanner => _scanner;

        protected override void OnInitializing()
        {
            base.OnInitializing();
            //scanner.filter.layerMask = LayerMask.GetMask("Character");
            filter = new ContactFilter2D()
            {
                useLayerMask = true,
                layerMask = PhysicsObject | GravitationalObject
            };
            if (!collider && TryGetComponent(out CapsuleCollider2D c))
                collider = c;
            ignoreSiblingColliders = true;
            //scanner.Init(transform);
        }
        private void LateUpdate()
        {
            if (IsGrounded && groundHit && groundHit.collider.gameObject.CompareTag(MovingPlatformTag))
                CheckPlatformStandHeight();
        }
        void OnGroundStateChanged()
        {
            _GroundStateChanged?.Invoke(IsGrounded);
            if (IsGrounded)
            {
                CheckPlatformStandHeight(0);
                if (groundHit.collider.gameObject.CompareTag(MovingPlatformTag))
                    transform.SetParentAndUnitScale(groundHit.collider.transform, true);
            }
            else
            {
                if (transform.parent != null)
                    transform.SetParentAndUnitScale(null, true);
            }
        }
        #region Check Methods
        /// <summary>
        /// returb slopDir
        /// </summary>
        /// <param name="right"></param>
        public Vector2 CheckSlop(bool right)
        {
            var n = groundHit.normal;
            /// simple solution
            var slopDir = right ? new Vector2(n.y, -n.x) : new Vector2(-n.y, n.x);
#if UNITY_EDITOR
            Debug.DrawRay(groundHit.point, slopDir.normalized * extents.x, Color.green);
#endif
            return slopDir;
        }
        public void LeaveGround()
        {
            if (!IsGrounded)
                return;
            results = new RaycastHit2D[ResultsCount];
            groundHit = default(RaycastHit2D);
            IsGrounded = false;
        }
        #region WIP
        public void CheckForward()
        {
            if (!collider)
                return;

        }
        public void CheckMomentum()
        {
            
        }

        #endregion
        public void CheckGround(Vector2 direction)
        {
            // floating
            if (!IsGrounded && transform.InverseTransformDirection(velocity.normalized).y > 0)
                return;
            results = new RaycastHit2D[ResultsCount];
            groundHit = default(RaycastHit2D);

            for (int i = 0; i < Cast(direction); i++)
            {
                if (Physics2D.GetIgnoreCollision(results[i].collider, collider))
                    continue;
                var hit = results[i];
                if (!hit)
                    continue;
                var p = (Vector2)transform.InverseTransformPoint(hit.point);
                var normal = transform.InverseTransformDirection(hit.normal);
                var localbottom = (Vector2)transform.InverseTransformPoint(bottom);
                if (groundHit && groundHit.distance < hit.distance)
                    continue;
                /// normal.y > 0.5f = slop angle > 45¢X
                if (normal.y > 0.5f && p.y <= localbottom.y + groundRayOffset && hit.distance < groundDistanceThreshold)
                {
                    groundHit = hit;
#if UNITY_EDITOR
                    Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green);
#endif
                }
            }
#if UNITY_EDITOR
            if (groundHit)
                Debug.DrawLine(center, groundHit.point, Color.yellow);
            else
                Debug.DrawLine(center, bottom);
#endif
            IsGrounded = groundHit;
        }
        public void CheckPlatformStandHeight(float Threshold = 0.05f)
        {
            if (!groundHit)
                return;
            var surfaceHitPoint = groundHit.collider.ClosestPoint(groundHit.point);
            var localclosetGroundPoint = (Vector2)transform.InverseTransformPoint(surfaceHitPoint);
            var localBottomY = -extents.y + offset.y;
            var distance = localBottomY.Distance(localclosetGroundPoint.y);
            if (Threshold <= 0 || distance > Threshold)
            {
                transform.position = transform.TransformPoint(0, Mathf.
                    Sign(localclosetGroundPoint.y - localBottomY) * distance); 
                
            }
        }
        public bool CheckCliff()
        {
            if (!collider || !IsGrounded)
                return false;
            var pos = bottom + (right * extents.x);
            var hitground = Physics2D.Raycast(pos, down, 1, layerMask.value);
            pos += up * 0.1f;
            if (hitground)
            {
                Debug.DrawRay(pos, down * (hitground.distance + 0.1f), Color.white, Time.deltaTime);
                return false;
            }
            Debug.DrawRay(pos, down, Color.white, Time.deltaTime);
            return true;
        }

        public Vector2 ClosestPoint(Vector2 pos) => collider.ClosestPoint(pos);

        public RaycastHit2D CheckObstacle()
        {
            //var hit = collider.Cast( right, 0.5f, 0),
            //    new Vector2(2, host.size.y - 1),
            //    host.rotation.z,
            //    host.ForwardVector2,
            //    0,
            //    host.sceneObjectLayer | host.effectController.BodyColliderLayer
            //);

            //if (hit)
            //{
            //    ObstacleAngle = Vector2.Angle(hit.normal, Vector3.up);
            //}
            //else
            //{
            //    ObstacleAngle = 0;
            //}
            return default(RaycastHit2D);
        }        
#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (!collider || !groundHit)
                return;
            var color = Gizmos.color;
            var reverseHroundHitPoint = groundHit.point + groundHit.normal;
            Gizmos.DrawSphere(reverseHroundHitPoint, 0.1f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(groundHit.collider.ClosestPoint(groundHit.point), 0.05f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundHit.point, 0.15f);
            Gizmos.color = color;
        }
#endif

        #endregion

        #region ContextMenu

        //[ContextMenu(nameof(UsedefaultScannerSetting))]
        //public void UsedefaultScannerSetting()
        //{
        //    scanner.layerMask = LayerMask.GetMask("Character");
        //    scanner.Tag.tag = "Player";
        //    scanner.ObstacleMask = LayerMask.GetMask("PhysicsObject");
        //    scanner.Tag.type = TagOption.ComparisionType.Equal;
        //    scanner.filter.useTriggers = false;
        //    scanner.filter.useLayerMask = true;
        //    scanner.direction = Vector2.zero;
        //    scanner.useCircleCast = true;
        //    scanner.distance = 10;
        //}

        #endregion
    }
}
