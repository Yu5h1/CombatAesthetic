using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib.Game.Character;

/// <summary>
/// SMB = StateMachineBehaviour
/// </summary>
public abstract class BaseCharacterSMB : StateMachineBehaviour
{
    public Controller2D owner { get; private set; }
    public Animator animator { get; private set; }
    public Dictionary<string, AnimatorControllerParameter> Parameters;


    public virtual void Init(Controller2D characterController)
    {
        owner = owner ?? characterController;
        animator = owner.animator;
        Parameters = animator.parameters.ToDictionary(p => p.name, p => p);
        
    }

    protected bool TryGetAnimParam(string name,out AnimatorControllerParameter result)
        => Parameters.TryGetValue(name, out result);
    public bool DoesParamExists(string name) => Parameters.ContainsKey(name);
    public bool DoesParamsExists(params string[] name) => name.All(n=> DoesParamExists(n));
}
