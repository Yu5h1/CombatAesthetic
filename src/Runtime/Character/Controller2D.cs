using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using NullReferenceException = System.NullReferenceException;

namespace Yu5h1Lib.Game.Character
{
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(ColliderDetector2D))]
    [DisallowMultipleComponent]
    public class Controller2D : Rigidbody2DBehaviour
    {
        [SerializeField] private float GravityScale = 0.0333f;
        [SerializeField] private float JumpPower = 6;
        [SerializeField] private float MaxAirborneSpeed = 3.5f;
        [SerializeField] private Vector2 AirborneMultiplier = new Vector2(0.1f, 0.025f);
        [SerializeField] private Vector2 FloatingMultiplier = new Vector2(0.15f, 0.5f);

        #region Components 
        public Animator animator { get; private set; }
        public CapsuleCollider2D CharacterCollider { get; private set; }
        public AttributeBehaviour attribute { get; private set; }
        public ColliderDetector2D groundDetector { get; private set; }
        public bool IsGrounded => groundDetector.IsGrounded;
        #endregion

        #region Instructions
        public Host2D host;
        private bool TriggerJump = false;
        private Vector2 _Movement;
        public Vector2 InputMovement
        {
            get => _Movement;
            private set
            {
                TriggerJump = _Movement.y == 0 && value.y > 0;
                if (_Movement == value)
                    return;
                _Movement = value;
            }
        }
        public int BoostMultiplier { get; set; } = 1;
        #endregion

        #region Animator      
        public CharacterSMB[] states { get; private set; }
        private CharacterSMB _currentState;
        public CharacterSMB currentState
        {
            get => _currentState;
            set
            {
                if (_currentState == value || states.IndexOf(value) < 0)
                    return;
                _currentState = value;
            }
        }
        public AnimParamSMB animParam { get; private set; }
        [SerializeField]
        private bool _Floatable;
        public bool Floatable { get => _Floatable; set => _Floatable = value; }
        private bool _underControl;
        public bool underControl { get => _underControl && Conscious > 0; private set => _underControl = value; }
        public int Conscious { get; private set; } = 100;
        #endregion        

        #region  Skill
        [SerializeField]
        private SkillData[] _Skills;
        private SkillData[] bindingskills;

        private SkillData[] optionalSkills;
        public int indexOfSkill;

        public SkillData currentSkill => optionalSkills.Validate(indexOfSkill) ? optionalSkills[indexOfSkill] : null;
        
        public SkillBehaviour currentSkillBehaviour => currentSkill == null ? null : 
            skillBehaviours[_Skills.IndexOf(optionalSkills[indexOfSkill])];

        public SkillBehaviour[] skillBehaviours { get; private set; }
        #endregion

        #region parameters
        public float forwardSign => IsFaceForward ? 1 : -1;
        public bool IsFaceForward => transform.forward.z == 1;
        public Vector2 down => -transform.up;
        public Vector2 position => transform.position;
        #endregion

        #region flag , caches
        private Vector2 floating_v_temp = Vector2.zero;
        public bool Initinalized { get; private set; }

        public Vector2[] gravitations;
        public Vector2 gravity {
            get {
                var g = gravitations.IsEmpty() ? Vector2.down : gravitations.First();
                return (g * Physics2D.gravity.magnitude) * 0.0666666f;
            }
        }
        #endregion

        #region Event
        public event UnityAction<Vector2> Hited;
        #endregion

        public void Init()
        {
            if (Initinalized)
                return;
            animator = GetComponent<Animator>();
            CharacterCollider = GetComponent<CapsuleCollider2D>();

            if (TryGetComponent(out ColliderDetector2D colliderDetector))
            {
                groundDetector = colliderDetector;
                groundDetector.OnGroundStateChangedEvent.AddListener(OnGroundStateChanged);
            }
            if (TryGetComponent(out AttributeBehaviour attributeStat))
            {
                attribute = attributeStat;
                attribute.StatDepleted += OnStatDepleted;
            }
            if (rigidbody)
                rigidbody.gravityScale = 0;

            #region initinalize State machine behaviour
            foreach (var item in animator.GetBehaviours<BaseCharacterSMB>())
                item.Init(this);
            states = animator.GetBehaviours<CharacterSMB>();
            animParam = animator.GetBehaviour<AnimParamSMB>();
            #endregion

            #region initinalize skill
            optionalSkills = _Skills.Where(s => s.incantation.IsEmpty()).ToArray();
            skillBehaviours = new SkillBehaviour[_Skills.Length];
            for (int i = 0; i < skillBehaviours.Length; i++)
                skillBehaviours[i] = _Skills[i].GetBehaviour(this); 
            #endregion

            //End initinalization
            Initinalized = true;
        }
        protected override void Reset()
        {
            GetComponent<Rigidbody2D>().freezeRotation = true;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            Init();
        }
        protected void Update()
        {
            UpdateInputInstruction();
            animParam?.Update();
            if (Input.GetKeyDown(KeyCode.E))
                AddForce(Vector2.one * 5);
        }
        private void FixedUpdate()
        {
            groundDetector.CheckGround();
        }
        public void Hit(Vector2 strength)
        {
            Hited?.Invoke(strength);
            animParam.Hurt();
        }        
        private void OnGroundStateChanged(bool grounded)
        {
            if (grounded)
            {
                landingImpactForce = rigidbody.mass * localVelocity.y / 2;
                localVelocity *= Vector2.right;
                rigidbody.Sleep();
                velocity = transform.TransformVector(localVelocity);
            }
        }
        private void OnStatDepleted(AttributeType AttributeType)
        {
            if (AttributeType == AttributeType.Health)
            {
                Conscious = 0;
                animParam.Hurt();
            }
        }
        public float landingImpactForce { get; private set; }
        public bool UseCustomVelocity;
        private Vector2 _velocity;
        public Vector2 velocity 
        {
            get => UseCustomVelocity ? _velocity : rigidbody.velocity;
            private set {
                if (UseCustomVelocity)
                    _velocity = value;
                else
                    rigidbody.velocity = value;
            }
        }
        public Vector2 localVelocity { get; private set; }
        public bool debug;
        void OnAnimatorMove()
        {
            if (Time.timeScale == 0)
                return;
            currentState.GetMoveInfo(out bool controllable, out Vector2 rootMotionWeight, out Vector2 VelocityWeight);
            underControl = controllable && Conscious > 10;
            var localAnimVelocity = transform.InverseTransformDirection(animator.velocity);
            localVelocity = transform.InverseTransformDirection(velocity);
            var momentum = (localVelocity * VelocityWeight) + (localAnimVelocity * rootMotionWeight);

            if (IsGrounded)
            {
                if (underControl)
                {
                    if (TriggerJump)
                    {
                        momentum.y = JumpPower;
                        groundDetector.LeaveGround();
                    }
                    if (InputMovement.x == 0 )
                    {
                        momentum.x = 0;
                        if (momentum.y < JumpPower)
                            momentum.y = 0;
                    }
                }
                if (momentum.y < JumpPower)
                {
                    /// fix bouncing while moving on slop
                    var localSlopDir = transform.InverseTransformDirection(groundDetector.CheckSlop(IsFaceForward).normalized);
                    momentum = momentum.magnitude * localSlopDir;
                    if (groundDetector.groundHit.distance > 0)
                        momentum += new Vector2(0, -groundDetector.groundHit.distance * momentum.magnitude);
                }
            }
            else if (Floatable)
            {
                momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * FloatingMultiplier;
                momentum = Vector2.SmoothDamp(momentum, Vector2.zero, ref floating_v_temp, 0.3f);
            }
            else
            {
                if (VelocityWeight.magnitude != 0)
                {
                    if (momentum.y > Physics2D.gravity.y * 2f)
                        momentum += Physics2D.gravity * GravityScale;
                    if (Mathf.Abs(momentum.x) < MaxAirborneSpeed)
                        momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * AirborneMultiplier;
                }
            }
            localVelocity = momentum;

            if (Force != default(Vector2))
                Force.print();

            if (UseCustomVelocity)
                rigidbody.MovePosition(rigidbody.position + ((velocity = Force + (Vector2)transform.TransformDirection(momentum)) * Time.fixedDeltaTime));
            else /// deprecated using velocity control movement . this method will causing flick movement
                velocity = Force + (Vector2)transform.TransformDirection(momentum);
            Force = Vector2.zero;
        }
        private Vector2 Force;
        [ContextMenu("AddForce")]
        public void AddForce()
        {
            AddForce(Vector2.one * 5);
        }
        public void AddForce(Vector2 force) {
            velocity += force;
            groundDetector.LeaveGround();
        } 
        
        #region Custom Functions
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

        public void UpdateInputInstruction()
        {
            if (GameManager.IsGamePause)
                return;
            if (host == null)
            {
                InputMovement = Vector2.zero;
                return;
            }
            InputMovement = host.GetMovement(this);
            foreach (var behaviour in skillBehaviours)
                behaviour.Update(host);
            if (host.ShiftIndexOfSkill(this, out bool next))
                indexOfSkill = optionalSkills.ShiftIndex(indexOfSkill, next);
        }
        #endregion
        #region Animation Events
        public void CastFX(int index)
        {
            if (!currentSkill)
                return;
            if (currentSkill is Anim_FX_Skill fxSkill && fxSkill.effects.Validate(index) && !fxSkill.effects[index].IsEmpty()) {
                var offsetTransform = transform.Find("FxOffset") ?? transform;
                var fx = PoolManager.instance.Spawn<Transform>(fxSkill.effects[index], offsetTransform.position, offsetTransform.rotation);
                foreach (var mask in fx.GetComponents<EventMask2D>())
                {
                    mask.tagOption.tag = gameObject.tag;
                    mask.tagOption.type = TagOption.ComparisionType.NotEqual;
                }
            }
        }
        public void PlayAudio(int index)
        {
            var clip = Resources.Load<AudioClip>($"Sound/footstep{index}");
            var clipName = clip == null ? $"footstep{index}.mp3 not found" : clip.name;
            $"PlayAudio : {clipName}".print();
            if (clip)
            {
                GameManager.instance.PlayAudio(clip);
            }
        }
        #endregion
    }
}

