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
        private readonly int GroundedHash = Animator.StringToHash("Grounded");
        private readonly int SpeedXHash = Animator.StringToHash("SpeedX");
        private readonly int SpeedYHash = Animator.StringToHash("SpeedY");
        private readonly int IndexOfSkillHash = Animator.StringToHash("IndexOfSkill");
        private readonly int TriggerSkillHash = Animator.StringToHash("TriggerSkill");
        private readonly int ConsciousHash = Animator.StringToHash("Conscious");
        private readonly int HurtHash = Animator.StringToHash("Hurt");
        #region Animator Parameters
        public bool Grounded
        {
            get => animator.GetBool(GroundedHash);
            set => animator.SetBool(GroundedHash, value);
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
        public int IndexOfSkill
        {
            get => animator.GetInteger(IndexOfSkillHash);
            set => animator.SetInteger(IndexOfSkillHash, value);
        }
        public int Conscious
        {
            get => animator.GetInteger(ConsciousHash);
            set => animator.SetInteger(ConsciousHash, value);
        }

        public void TriggerSkill() => animator.SetTrigger(TriggerSkillHash);
        #endregion
        private AnimatorControllerParameter hurtParam;
        private AnimatorControllerParameter ConsciousParam;
        public override void Init(Controller2D controller)
        {
            base.Init(controller);  
            controller.detector.OnGroundStateChangedEvent.AddListener(val => Grounded = val);
            TryGetAnimParam("Hurt", out hurtParam);
            TryGetAnimParam(nameof(Conscious), out hurtParam);
        }
        public void Hurt()
        {
            if (hurtParam == null)
                return;
            ///May cause Parameter type 'Hash ########' does not match.
            //animator.SetTrigger(hurtParam.nameHash);
            animator.SetTrigger(HurtHash);
        }

        public virtual void Update()
        {
            SpeedX = Math.Abs(owner.InputMovement.x * owner.BoostMultiplier);
            SpeedY = owner.IsGrounded ? owner.landingImpactForce : owner.localVelocity.y;
            Conscious = owner.Conscious;

        }
    }
}