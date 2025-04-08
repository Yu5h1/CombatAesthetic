using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using NullReferenceException = System.NullReferenceException;

namespace Yu5h1Lib.Game.Character
{
    [RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class AnimatorCharacterController2D : CharacterController2D
    {
        #region Animator     
        private Animator _animator;
        public Animator animator { get => _animator; private set => _animator = value; }

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
        public override bool IsActing => actionSMB?.IsActing == true;

        private StateInfo stateInfo;
        #endregion        

        #region  Skill
        [SerializeField]
        private SkillData[] _Skills;
        public SkillBehaviour[] skillBehaviours { get; private set; }

        private SkillData[] bindingskills;
        private SkillData[] _optionalSkills;
        public SkillData[] optionalSkills 
        { 
            get 
            {
                if (_optionalSkills.IsEmpty() && !_Skills.IsEmpty())
                    _optionalSkills = _Skills.Where(s => s != null && s.incantation.IsEmpty()).ToArray();
                return _optionalSkills;
            }
        }
        
        public int indexOfSkill;

        public SkillData currentSkill => optionalSkills.IsValid(indexOfSkill) ? optionalSkills[indexOfSkill] : null;

        public SkillBehaviour currentSkillBehaviour => currentSkill == null ? null :
            skillBehaviours[_Skills.IndexOf(optionalSkills[indexOfSkill])];

        #endregion

        public float fixedPoseDirSpeed = 5;

        protected override void OnInitializing()
        {
            base.OnInitializing();
            if (attribute)
                attribute.StatDepleted += OnStatDepleted;

            this.GetComponent(ref _animator);
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

            #region initialize State machine behaviour
            foreach (var item in animator.GetBehaviours<BaseCharacterSMB>())
                item.Init(this);
            states = animator.GetBehaviours<CharacterSMB>();
            animParam = animator.GetBehaviour<AnimParamSMB>();
            actionSMB = animator.GetBehaviour<ActionSMB>();

            if (animParam == null)
                throw new NullReferenceException("animParam(AnimParamSMB) is null");
            #endregion

            #region initialize skill
            
            skillBehaviours = new SkillBehaviour[_Skills.Length];
            for (int i = 0; i < skillBehaviours.Length; i++)
                skillBehaviours[i] = _Skills[i].CreateBehaviour(this);

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
            if (IsInteracting)
                return;
            animParam?.Update();
        }
        //public override void PauseStateChange(bool paused)
        //{
        //    base.PauseStateChange(paused);
        //    rigidbody.sleepMode = paused ? RigidbodySleepMode2D.StartAsleep : RigidbodySleepMode2D.StartAwake;
            
        //    //if (animator)
        //    //{
        //    //    rigidbody.isKinematic = paused;
        //    //    animator.speed = paused ? 0 : 1;
        //    //}

        //}
        //protected override void FixedUpdate()
        //{
        //    if (Time.timeScale == 0)
        //        return;
        //    PerformDetection();
        //}
        private void OnAnimatorMove()
        {
            currentState.GetStateInfo(out stateInfo);
            underControl = stateInfo.controllable && Conscious > 10;
            //ProcessMovement();
        }
        protected override void ProcessMovement()
        {
            if (IsInteracting)
                return;
            var gravitation = gravityDirection;

            var localAnimVelocity = transform.InverseTransformDirection(animator.velocity);
            localVelocity = transform.InverseTransformDirection(velocity);

            if (!Floatable && localVelocity.y < -5f )
                stateInfo.VelocityWeight.y = 1;

            #region momentum is based on animation velocity

            var animVelocity = localAnimVelocity * stateInfo.rootMotionWeight;

            if (stateInfo.affectByMultiplier)
               animVelocity *= Floatable ? FloatingMultiplier : IsGrounded ? GroundMultiplier : AirborneMultiplier;

            var momentum = (localVelocity * stateInfo.VelocityWeight) + animVelocity; 

            #endregion

            if (IsGrounded)
            {
                if (underControl)
                {
                    if (JumpPower > 0 && TriggerJump )
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

            TriggerJump = false;
        }
        private void ProcessingGravitation(Vector2 gravitation, Vector2 VelocityWeight, ref Vector2 momentum)
        {           
            if (VelocityWeight.IsZero())
                return;
            var localGdir = transform.InverseTransformDirection(gravitation);
            var localGQ = Quaternion.LookRotation(Vector3.forward, localGdir);
            var gMomentum = Quaternion.Inverse(localGQ) * momentum;

            if (gMomentum.y > Physics2D.gravity.y)
                momentum += (Vector2)(localGQ * scaledGravity);

            if (underControl && !IsInteracting && Mathf.Abs(momentum.x) < MaxAirborneSpeed)
                momentum += new Vector2(Mathf.Abs(InputMovement.x), InputMovement.y) * AirborneMultiplier;
        }

        protected void RotateToGravitationSmooth(Vector2 gravitation,float fixAngleWeight,bool fade = true)
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
        public void RandomCurrentSkill(int primaryIndex = -1)
        {
            var enabledSkills = skillBehaviours.Where(b => b.enable && b.data.incantation.IsEmpty()).ToArray();
            if (enabledSkills.Length == 1)
                return;
            if (primaryIndex < 0)
                indexOfSkill = optionalSkills.IndexOf(enabledSkills.RandomElement().data);
            else
                indexOfSkill = Random.value < 0.5f ? optionalSkills.IndexOf(enabledSkills.RandomElement().data) : primaryIndex;
        }
        public override bool HitFrom(Vector2 v, bool push, bool faceToFrom)
        {
            if (!base.HitFrom(v, push, faceToFrom))
                return false;
            animParam.Hurt();
            _Hited?.Invoke(v);
            return true;
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
        #region FX
        public void CastHitBox(string offsetName)
        {
            var t = transform.Find(offsetName) ?? transform;
            var hitBox = caster.Retrieve("HitBox", t.position, t.rotation,true);
            //var origiParent = hitBox.parent;
            if (t.TryGetComponent(out Collider2D info))
            {
                foreach (var col in hitBox.GetComponents<Collider2D>())
                    col.enabled = false;
                switch (info)
                {
                    case BoxCollider2D box:
                        if (hitBox.TryGetComponent(out BoxCollider2D hitbox))
                        {
                            hitbox.enabled = true;
                            hitbox.offset = box.offset;
                            hitbox.size = box.size;
                        }
                        break;
                    case CircleCollider2D circle:
                        if (hitBox.TryGetComponent(out CircleCollider2D hitCircle))
                        {
                            hitCircle.enabled = true;
                            hitCircle.offset = circle.offset;
                            hitCircle.radius = circle.radius;
                        }
                        break;
                }
            }
        }
        public void CastFxOnTransform(Transform offsetT)
        {
            if (currentSkillBehaviour == null)
                return;
            if (currentSkillBehaviour.data is Anim_FX_Skill fxSkill && !fxSkill.castInfos.IsEmpty())
            {
                if (scanner.target)
                {
                    for (int i = 0; i < fxSkill.castInfos.Length; i++)
                        SpawnSkillFX(fxSkill, i, offsetT);
                }
                else
                    SpawnSkillFX(fxSkill, 0, offsetT);
            }
        }
        public void SpawnSkillFX(Anim_FX_Skill skill, int index, Transform offsetT)
        {
            if ($"{name}'s skill Fx [{index}] is empty.".printWarningIf(skill.castInfos[index].source.IsEmpty()))
                return;
            offsetT ??= transform;
            var info = skill.castInfos[index];
            var pos = offsetT.position;
            var rot = offsetT.rotation;
            caster.Cast(info, pos, rot);
        }
        public void CastFxOnOffset(string offsetName)
        {
            if ($"{name} caster does not exists".printWarningIf(!caster))
                return;
            CastFxOnTransform(transform.Find(offsetName) ?? transform);
        }        
        public void CastFX() => CastFxOnOffset("FxOffset");

        public void CastFXByIndex(int index)
        {
            if (currentSkillBehaviour == null)
                return;
            if (currentSkillBehaviour.data is Anim_FX_Skill fxSkill && fxSkill.IsValid(index))
                SpawnSkillFX(fxSkill, index, transform.Find("FxOffset"));
        }
        public void CastFxWithAnimEventData(AnimatorStateEventData data)
        {
            if (animator.GetDominantLayer() != data.layer)
                return;
            if (data.HitBox)
                CastHitBox(data.offsetTransformName);
            else
                CastFxOnOffset(data.offsetTransformName);
            
        }
     
        #endregion
        public void PlayAudioClips(AudioClips clips)
        {
            if ($"{name} tring to play Empty AudioClips".printErrorIf(!clips))
                return;
            PlayAudioClip(clips.RandomElement());
        }
        public void PlayAudioClip(AudioClip clip)
        {
            if ($"{name} tring to play Null AudioClip".printErrorIf(!clip))
                return;
          

            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(animator.GetDominantLayer());
            foreach (var info in clipInfos)
            {
                if (clip.name.Compare(info.clip.name,StringComparisonStyle.StartsWith) && info.weight >= 0.9f)
                {
                    AudioManager.Play(clip, transform.position);
                    break;
                }
            }

        }
    


        public void MoveLayer(int offset)
            => SetLayer(Mathf.Clamp(animator.GetDominantLayer() + offset, 0, animator.layerCount - 1));
        [ContextMenu(nameof(MoveNextLayer))]
        public void MoveNextLayer() => MoveLayer(1);
        [ContextMenu(nameof(MoveLastLayer))]
        public void MoveLastLayer() => MoveLayer(-1);
        public void SetLayer(int layer)
        {
            if (animator.layerCount < 2)
                return;
            animator.Play("Intro", layer);
            for (int i = 0; i < animator.layerCount; i++)
                animator.SetLayerWeight(i, i == layer ? 1 : 0);
        }
        [ContextMenu(nameof(RestartAnimator))]
        public void RestartAnimator()
        {
          
        }
        #endregion



        [ContextMenu("Set as Player")]
        public void SetAsPlayer()
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
                obj.tag = "Enemy";
            tag = "Player";
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            //if (currentSkill == null)
            //    return;


            //if (currentSkill.distance <= 0)
            //    return;
            //var color = Gizmos.color;
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawWireSphere(transform.position, currentSkill.distance);
            //Gizmos.color = color;
        }

#endif
    }
    public struct StateInfo
    {
        public bool controllable;
        public Vector2 rootMotionWeight;        
        public Vector2 VelocityWeight;
        public bool affectByMultiplier;
        public float fixAngleWeight;

        public static readonly StateInfo Default = new StateInfo()
        {
            controllable = false,
            rootMotionWeight = Vector2.zero,
            VelocityWeight = Vector2.one,
            fixAngleWeight = 1f,
            affectByMultiplier = false
        };
    }
}

