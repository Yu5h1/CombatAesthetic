using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSMB : BaseCharacterSMB
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        $"{owner.name} OnStateExit".Log();
    }
}
