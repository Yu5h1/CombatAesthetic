using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game.Character;

/// <summary>
/// SMB = StateMachineBehaviour
/// </summary>
public abstract class BaseCharacterSMB : StateMachineBehaviour
{
    public Controller2D owner { get; private set; }
    public Animator animator { get; private set; }
    public AnimatorControllerParameter[] parameters => animator.parameters;
    public virtual void Init(Controller2D characterController)
    {
        owner = owner ?? characterController;
        animator = owner.animator;
    }

    protected bool TryGetAnimParam(string name,out AnimatorControllerParameter result)
    {
        result = null;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == name)
            {
                result = parameters[i];
                return true;
            }
        }
        return false;
    }
}
