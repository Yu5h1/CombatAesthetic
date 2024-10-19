using System;
using UnityEngine;
using Yu5h1Lib.Game.Character;


public class AdvancedAnimParamSMB : AnimParamSMB
{
    #region Animator Parameters
    public bool CrouchingParam
    {
        get => animator.GetBool("Crouching");
        set => animator.SetBool("Crouching", value);
    }
    #endregion
    public override void Update()
    {
        base.Update();
        //if (character.currentSkill)
        //NextSkillIndexParam = ;
        //if (character.NextSkillIndex >= 0)
        //{
        //    Debug.Log("character.NextSkillIndex");



        //    //animator.SetTrigger("UseSkill");
        //    //character.Floatable = !character.Floatable;
        //    //if (!character.Floatable)
        //    //    character.rigidbody.gravityScale = 3;
        //}
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
