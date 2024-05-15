using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using NullReferenceException = System.NullReferenceException;

namespace Yu5h1Lib.Game.Character
{
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class AnimatorController2D : Controller2D
    {
        #region Animator     
        public Animator animator { get; private set; }
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

        #region Event
        public event UnityAction<Vector2> Hited;
        #endregion

        protected override void Init()
        {
            base.Init();
            if (attribute)
                attribute.StatDepleted += OnStatDepleted;

            animator = GetComponent<Animator>();
            #region initinalize State machine behaviour
            foreach (var item in animator.GetBehaviours<BaseCharacterSMB>())
                item.Init(this);
            states = animator.GetBehaviours<CharacterSMB>();
            animParam = animator.GetBehaviour<AnimParamSMB>();
            #endregion

            #region initinalize skill
            optionalSkills = _Skills.Where(s => s != null && s.incantation.IsEmpty()).ToArray();
            skillBehaviours = new SkillBehaviour[_Skills.Length];
            for (int i = 0; i < skillBehaviours.Length; i++)
                skillBehaviours[i] = _Skills[i].GetBehaviour(this); 
            #endregion

        }
        protected override void Reset()
        {
            base.Reset();
            rigidbody.freezeRotation = true;
        }
        protected override void Update()
        {
            UpdateInputInstruction();
            if (!IsInteracting)
                animParam?.Update();
        }
        public void Hit(Vector2 strength)
        {
            Hited?.Invoke(strength);
            animParam.Hurt();
        }
        protected override void OnGroundStateChanged(bool grounded)
        {
            if (grounded)
                animParam.SpeedY = rigidbody.mass * localVelocity.y / 2;
            base.OnGroundStateChanged(grounded);
        }
        private void OnStatDepleted(AttributeType AttributeType)
        {
            if (AttributeType == AttributeType.Health)
            {
                Conscious = 0;
                animParam.Hurt();
            }
        }
        protected override void FixedUpdate()
        {
            detector.CheckGround();
        }
        void OnAnimatorMove()
        {
            if (Time.timeScale == 0)
                return;
            if (IsInteracting)
            {
                return;
            }
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
                        detector.LeaveGround();
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
                if (VelocityWeight.magnitude != 0)
                {
                    if (momentum.y > Physics2D.gravity.y * 2f)
                        momentum += Physics2D.gravity * GravityScale;
                    if (Mathf.Abs(momentum.x) < MaxAirborneSpeed && !IsInteracting)
                        momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * AirborneMultiplier;
                }
            }
            localVelocity = momentum;

            if (UseCustomVelocity)
                rigidbody.MovePosition(rigidbody.position + (velocity = transform.TransformDirection(momentum) * Time.fixedDeltaTime));
            else /// deprecated using velocity control movement . this method will causing flick movement
                velocity = transform.TransformDirection(momentum);
        }
        protected override bool UpdateInputInstruction()
        {
            if (!base.UpdateInputInstruction() || IsInteracting) 
                return false;
            foreach (var behaviour in skillBehaviours)
                behaviour.Update(hostBehaviour);
            if (hostBehaviour.ShiftIndexOfSkill(out bool next))
                indexOfSkill = optionalSkills.ShiftIndex(indexOfSkill, next);
            return true;
        }

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
            if (ResourcesEx.TryLoad($"Sound/footstep{index}", out AudioClip clip))
                GameManager.instance.PlayAudio(clip);
            else {
                var clipName = clip == null ? $"footstep{index}.mp3 not found" : clip.name;
                $"PlayAudio : {clipName} does not Exists.".LogWarning();
            }
        }
        #endregion
    }
}

