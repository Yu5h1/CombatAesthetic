using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib.Game.Character;

/// <summary>
/// SMB = StateMachineBehaviour
/// </summary>
public abstract class StateMachineAdvanced : StateMachineBehaviour
{
    public string StateName;
    public Animator animator { get; private set; }
    public Dictionary<string, AnimatorControllerParameter> Parameters;

    public virtual void Init()
    {
        Parameters = animator.parameters.ToDictionary(p => p.name, p => p);
        animator.keepAnimatorStateOnDisable = true;
    }


}
