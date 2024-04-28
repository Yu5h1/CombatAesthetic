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
        [SerializeField] private float JumpPower = 10;
        [SerializeField] private float MaxAirborneSpeed = 8;
        [SerializeField] private Vector2 AirborneMultiplier = new Vector2(0.2f, 0.05f);
        [SerializeField] private Vector2 FloatingMultiplier = new Vector2(0.3f, 1f);

        #region Components 
        public Animator animator { get; private set; }
        public CapsuleCollider2D CharacterCollider { get; private set; }
        public AttributeStatBehaviour statBehaviour { get; private set; }
        public ColliderDetector2D detector { get; private set; }
        public bool IsGrounded => detector.IsGrounded;
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
        private bool _UnderControl;
        public bool underControl { get => _UnderControl; private set => _UnderControl = value; }
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
            detector = GetComponent<ColliderDetector2D>();
            statBehaviour = GetComponent<AttributeStatBehaviour>();
            if (rigidbody)
                rigidbody.gravityScale = 0;

            #region State machine behaviour
            foreach (var item in animator.GetBehaviours<BaseCharacterSMB>())
                item.Init(this);
            states = animator.GetBehaviours<CharacterSMB>();
            animParam = animator.GetBehaviour<AnimParamSMB>();
            #endregion

            detector.OnGroundStateChangedEvent.AddListener(OnGroundStateChanged);


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
        }
        private void FixedUpdate()
        {
            detector.CheckGroundState();
        }
        private void LateUpdate()
        {
            detector.CheckPlatformStandHeight();
        }
        public void Hit(Vector2 strength)
        {
            Debug.Log($"{name} has been Hited !");
            Hited?.Invoke(strength);
        }
        private void OnGroundStateChanged(bool grounded)
        {
            if (grounded)
            {
                landingImpactForce = rigidbody.mass * localVelocity.y / 2;
                localVelocity *= Vector2.right;
                velocity = transform.TransformVector(localVelocity);
            }
        }
        public float landingImpactForce { get; private set; }
        public Vector2 velocity { get; private set; }
        public Vector2 localVelocity { get; private set; }
        public bool debug;
        void OnAnimatorMove()
        {
            if (Time.timeScale == 0)
                return;
            currentState.GetMoveInfo(out _UnderControl, out Vector2 rootMotionWeight, out Vector2 VelocityWeight);

            var localAnimVelocity = transform.InverseTransformDirection(animator.velocity);
            localVelocity = transform.InverseTransformDirection(velocity);

            var momentum = (localVelocity * VelocityWeight) + (localAnimVelocity * rootMotionWeight);

            if (IsGrounded)
            {
                if (underControl)
                {
                    if (TriggerJump)
                        momentum.y = JumpPower;
                    if (InputMovement.x == 0)
                    {
                        momentum.x = 0;
                    }
                    //else if (momentum.y < JumpPower)
                    //{
                    //    /// fix bouncing while moving on slop
                    //    if (detector.groundHit.distance > 0)
                    //        momentum += new Vector2(0, -detector.groundHit.distance).normalized;
                    //    momentum = momentum.magnitude * detector.CheckSlop2D(IsFaceForward).normalized;
                    //}
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
                    if (momentum.y > Physics2D.gravity.y)
                        momentum += Physics2D.gravity * 0.0666666f;
                    //if (localmomentum.magnitude < Physics2D.gravity.magnitude && gravity.normalized.IsSameDirectionAs(down))
                    //    momentum += gravity;
                    //momentum += Physics2D.gravity * 0.0666666f;
                    //momentum += InputMovement * AirborneMultiplier;
                    //momentum.x = IsFaceForward ? Mathf.Min(MaxAirborneSpeed, momentum.x) : Mathf.Max(-MaxAirborneSpeed, momentum.x);
                }
            }
            localVelocity = momentum;
            rigidbody.MovePosition(rigidbody.position + ((velocity = transform.TransformDirection(momentum)) * Time.fixedDeltaTime));
            if (debug)
                Debug.Log($"velocity: {velocity} momentum:{momentum}");
            /// deprecated using velocity control movement . this method will causing flick movement
            //rigidbody.velocity = momentum; 
        }
        #region Custom Functions
        public void CheckForward(float x)
        {
            if (x == 0)
                return;
            if (Mathf.Sign(x) == forwardSign)
                return;
//180 rotation will cause collider error
//Re-enabling force the collider returns to normal
            //detector.collider.enabled = false;
            transform.Rotate(Vector3.up, 180, Space.Self);
            //detector.collider.enabled = true;
            //transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, -forwardSign), transform.up);

        }
        public void CheckForward() => CheckForward(InputMovement.x);

        public void UpdateInputInstruction()
        {
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
                    mask.IgnoreTag = gameObject.tag;
            }
        }
        #endregion
    }
}

