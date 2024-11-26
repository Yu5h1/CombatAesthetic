using UnityEngine;

/// <summary>
/// CSMB = Character State Machine Behaviour
/// </summary>
public class SMB_Physics : BaseCharacterSMB
{
    public bool ChangeLayer;
    public string layerName;
    private int lastlayer;
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        lastlayer = owner.gameObject.layer;
        if (ChangeLayer)
            owner.gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (ChangeLayer)
            owner.gameObject.layer = lastlayer;
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
