using System.Collections.Generic;
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
        //public const float DefaultGravityScale = 3;
        [SerializeField] private float JumpPower = 10;
        [SerializeField] private float MaxAirborneSpeed = 8;
        [SerializeField] private Vector2 AirborneMultiplier = new Vector2(0.2f, 0.05f);
        [SerializeField] private Vector2 FloatingMultiplier = new Vector2(0.3f, 1f);

        #region Components 
        public Animator animator { get; private set; }
        public CapsuleCollider2D CharacterCollider { get; private set; }
        public AttributeStatBehaviour statBehaviour { get; private set; }
        public ColliderDetector2D colliderDetector { get; private set; }
        public bool IsGrounded => colliderDetector.IsGrounded;
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
        public IEnumerable<SkillData> Skills => _Skills;
        public int indexOfSkill;
        public SkillData currentSkill => _Skills.Validate(indexOfSkill) ? _Skills[indexOfSkill] : null;
        public bool TryGetSkill(int index, out SkillData result)
            => (result = _Skills.Validate(index) ? _Skills[index] : null) != null;
        public SkillBehaviour[] skillBehaviours { get; private set; }
        public SkillBehaviour currentSkillBehaviour => currentSkill == null ? null : skillBehaviours[indexOfSkill];

        public void ShiftIndexOfSilll(int val)
        {
            if (val == 0)
                return;
            indexOfSkill += val;
            if (indexOfSkill >= _Skills.Length)
            {
                indexOfSkill = 0;
            }
            else if (indexOfSkill < 0)
                indexOfSkill = _Skills.Length - 1;
        }
        #endregion

        #region parameters
        public float forward2D => IsFaceForward ? 1 : -1;
        public bool IsFaceForward => transform.forward == Vector3.forward;
        #endregion

        #region flag , caches
        private Vector2 floating_v_temp = Vector2.zero;
        public bool Initinalized { get; private set; }
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
            colliderDetector = GetComponent<ColliderDetector2D>();
            statBehaviour = GetComponent<AttributeStatBehaviour>();

            #region State machine behaviour
            foreach (var item in animator.GetBehaviours<BaseCharacterSMB>())
                item.Init(this);
            states = animator.GetBehaviours<CharacterSMB>();
            animParam = animator.GetBehaviour<AnimParamSMB>();
            #endregion

            colliderDetector.OnGroundStateChangedEvent.AddListener(OnGroundStateChanged);
            skillBehaviours = new SkillBehaviour[_Skills.Length];
            for (int i = 0; i < skillBehaviours.Length; i++)
                skillBehaviours[i] = _Skills[i].GetBehaviour(this);

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
        public void Hit(Vector2 strength)
        {
            
            Hited?.Invoke(strength);
        }
        private void OnGroundStateChanged(bool val)
        {
            //rigidbody.gravityScale = Floatable || IsGrounded ? 0 : DefaultGravityScale;
            if (val)
            {
                landingImpactForce = rigidbody.mass * velocity.y / 2;
                velocity *= Vector2.right;// clear gravity
            }
        }
        public float landingImpactForce { get; private set; }
        public Vector2 velocity { get; private set; }
        void OnAnimatorMove()
        {
            if (Time.timeScale == 0)
                return;
            currentState.GetMoveInfo(out _UnderControl, out Vector2 rootMotionWeight, out Vector2 VelocityWeight);

           var momentum = (velocity * VelocityWeight) + (animator.velocity * rootMotionWeight);

            if (IsGrounded)
            {
                if (underControl)
                {
                    if (TriggerJump)
                        momentum.y = JumpPower;
                    if (InputMovement.x == 0)
                        momentum.x = 0;
                }
            }
            else if (Floatable)
            {
                momentum += InputMovement * FloatingMultiplier;
                momentum = Vector2.SmoothDamp(momentum, Vector2.zero, ref floating_v_temp, 0.3f);
            }
            else
            {
                if (VelocityWeight.magnitude != 0)
                {
                    momentum += Physics2D.gravity * 0.0666666f;
                    momentum += InputMovement * AirborneMultiplier;
                    momentum.x = IsFaceForward ? Mathf.Min(MaxAirborneSpeed, momentum.x) : Mathf.Max(-MaxAirborneSpeed, momentum.x);
                }
            }
            //custom velocity
            rigidbody.MovePosition(rigidbody.position + ((velocity = momentum) * Time.fixedDeltaTime));
            /// flick bug
            //rigidbody.velocity = momentum; 
        }
        #region Custom Functions
        public void CheckForward(float x)
        {
            if (x == 0)
                return;
            if (Mathf.Sign(x) == forward2D)
                return;
            transform.forward *= -1;
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
                behaviour.update();
            ShiftIndexOfSilll(host.ShiftIndexOfSkill(this));
        }
        #endregion
        #region Animation Events

        public void CastFX(int index)
        {
            if (!currentSkill)
                return;
            if (currentSkill is Anim_FX_Skill fxSkill && fxSkill.effects.Validate(index) && !fxSkill.effects[index].IsEmpty()) {
                var offsetTransform = transform.Find("FxOffset") ?? transform;
                var fx = PoolManager.instance.Spawn<Transform>(fxSkill.effects[index], offsetTransform.position, offsetTransform.forward);
                foreach (var mask in fx.GetComponents<EventMask2D>())
                    mask.IgnoreTag = gameObject.tag;
            }
        }
        #endregion
    }
}

