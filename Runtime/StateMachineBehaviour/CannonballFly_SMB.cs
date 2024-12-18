using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballFly_SMB : CharacterSMB
{
    public float leaveStateThreshold = 0;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        owner.detector.enabled = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var v = owner.velocity;
        if (v == Vector2.zero)
            return;
        var vdir = owner.velocity.normalized;
        var dirDelta = Vector2.Dot(Vector2.up, vdir);
        if (dirDelta > leaveStateThreshold)
            owner.transform.rotation = Quaternion.LookRotation(owner.transform.forward, vdir);
        else
            owner.animParam.TriggerExit();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        owner.ResetRotation();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
