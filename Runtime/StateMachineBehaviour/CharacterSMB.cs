using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Yu5h1Lib.Game.Character;

public class CharacterSMB : BaseCharacterSMB
{
    public bool Controllable = true;
    public Vector2 rootMotionWeight = Vector2.one;
    public Vector2 rigidbodyVelocityWeight = Vector2.one;
    public ProcessStep CheckForwardType = ProcessStep.Excute;

    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!owner)
            return;
        owner.currentState = this;
        if (CheckForwardType == ProcessStep.Enter)
            owner.CheckForward();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!owner)
            return;
        if (CheckForwardType == ProcessStep.Excute)
            owner.CheckForward();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!owner)
            return;
        if (CheckForwardType == ProcessStep.Exit)
            owner.CheckForward();
        if (owner.currentState == this)
            owner.currentState = null;
    }

    //OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
        // Implement code that processes and affects root motion
    //}

    //OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
        // Implement code that sets up animation IK (inverse kinematics)
    //}
}
