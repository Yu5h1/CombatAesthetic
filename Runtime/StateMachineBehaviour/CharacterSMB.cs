using UnityEngine;
using Yu5h1Lib.Runtime;

public class CharacterSMB : BaseCharacterSMB
{
    
    [SerializeField]
    private bool _Controllable = true;    
    private bool IsInControlInterval { get; set; }
    public bool Controllable => _Controllable && IsInControlInterval;

    [SerializeField,Range(0f,1f)]
    private float ResumControllableTime = 0;

    public Vector2 rootMotionWeight = Vector2.one;
    public Vector2 rigidbodyVelocityWeight = Vector2.one;

    public bool affectByMultiplier;

    public float FixAngleWeight = 1;
    public ProcessStep CheckForwardType = ProcessStep.Excute;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!owner || animator.GetDominantLayer() != layerIndex)
            return;        
        owner.currentState = this;
        if (CheckForwardType == ProcessStep.Enter)
            owner.CheckForward();
        IsInControlInterval = stateInfo.normalizedTime >= ResumControllableTime;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!owner)
            return;
        if (CheckForwardType == ProcessStep.Excute)
            owner.CheckForward();
        IsInControlInterval = stateInfo.normalizedTime >= ResumControllableTime;
        
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
