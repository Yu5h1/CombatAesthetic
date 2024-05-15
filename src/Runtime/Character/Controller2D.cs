using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Yu5h1Lib.Game.Character
{
    [DisallowMultipleComponent,RequireComponent(typeof(ColliderDetector2D))]
    public class Controller2D : Rigidbody2DBehaviour
    {
        [SerializeField] protected float GravityScale = 0.0333f;
        [SerializeField] protected float JumpPower = 6;
        [SerializeField] protected float MaxAirborneSpeed = 3.5f;
        [SerializeField] protected Vector2 AirborneMultiplier = new Vector2(0.1f, 0.025f);
        [SerializeField] protected Vector2 FloatingMultiplier = new Vector2(0.15f, 0.5f);
        [SerializeField] protected Vector2 GroundMultiplier = new Vector2(1, 1);
        [SerializeField] protected bool _Floatable;

        #region Components   
        private AttributeBehaviour _attribute;
        public AttributeBehaviour attribute => _attribute;
        public ColliderDetector2D detector { get; private set; }
        public bool IsGrounded => detector.IsGrounded;
        public bool IsInteracting => !detector.enabled;
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
                hostBehaviour = host.Create(this);
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

        public bool Floatable { get => _Floatable; set => _Floatable = value; }
        protected bool _underControl;
        public bool underControl { get => _underControl && Conscious > 0; protected set => _underControl = value; }
        public int Conscious { get; protected set; } = 100;
        #endregion

        #region operating parameters
        public float forwardSign => IsFaceForward ? 1 : -1;
        public bool IsFaceForward => transform.forward.z == 1;

        public Vector2 position => transform.position;
        public Vector2 down => -transform.up;
        

        protected Vector2 floating_v_temp = Vector2.zero;
        public Vector2[] gravitations = new Vector2[] { Vector2.down };
        public Vector2 gravity
        {
            get
            {
                var g = gravitations.IsEmpty() ? Vector2.down : gravitations.First();
                return g.normalized * Physics2D.gravity.magnitude * GravityScale;
            }
        }

        #endregion

        #region Movement
        public bool UseCustomVelocity;
        private Vector2 _velocity;
        public Vector2 velocity
        {
            get => UseCustomVelocity ? _velocity : rigidbody.velocity;
            set
            {
                if (UseCustomVelocity)
                    _velocity = value;
                else
                    rigidbody.velocity = value;
            }
        }
        public Vector2 localVelocity { get; protected set; }
        #endregion
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
            if (TryGetComponent(out ColliderDetector2D colliderDetector))
            {
                detector = colliderDetector;
                detector.OnGroundStateChangedEvent.AddListener(OnGroundStateChanged);
            }
            TryGetComponent(out _attribute);

            if (rigidbody)
                rigidbody.gravityScale = 0;

            if (host)
                hostBehaviour = host.Create(this);
        }
        protected virtual void OnGroundStateChanged(bool grounded)
        {
            if (grounded)
            {
                localVelocity *= Vector2.right;
                rigidbody.Sleep();
                velocity = transform.TransformVector(localVelocity);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Init();
        }
        protected virtual void Update()
        {
            UpdateInputInstruction();
        }
        protected virtual void FixedUpdate()
        {
            detector.CheckGround();
            OnMove();
        }
        [ContextMenu("AddForce Test")]
        public void AddForce()
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
                if (momentum.y > Physics2D.gravity.y * 2f)
                    momentum += Physics2D.gravity * GravityScale;
                if (Mathf.Abs(momentum.x) < MaxAirborneSpeed)
                    momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * AirborneMultiplier;
            }
            localVelocity = momentum;

            if (UseCustomVelocity)
                rigidbody.MovePosition(rigidbody.position + (velocity = transform.TransformDirection(momentum) * Time.fixedDeltaTime));
            else /// deprecated using velocity control movement . this method will causing flick movement
                velocity = transform.TransformDirection(momentum);
        }

        public void CheckForward(float x)
        {
            if (x == 0)
                return;
            if (Mathf.Sign(x) == forwardSign)
                return;
            transform.Rotate(Vector3.up, 180, Space.Self);
            // Alternative
            //transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -forwardSign), transform.up);

        }
        public void CheckForward() => CheckForward(InputMovement.x);

        protected virtual bool UpdateInputInstruction()
        {
            if (GameManager.IsGamePause)
                return false;
            if (host == null)
            {
                InputMovement = Vector2.zero;
                return false;
            }
            InputMovement = hostBehaviour.GetMovement();
            return true;
        }
    }
}

