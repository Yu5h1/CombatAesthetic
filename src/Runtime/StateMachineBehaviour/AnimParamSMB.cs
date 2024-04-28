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
        #region Animator Parameters
        public bool Grounded
        {
            get => animator.GetBool(GroundedHash);
            set => animator.SetBool(GroundedHash, value);
        }
        public float SpeedXParam
        {
            get => animator.GetFloat(SpeedXHash);
            set => animator.SetFloat(SpeedXHash, value);
        }
        public float SpeedYParam
        {
            get => animator.GetFloat(SpeedYHash);
            set => animator.SetFloat(SpeedYHash, value);
        }
        public int IndexOfSkillParam
        {
            get => animator.GetInteger(IndexOfSkillHash);
            set => animator.SetInteger(IndexOfSkillHash, value);
        }
        public void TriggerSkill() => animator.SetTrigger(TriggerSkillHash);
        #endregion
        AnimatorControllerParameter hurt;
        public override void Init(Controller2D controller)
        {
            base.Init(controller);  
            controller.detector.OnGroundStateChangedEvent.AddListener(val => Grounded = val);
            if (TryGetAnimParam("Hurt",out hurt) ) {
                controller.Hited += Controller_Hited;
            }

        }
        private void Controller_Hited(Vector2 strength)
            => animator.SetTrigger(hurt.nameHash);

        public virtual void Update()
        {
            SpeedXParam = Math.Abs(owner.InputMovement.x * owner.BoostMultiplier);
            SpeedYParam = owner.IsGrounded ? owner.landingImpactForce : owner.localVelocity.y;
        }
    }
}