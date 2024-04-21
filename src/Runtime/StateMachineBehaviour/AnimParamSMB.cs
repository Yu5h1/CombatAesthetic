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
        public Animator animator { get; private set; }        
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
        //public Dictionary<string,AnimatorControllerParameter> AnimParams { get; private set; }
        public override void Init(Controller2D character)
        {
            base.Init(character);
            animator = character.animator;
            character.colliderDetector.OnGroundStateChangedEvent.AddListener(val => Grounded = val);
            //AnimParams = animator.parameters.ToDictionary(p => p.name, p => p);
        }
        public virtual void Update()
        {
            SpeedXParam = Math.Abs(owner.InputMovement.x * owner.BoostMultiplier);
            SpeedYParam = owner.IsGrounded ? owner.landingImpactForce : owner.velocity.y;

            //NextSkillIndexParam = character.indexOfSkill;
            //if ( >= 0)
            //{
            //    //character.NextSkill
            //    //animator.SetTrigger("UseSkill");
            //    character.Floatable = !character.Floatable;
            //    if (!character.Floatable)
            //        character.rigidbody.gravityScale = 3;
            //}
            //SpeedXParam = character.Movement.x == 0 ? 0 : (Input.GetKey(BurstModeKey) ? 2 : 1);

            //CrouchingParam = Input.GetButton("Crouch");
            //SpeedYParam = character.IsGrounded ? 0 : character.rigidbody.velocity.y;
        }



        // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
        //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called before OnStateExit is called on any state inside this state machine
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called before OnStateMove is called on any state inside this state machine
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateIK is called before OnStateIK is called on any state inside this state machine
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMachineEnter is called when entering a state machine via its Entry Node
        //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
        //{
        //    
        //}

        // OnStateMachineExit is called when exiting a state machine via its Exit Node
        //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
        //{
        //    
        //}
    }
}