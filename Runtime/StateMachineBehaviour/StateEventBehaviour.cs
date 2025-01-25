using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class StateEventBehaviour : StateMachineBehaviour
{
    private AnimatorEventHandler EventHandler;
    private int lastLoopCount = 0;
    public void Init(AnimatorEventHandler eventHandler)
    {
        EventHandler = eventHandler;
    }
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //var clipinfos = animator.GetCurrentAnimatorClipInfo(layerIndex);
        //if (clipinfos.IsEmpty())
        //    return;
        //var maxWeightClip = clipinfos[0];
        //foreach (var clipinfo in animator.GetCurrentAnimatorClipInfo(0))
        //{
        //    if (clipinfo.weight > maxWeightClip.weight)
        //        maxWeightClip = clipinfo;
        //}


        int currentLoopCount = Mathf.FloorToInt(stateInfo.normalizedTime);

        if (currentLoopCount > lastLoopCount)
        {
            OnAnimationLoop(animator);
            lastLoopCount = currentLoopCount;
        }
    }
    private void OnAnimationLoop(Animator animator)
    {
        
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
