using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib.Game.Character;

/// <summary>
/// SMB = StateMachineBehaviour
/// </summary>
public abstract class BaseCharacterSMB : StateMachineBehaviour
{
    public AnimatorCharacterController2D owner { get; private set; }
    public Animator animator { get; private set; }
    public Dictionary<string, AnimatorControllerParameter> Parameters;


    public virtual void Init(AnimatorCharacterController2D characterController)
    {
        owner = owner ?? characterController;
        animator = owner.animator;
        Parameters = animator.parameters.ToDictionary(p => p.name, p => p);
        
    }

    protected bool TryGetAnimParam(string name,out AnimatorControllerParameter result)
        => Parameters.TryGetValue(name, out result);
    public bool DoesParamExist(string name) => Parameters.ContainsKey(name);
    public bool DoesParamsExist(params string[] name) => name.All(n=> DoesParamExist(n));
}
