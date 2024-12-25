using System.Collections;
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
        private ActionSMB actionSMB;
        public bool IsActing => actionSMB?.IsActing == true;

        private StateInfo stateInfo;
        #endregion        

        #region  Skill
        [SerializeField]
        private SkillData[] _Skills;
        public SkillBehaviour[] skillBehaviours { get; private set; }

        private SkillData[] bindingskills;
        private SkillData[] _optionalSkills;
        public SkillData[] optionalSkills { get => _optionalSkills; private set => _optionalSkills = value; }
        public int indexOfSkill;

        public SkillData currentSkill => optionalSkills.IsValid(indexOfSkill) ? optionalSkills[indexOfSkill] : null;
        public SkillBehaviour currentSkillBehaviour => currentSkill == null ? null :
            skillBehaviours[_Skills.IndexOf(optionalSkills[indexOfSkill])];
        #endregion

        public float fixedPoseDirSpeed = 5;

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
            actionSMB = animator.GetBehaviour<ActionSMB>();

            if (animParam == null)
                throw new NullReferenceException("animParam(AnimParamSMB) is null");
            #endregion

            #region initinalize skill
            optionalSkills = _Skills.Where(s => s != null && s.incantation.IsEmpty()).ToArray();
            skillBehaviours = new SkillBehaviour[_Skills.Length];
            for (int i = 0; i < skillBehaviours.Length; i++)
                skillBehaviours[i] = _Skills[i].GetBehaviour(this);

            if (CompareTag("Player"))
                attribute.ui = UI_Manager.instance.PlayerAttribute_UI;
            if (!optionalSkills.IsEmpty())
                currentSkillBehaviour.Select();
            #endregion            
        }
        
        protected override void Reset()
        {
            base.Reset();
            rigidbody.freezeRotation = true;
        }
        protected override void Update()
        {
            base.Update();
            if (!IsInteracting)
                animParam?.Update();
        }
        private void OnAnimatorMove()
        {
            currentState.GetMoveInfo(out stateInfo);
            underControl = stateInfo.controllable && Conscious > 10;
        }
        protected override void ProcessMovement()
        {
            if (Time.timeScale == 0)
                return;
            if (IsInteracting)
                return;
            var gravitation = gravityDirection;
 
            var localAnimVelocity = transform.InverseTransformDirection(animator.velocity);
            localVelocity = transform.InverseTransformDirection(velocity);

            if (localVelocity.y < -5f)
                stateInfo.VelocityWeight.y = 1;

            /// momentum is based on animation velocity
            var momentum = (localVelocity * stateInfo.VelocityWeight) + (localAnimVelocity * stateInfo.rootMotionWeight);

            if (IsGrounded)
            {
                if (underControl)
                {
                    if (TriggerJump)
                    {
                        momentum.y = JumpPower;
                        detector.LeaveGround();
                    }
                    if (InputMovement.x == 0)
                    {
                        momentum.x = 0;
                        if (momentum.y < JumpPower)
                            momentum.y = 0;
                    }
                }
                if (momentum.y < JumpPower)
                {
                    RotateToGravitationSmooth(overrideGravityDirection.IsZero() ? gravitation : detector.groundHit.normal, 1);
                    if (localAnimVelocity.x != 0)
                    {
                        //always stick on ground
                        //RotateToGravitationSmooth(detector.groundHit.normal, 1);
                        //RotateToGravitationSmooth(overrideGravityDirection.IsZero() ? gravitation : detector.groundHit.normal, 1);
                        var IsVectorRight = (forwardSign * localAnimVelocity.x) > 0;
                        /// move on slop
                        var localSlopDir = transform.InverseTransformDirection(detector.CheckSlop(IsVectorRight).normalized);
                        momentum = momentum.magnitude * localSlopDir;
                        if (detector.groundHit.distance > 0)
                            momentum += new Vector2(0, -detector.groundHit.distance * momentum.magnitude);
                    }
                }
            }
            else if (Floatable)
            {
                momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * FloatingMultiplier;
                momentum = Vector2.SmoothDamp(momentum, Vector2.zero, ref floating_v_temp, 0.3f);
            }
            else
            {
                RotateToGravitationSmooth(gravitation, stateInfo.fixAngleWeight);
                ProcessingGravitation(gravitation, stateInfo.VelocityWeight, ref momentum);
            }


            #region rotate to gravityDirection
            ///remove overrideGravityDirection per update
            overrideGravityDirection = Vector2.zero;
            #endregion

            localVelocity = momentum;

            ///     fix unknow rotation 
            if (Mathf.Abs(transform.forward.z) != 1)
            {
                var euler = transform.eulerAngles;
                euler.x = 0;
                euler.y = forwardSign > 0 ? 0 : 180;
                transform.eulerAngles = euler;
            }

            if (UseCustomVelocity)
                rigidbody.MovePosition(rigidbody.position + (velocity = transform.TransformDirection(momentum) * Time.fixedDeltaTime));
            else /// deprecated using velocity control movement . this method will causing flick movement
                velocity = transform.TransformDirection(momentum);
        }
        private void ProcessingGravitation(Vector2 gravitation, Vector2 VelocityWeight, ref Vector2 momentum)
        {           
            if (VelocityWeight.IsZero())
                return;
            var localGdir = transform.InverseTransformDirection(gravitation);
            var localGQ = Quaternion.LookRotation(Vector3.forward, localGdir);
            var gMomentum = Quaternion.Inverse(localGQ) * momentum;

            if (gMomentum.y > Physics2D.gravity.y)
                momentum += (Vector2)(localGQ * (Physics2D.gravity * gravityScale));

            if (underControl && !IsInteracting && Mathf.Abs(momentum.x) < MaxAirborneSpeed)
                momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * AirborneMultiplier;
        }

        protected void RotateToGravitationSmooth(Vector2 gravitation,float fixAngleWeight,bool fade= true)
        {
            if (fixAngleWeight == 0)
                return;
            var GdirAngleGap = GetStandingAngleGap(gravitation);
            if (GdirAngleGap == 0)
                return;
            if (IsFaceForward)
                GdirAngleGap *= -1;
            if (fade && Mathf.Abs(GdirAngleGap) > 1)
                GdirAngleGap *= Time.deltaTime * fixedPoseDirSpeed;
            transform.Rotate(Vector3.forward, GdirAngleGap);
        }
        protected void RotateToGravitation(Vector2 gravitation)
        {
            var GdirAngleGap = GetStandingAngleGap(gravitation);
            if (GdirAngleGap == 0)
                return;
            if (IsFaceForward)
                GdirAngleGap *= -1;
            transform.Rotate(Vector3.forward, GdirAngleGap);
        }
        protected override void OnGroundStateChanged(bool grounded)
        {
            base.OnGroundStateChanged(grounded);
            //if (grounded)
            //    RotateToGravitation(gravityDirection);
        }
        protected override bool UpdateInputInstruction()
        {
            if (!base.UpdateInputInstruction())
                return false;
            foreach (var behaviour in skillBehaviours)
                behaviour.Update(hostBehaviour);

            if (hostBehaviour.ShiftIndexOfSkill(out bool next))
            {
                indexOfSkill = optionalSkills.ShiftIndex(indexOfSkill, next);
                currentSkillBehaviour.Select();
            }
            return true;
        }
        public override void HitFrom(Vector2 v)
        {
            if (!isActiveAndEnabled || IsInvincible)
                return;
            //face to impact Direction
            if (!v.IsZero() && Vector2.Dot(v.normalized, right) > 0)
                CheckForwardFrom(-forwardSign);
            animParam.Hurt();
            _Hited?.Invoke(v);
        }
        private void OnStatDepleted(AttributeType AttributeType)
        {
            if (AttributeType == AttributeType.Health)
            {
                Floatable = false;
                Conscious = 0;
                animParam.Hurt();
            }
        }
        #region Animation Events
        public void TriggerAction(string actionName)
        {
            animParam.IndexOfSkill = Animator.StringToHash(actionName);
            animParam.TriggerAction();
        }
        public void TriggerAction(int index)
        {
            animParam.IndexOfSkill = index;
            animParam.TriggerAction();
        }
        private void Hit()
        {
            var offsetTransform = transform.Find("HitBoxOffset") ?? transform;

            var hitboxType = detector.collider.bounds.size.magnitude > 2 ? "HitBoxBig" : "HitBox";
            var fx = PoolManager.instance.Spawn<Transform>(hitboxType, detector.front, offsetTransform.rotation);
            foreach (var mask in fx.GetComponents<EventMask2D>())
                mask.owner = transform;
        }
        public void CastFX(int index)
        {
            if (currentSkillBehaviour == null)
                return;
            if (currentSkillBehaviour.data is Anim_FX_Skill fxSkill && fxSkill.effects.IsValid(index) && !fxSkill.effects[index].IsEmpty()) {
                var offsetTransform = transform.Find("FxOffset") ?? transform;
                var fx = PoolManager.instance.Spawn<Transform>(fxSkill.effects[index], offsetTransform.position, offsetTransform.rotation);
                foreach (var mask in fx.GetComponents<EventMask2D>())
                    mask.owner = transform;
            }
        }
        public void PlayAudio(int index)
        {
            SoundManager.Play($"footstep{index}", transform.position);
        }
        #endregion

        [ContextMenu(nameof(SetPlayer))]
        public void SetPlayer()
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
                obj.tag = "Enemy";
            tag = "Player";
        }
    }
    public struct StateInfo
    {
        public bool controllable;
        public Vector2 rootMotionWeight;
        public Vector2 VelocityWeight;
        public float fixAngleWeight;
    }
}

