using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Yu5h1Lib.Game.Character
{
    [DisallowMultipleComponent, RequireComponent(typeof(ColliderDetector2D))]
    public class Controller2D : Rigidbody2DBehaviour
    {
        private static float _gravityScale = 0.03333f;
        public static Vector2 scaledGravity { get; protected set; } = new Vector2(0, -0.32699673f);
        public static float gravityScale
        { 
            get => _gravityScale;
            set {
                if (_gravityScale == value)
                    return;
                scaledGravity = Physics2D.gravity * gravityScale;
            }
        }
        #region Components   
        [SerializeField]
        private Collider2D _hurtBox;
        public Collider2D hurtBox => _hurtBox;
        [SerializeField, ReadOnly]
        private AttributeBehaviour _attribute;
        public AttributeBehaviour attribute => _attribute;
        [SerializeField, ReadOnly]
        private ColliderDetector2D _detector;
        public ColliderDetector2D detector => _detector;
        public bool IsGrounded => detector.IsGrounded;
        public bool IsInteracting => !detector.enabled;
        #endregion

        [SerializeField] protected float JumpPower = 6;
        [SerializeField] protected float MaxAirborneSpeed = 3.5f;
        [SerializeField] protected Vector2 AirborneMultiplier = new Vector2(0.1f, 0.025f);
        [SerializeField] protected Vector2 FloatingMultiplier = new Vector2(0.15f, 0.5f);
        [SerializeField] protected Vector2 GroundMultiplier = new Vector2(1, 1);
        [SerializeField] protected bool _Floatable;


        #region Events
        [SerializeField]
        protected Vector2Event _Hited;
        public event UnityAction<Vector2> Hited
        {
            add => _Hited.AddListener(value);
            remove => _Hited.RemoveListener(value);
        }
        #endregion


        #region Instructions
        [SerializeField]
        protected HostData2D _Host;
        public HostData2D host
        {
            get => _Host;
            set
            {
                if (_Host == value)
                    return;
                hostBehaviour = null;
                _Host = value;
                if (value == null)
                    return;
                hostBehaviour = host.CreateBehaviour(this);
            }
        }
        public HostData2D.HostBehaviour2D hostBehaviour;

        protected bool TriggerJump = false;
        protected Vector2 _Movement;
        public Vector2 InputMovement
        {
            get => _Movement;
            protected set
            {
                TriggerJump = _Movement.y == 0 && value.y > 0;
                if (_Movement == value)
                    return;
                _Movement = value;
            }
        }
        public float BoostMultiplier { get => GroundMultiplier.x; set => GroundMultiplier.x = value; }

        public bool Floatable { get => _Floatable; protected set => _Floatable = value; }
        protected bool _underControl;
        public bool underControl { get => _underControl && Conscious > 0; protected set => _underControl = value; }
        public int Conscious { get; protected set; } = 100;
        #endregion

        #region operating parameters
        public float forwardSign => IsFaceForward ? 1 : -1;
        public bool IsFaceForward => transform.forward.z > 0;

        public Vector2 position => transform.position;
        public Vector2 center => detector.center;

        protected Vector2 floating_v_temp = Vector2.zero;

  

        #endregion

        #region Movement
        public bool UseCustomVelocity;
        private Vector2 _velocity;
        public override Vector2 velocity
        {
            get => UseCustomVelocity ? _velocity : rigidbody.velocity;
            protected set
            {
                if (UseCustomVelocity)
                    _velocity = value;
                else
                    rigidbody.velocity = value;

                if (velocity.y > 0 || Mathf.Approximately(velocity.y, 0))
                {
                    lastfallingHeight = transform.position.y;
                        //lastFallingTime = Time.time;
                }
                    
            }
        }
        public Vector2 localVelocity { get; protected set; }

        [SerializeField]
        private Vector2 _BaseGravityDirection;
        public Vector2 baseGravityDirection
        { 
            get => _BaseGravityDirection.magnitude == 0 ? Vector2.up : _BaseGravityDirection;
            protected set => _BaseGravityDirection = value;
        }

        public Vector2 overrideGravityDirection { get; set; }

        public Vector2 gravityDirection => overrideGravityDirection.magnitude == 0 ? baseGravityDirection : overrideGravityDirection;
        public bool UseTransformUpAsGravitationOnStart;
        #endregion
        public bool IsInvincible => !attribute || !attribute.isActiveAndEnabled;

        private float lastfallingHeight;
        //protected float lastFallingTime;
        //public float FallingTimeElapsed => Time.time - lastFallingTime;


        public bool Initinalized { get; private set; }
        private void Initinalize()
        {
            if (Initinalized)
                return;
            Init();
            Initinalized = true;
        }
        protected virtual void Init()
        {            
            if (!$"{name}'s Detector does not Exist ! ".printWarningIf(!TryGetComponent(out _detector)))
                detector.GroundStateChanged += OnGroundStateChanged;
            if (!$"{name}'s Attribute does not Exist ! ".printWarningIf(!TryGetComponent(out _attribute)))
                attribute.Init();

            if (rigidbody)
                rigidbody.gravityScale = 0;

            if (host)
                hostBehaviour = host.CreateBehaviour(this);

            if (UseTransformUpAsGravitationOnStart)
                baseGravityDirection = up;

            lastfallingHeight = transform.position.y;
        }
        protected virtual void OnGroundStateChanged(bool grounded)
        {
            if (grounded)
            {
                var fallingDistance = lastfallingHeight - detector.groundHit.point.y;
                var unbouncable = detector.groundHit.rigidbody == null || detector.groundHit.rigidbody.sharedMaterial == null || detector.groundHit.rigidbody.sharedMaterial.bounciness == 0;
                if (unbouncable && attribute && localVelocity.y < -9.55f && fallingDistance > 5)
                {
                    var damage = Mathf.Floor((fallingDistance - 5) * 2) / 2; ;
                    $"velocity.y: {localVelocity.y} || fallingDistance:{fallingDistance} || Damage:{damage}".print();
                    attribute.Affect(AttributeType.Health, AffectType.NEGATIVE, damage);
                }

                localVelocity *= Vector2.right;
                rigidbody.Sleep();
                velocity = transform.TransformVector(localVelocity);                
                //ResetRotation();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Initinalize();
        }
        protected virtual void Update()
        {
            UpdateInputInstruction();
        }
        protected virtual void FixedUpdate()
        {
            PerformDetection();
            OnMove();
        }
        protected void PerformDetection()
        {
            if (!detector.IsValid())
                return;
            if (!Floatable)
                detector.CheckGround(down);
        }
        [ContextMenu("AddForce Test")]
        public void AddForceTest()
        {
            AddForce(Vector2.one * 5);
        }
        public void AddForce(Vector2 force)
        {
            velocity += force;
            detector.LeaveGround();
        }

        protected virtual void OnMove()
        {
            if (Time.timeScale == 0)
                return;
            if (IsInteracting) {
                velocity = Vector2.zero;
                return;
            }
            underControl = Conscious > 10;
            CheckForward();
            localVelocity = transform.InverseTransformDirection(velocity);
            var momentum = localVelocity;

            if (IsGrounded)
            {
                if (underControl)
                {
                    momentum.x = GroundMultiplier.x * Mathf.Abs(InputMovement.x);
                    if (TriggerJump)
                    {
                        momentum.y = JumpPower;
                        detector.LeaveGround();
                    }
                }
                if (momentum.y < JumpPower)
                {
                    /// fix bouncing while moving on slop
                    var localSlopDir = transform.InverseTransformDirection(detector.CheckSlop(IsFaceForward).normalized);
                    momentum = momentum.magnitude * localSlopDir;
                    if (detector.groundHit.distance > 0)
                        momentum += new Vector2(0, -detector.groundHit.distance * momentum.magnitude);
                }
            }
            else if (Floatable)
            {
                momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * FloatingMultiplier;
                momentum = Vector2.SmoothDamp(momentum, Vector2.zero, ref floating_v_temp, 0.3f);
            }
            else
            {
                if (momentum.y > Physics2D.gravity.y )
                    momentum += scaledGravity;
                if (Mathf.Abs(momentum.x) < MaxAirborneSpeed)
                    momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * AirborneMultiplier;
            }
            localVelocity = momentum;

            if (UseCustomVelocity)
                rigidbody.MovePosition(rigidbody.position + (velocity = transform.TransformDirection(momentum) * Time.fixedDeltaTime));
            else /// deprecated using velocity control movement . this method will causing flick movement
                velocity = transform.TransformDirection(momentum);
        }

        public void CheckForwardFrom(float x)
        {
            if (x == 0 )
                return;
            if (Mathf.Sign(x) == forwardSign)
                return;
            transform.Rotate(Vector3.up, 180, Space.Self);
            // Alternative
            //transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -forwardSign), transform.up);
        }
        public void CheckForward() => CheckForwardFrom(InputMovement.x);

        public bool DisableHostControl;
        protected virtual bool UpdateInputInstruction()
        {
            if (DisableHostControl || GameManager.IsGamePause || !Initinalized ||
                (hostBehaviour?.enable == false) || host == null)
            {
                InputMovement = Vector2.zero;
                return false;
            }
            InputMovement = hostBehaviour.GetMovement();
            return true;
        }
        public void ResetRotation()
        {
            var f = forwardSign;
            transform.localRotation = Quaternion.identity;
            CheckForwardFrom(f);
            //var parent = transform.parent;
            //transform.Rotate(Vector3.forward, GetStandingAngleGap(parent ? parent.up : Vector3.up));
        }

        protected float GetStandingAngleGap(Vector3 standDirection){
            var GdirAngleGap = Vector2.Angle(up, standDirection);
            return GetLocalDirection(standDirection, transform.up).x > 0 ? -GdirAngleGap : GdirAngleGap;
        }

        protected Vector2 GetLocalDirection(Vector2 baseDirection, Vector2 direction)
        {
            baseDirection.Normalize();
            float angle = Vector2.SignedAngle(Vector2.up, baseDirection);
            Quaternion rotation = Quaternion.Euler(0, 0, -angle);
            return rotation * direction;
        }

        public void ResetVelocity()
        {
            velocity = Vector2.zero;
        }

        public virtual void HitFrom(Vector2 v)
        {
            if (!isActiveAndEnabled || IsInvincible)
                return;
            if (!v.IsZero() && Vector2.Dot(v.normalized, right) > 0)
                CheckForwardFrom(-forwardSign);

        }
        #region Coroutine
        private Coroutine TemporarilyInvincibleCoroutine;
        public void InvincibleHalfSecondIfHurt(Vector2 force)
        {
            var stateNullable = attribute[AttributeType.Health];
            if (stateNullable == null)
                return;
            var state = stateNullable.Value;
            if (state.IsDepleted)
                return;
            this.StartCoroutine(ref TemporarilyInvincibleCoroutine, DisableForDuration(0.5f));
        }
        private IEnumerator DisableForDuration(float duration)
        {
            
            hurtBox.enabled = false;
            var renderer = GetComponent<SpriteRenderer>();
            var splitedDuration = duration / 5.0f;
            var flashColor = new Color(.8f, .8f, .8f, 1);
            for (int i = 0; i < 5; i++)
            {
                renderer.color = i % 2 == 0 ? Color.white : flashColor;
                yield return new WaitForSeconds(splitedDuration);
            }
            renderer.color = Color.white;
            //attribute.enabled = false;
            //yield return new WaitForSeconds(duration);
            hurtBox.enabled = true;
        }
        #endregion
    }
}

