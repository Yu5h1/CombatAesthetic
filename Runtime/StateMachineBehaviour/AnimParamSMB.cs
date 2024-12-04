using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Yu5h1Lib.Game.Character
{
    public class AnimParamSMB : BaseCharacterSMB
    {      
        private readonly int GroundedHash = Animator.StringToHash(nameof(Grounded));
        private readonly int UnderControlHash = Animator.StringToHash(nameof(UnderControl));
        private readonly int SpeedXHash = Animator.StringToHash(nameof(SpeedX));
        private readonly int SpeedYHash = Animator.StringToHash(nameof(SpeedY));
        private readonly int IndexOfSkillHash = Animator.StringToHash("IndexOfSkill");
        private readonly int TriggerSkillHash = Animator.StringToHash("TriggerSkill");
        private readonly int HoldSkillHash = Animator.StringToHash("HoldSkill");
        private readonly int ConsciousHash = Animator.StringToHash("Conscious");
        private readonly int HurtHash = Animator.StringToHash("Hurt");
        private readonly int TriggerExitHash = Animator.StringToHash(nameof(TriggerExit));

        #region Animator Parameters
        public bool Grounded
        {
            get => animator.GetBool(GroundedHash);
            set => animator.SetBool(GroundedHash, value);
        }
        public bool UnderControl
        {
            get => animator.GetBool(UnderControlHash);
            set => animator.SetBool(UnderControlHash, value);
        }
        public float SpeedX
        {
            get => animator.GetFloat(SpeedXHash);
            set => animator.SetFloat(SpeedXHash, value);
        }
        public float SpeedY
        {
            get => animator.GetFloat(SpeedYHash);
            set => animator.SetFloat(SpeedYHash, value);
        }
        #region Skill
        public int IndexOfSkill
        {
            get => animator.GetInteger(IndexOfSkillHash);
            set => animator.SetInteger(IndexOfSkillHash, value);
        }
        public bool HoldSkill
        {
            get => animator.GetBool(HoldSkillHash);
            set => animator.SetBool(HoldSkillHash, value);
        }
        #endregion
        public int Conscious
        {
            get => animator.GetInteger(ConsciousHash);
            set => animator.SetInteger(ConsciousHash, value);
        }

        public void TriggerSkill() => animator.SetTrigger(TriggerSkillHash);
        #endregion
        private AnimatorControllerParameter hurtParam;
        private AnimatorControllerParameter ConsciousParam;
        private AnimatorControllerParameter InteractParam;
        private AnimatorControllerParameter TriggerExitParam;
        public override void Init(AnimatorController2D controller)
        {
            base.Init(controller);
            controller.detector.GroundStateChanged += Detector_GroundStateChanged;
            TryGetAnimParam(nameof(Hurt), out hurtParam);
            TryGetAnimParam(nameof(Conscious), out ConsciousParam);
            TryGetAnimParam(nameof(TriggerExitParam), out TriggerExitParam);
        }

        private void Detector_GroundStateChanged(bool grounded) => Grounded = grounded;

        public void Hurt()
        {
            if (hurtParam == null)
                return;
            animator.SetTrigger(HurtHash);

            ///Deprecated. This May cause Parameter type 'Hash ########' does not match.
            //animator.SetTrigger(hurtParam.nameHash);
        }
        public void TriggerExit()
        {
            //if (TriggerExitParam == null)
            //    return;
            animator.SetTrigger(TriggerExitHash);
        }
        public virtual void Update()
        {
            SpeedX = Math.Abs(owner.InputMovement.x * owner.BoostMultiplier);
            if (!owner.IsGrounded)
                SpeedY = owner.localVelocity.y;
            Conscious = owner.Conscious;
            UnderControl = owner.underControl;

        }
    }
}