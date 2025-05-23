using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Yu5h1Lib.Game.Character
{
    [DisallowMultipleComponent, RequireComponent(typeof(ColliderDetector2D))]
    public class CharacterController2D : Rigidbody2DBehaviour
    {
        #region Setting
        public static string GetPrefsKey(string parameterName) => $"{typeof(CharacterController2D).FullName}_{parameterName}";

        [SerializeField]
        private float _gravityScale = 0.03333f;
        public float gravityScale
        {
            get => _gravityScale;
            set
            {                
                if (gravityScale == value)
                    return;
                _gravityScale = value;                
            }
        }
        public Vector2 scaledGravity => Physics2D.gravity * gravityScale;
        //{ get; protected set; } = new Vector2(0, -0.32699673f);

        public static string FallingDamageHeightKey => GetPrefsKey($"{nameof(FallingDamageHeight)}");
        public static float FallingDamageHeight
        {
            get => PlayerPrefs.GetFloat(FallingDamageHeightKey, 5);
            set
            {
                if (FallingDamageHeight == value)
                    return;
                PlayerPrefs.SetFloat(FallingDamageHeightKey, value);
            }
        }
        public static string FallingDamageMultiplierKey => GetPrefsKey($"{nameof(FallingDamageMultiplier)}");
        public static float FallingDamageMultiplier
        {
            get => PlayerPrefs.GetFloat(FallingDamageMultiplierKey, 0.5f);
            set
            {
                if (FallingDamageMultiplier == value)
                    return;
                PlayerPrefs.SetFloat(FallingDamageMultiplierKey, value);
            }
        }
        #endregion

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
        [SerializeField, ReadOnly]
        private Caster _caster;
        public Caster caster => _caster;
        [SerializeField, ReadOnly]
        private Scanner2D _scanner;
        public Scanner2D scanner => _scanner;

        [SerializeField, ReadOnly]
        private Teleportable _teleportable;
        public Teleportable teleportable
        { 
            get
            { 
                if (_teleportable == null)
                {
                    _teleportable = gameObject.AddComponent<Teleportable>();
                    _teleportable.begin += _teleportable_begin;
                    _teleportable.end += _teleportable_end;
                }
                return _teleportable;
            }
        }

        private void _teleportable_end()
        {
            rigidbody.simulated = true;
        }

        private void _teleportable_begin()
        {
            rigidbody.simulated = false;
        }

        public bool IsGrounded => detector.IsGrounded;
        public bool IsInteracting => !detector.enabled;
        #endregion

        [SerializeField] protected float JumpPower = 6;
        [SerializeField] protected float MaxAirborneSpeed = 3.5f;
        [SerializeField] protected Vector2 AirborneMultiplier = new Vector2(0.1f, 0.025f);
        [SerializeField] private Vector2 _FloatingMultiplier = new Vector2(0.15f, 0.5f);
        protected Vector2 FloatingMultiplier => _FloatingMultiplier.Multiply(transform.localScale);
        [SerializeField] protected Vector2 GroundMultiplier = new Vector2(1, 1);
        [SerializeField] protected bool _Floatable;
        [SerializeField] protected float InvincibleDuration;
        

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

        public bool TriggerJump = false;
        [SerializeField,ReadOnly]
        protected Vector2 _inputMovement;
        public Vector2 InputMovement
        {
            get => _inputMovement;
            protected set
            {
                //TriggerJump = Mathf.Approximately(_inputMovement.y, 0) && value.y > 0;
                //TriggerJump = _inputMovement.y == 0 && value.y > 0;
                if (_inputMovement == value)
                    return;
                _inputMovement = value;
            }
        }
        public float BoostMultiplier { get => GroundMultiplier.x; set => GroundMultiplier.x = value; }

        public bool Floatable { get => _Floatable; set => _Floatable = value; }
        [SerializeField,ReadOnly]
        protected bool _underControl;
        public bool underControl { get => _underControl && Conscious > 0; protected set => _underControl = value; }
        public virtual bool IsActing => false;
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
        private Vector2 _defaultGravityDirection;
        public Vector2 defaultGravityDirection
        { 
            get => _defaultGravityDirection.magnitude == 0 ? Vector2.up : _defaultGravityDirection;
            protected set => _defaultGravityDirection = value;
        }

        public Vector2 overrideGravityDirection { get; set; }

        public Vector2 gravityDirection => overrideGravityDirection.magnitude == 0 ? defaultGravityDirection : overrideGravityDirection;
        public bool UseTransformUpAsGravitationOnStart;
        #endregion
        
        
        public bool IsInvincible => !attribute || !attribute.isActiveAndEnabled || InvincibleCoroutine != null;

        private float lastfallingHeight;
        //protected float lastFallingTime;
        //public float FallingTimeElapsed => Time.time - lastFallingTime;


        protected override void OnInitializing()
        {
            base.OnInitializing();
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.gravityScale = 0;

            if (!$"{name}'s Detector does not Exist ! ".printWarningIf(!this.GetComponent(ref _detector)))
                detector.GroundStateChanged += OnGroundStateChanged;
            if (!$"{name}'s Attribute does not Exist ! ".printWarningIf(!this.GetComponent(ref _attribute)))
                attribute.Init();
            if (!$"{name}'s Scanner does not Exist ! ".printWarningIf(!this.GetComponent(ref _scanner)))
                scanner.Init();
            if (!$"{name}'s Caster does not Exist ! ".printWarningIf(!this.GetComponent(ref _caster)))
                caster.Init();

            if (host)
                hostBehaviour = host.CreateBehaviour(this);

            if (UseTransformUpAsGravitationOnStart)
                defaultGravityDirection = up;

            lastfallingHeight = transform.position.y;

            rigidbody.interpolation = RigidbodyInterpolation2D.None;
        }
        protected virtual void OnGroundStateChanged(bool grounded)
        {
            if (grounded)
            {
                var fallingDistance = lastfallingHeight - detector.groundHit.point.y;
                var unbouncable = detector.groundHit.rigidbody == null || detector.groundHit.rigidbody.sharedMaterial == null || detector.groundHit.rigidbody.sharedMaterial.bounciness == 0;
                if (unbouncable && attribute && localVelocity.y < -9.55f && fallingDistance > FallingDamageHeight)
                {
                    var damage = Mathf.Floor(fallingDistance - FallingDamageHeight) * FallingDamageMultiplier;

                    // 0.5
                    damage = Mathf.Floor(damage * 2) / 2;

#if  UNITY_EDITOR
                    $"velocity.y: {localVelocity.y} || fallingDistance:{fallingDistance} || Damage:{damage}".print();
#endif
                    attribute.Affect(AttributeType.Health,AttributePropertyType.Current, AffectType.NEGATIVE, damage );
                }

                localVelocity *= Vector2.right;
                rigidbody.Sleep();
                velocity = transform.TransformVector(localVelocity);                
                //ResetRotation();
            }
        }

        protected virtual void Update()
        {
            if (!IsScriptedActing)
                UpdateInputInstruction();
        }
        protected virtual void FixedUpdate()
        {
            PerformDetection();
            ProcessMovement();
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
            if (Vector2.Dot(force.normalized,down) < 0)
                detector.LeaveGround();
        }
        protected virtual void ProcessMovement()
        {
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
            else/// deprecated using velocity control movement . this method will causing flick movement
                velocity = transform.TransformDirection(momentum);

            TriggerJump = false;
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

        public bool controllable = true;
        protected virtual bool UpdateInputInstruction()
        {
            if (GameManager.IsGamePaused || !initialized || !controllable ||
                    host == null)
            {
                if (!controllable)
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

        public virtual bool HitFrom(Vector2 v,bool push, bool faceToFrom)
        {
            if (!IsAvailable() || IsInvincible)
                return false;
            if (faceToFrom && !v.IsZero() && Vector2.Dot(v.normalized, right) > 0)
                CheckForwardFrom(-forwardSign);
            if (push)
                AddForce(v);
            return true;
        }
        #region Coroutine
        private Coroutine InvincibleCoroutine;
        public void ApplyInvincibilityFrames(Vector2 force) => ApplyInvincibilityFrames(force,InvincibleDuration);

        public void ApplyInvincibilityFrames(Vector2 force,float duration){
            if (duration <= 0)
                return;
            var stateNullable = attribute[AttributeType.Health];
            if (stateNullable == null || stateNullable.Value.IsDepleted)
                return;

            if (InvincibleCoroutine != null)
            {
                StopCoroutine(InvincibleCoroutine);
                EndInvincibility();
            }
            InvincibleCoroutine = StartCoroutine(InvincibilityFramesProcess(duration));

            //if (IsInvincible)
            //    return;
            //this.StartCoroutine(ref InvincibleCoroutine, InvincibilityFramesProcess(duration));
        }
        private IEnumerator InvincibilityFramesProcess(float duration)
        {
            BeginInvincibility();
            var renderer = GetComponent<SpriteRenderer>();
            var interval = 0.15f;
            var flickCount = Mathf.Max(1, Mathf.FloorToInt(duration / interval));
            var flashColor = new Color(.8f, .8f, .8f, 1);
            for (int i = 0; i < flickCount; i++)
            {
                renderer.color = i % 2 == 0 ? Color.white : flashColor;
                yield return new WaitForSeconds(interval);
            }
            renderer.color = Color.white;
            EndInvincibility();
            InvincibleCoroutine = null;
        }
        private void BeginInvincibility()
        {
            hurtBox.enabled = false;
            detector.collider.excludeLayers = LayerMask.GetMask("Character");
        }

        private void EndInvincibility()
        {
            detector.collider.excludeLayers = 0;
            hurtBox.enabled = true;
        }


        Coroutine performanceCoroutine;
        public bool IsScriptedActing => performanceCoroutine != null;
        public void MoveTo(Transform target)
        {
            if (!target)
                return;
            this.StartCoroutine(ref performanceCoroutine, MoveTo(target.position, target.forward.z > 0 ? 1 : -1));
        }
        private IEnumerator MoveTo(Vector2 destination,float forward)
        {
            //float x = 0;
            while (Vector2.Distance(detector.bottom, destination) > 0.5f) {
                var dir = (destination - position).normalized;
                var inputDir = dir * 0.5f;
                //if (x == transform.position.x && IsGrounded) // not moving
                //{
                //    AddForce(up * JumpPower);
                //}
                //x = transform.position.x;
                InputMovement = inputDir;
                yield return null;
            }
            InputMovement = Vector2.zero;
            CheckForwardFrom(forward);
            performanceCoroutine = null;
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (InputMovement.IsZero() || !CompareTag("Player"))
                return;
            Debug.DrawLine(position, position + (InputMovement * 5 ),Color.white);
        }
#endif
    }
}

