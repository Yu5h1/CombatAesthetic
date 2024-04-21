using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Yu5h1Lib.Game.Character;

/// <summary>
/// SMB = StateMachineBehaviour
/// </summary>
public abstract class BaseCharacterSMB : StateMachineBehaviour
{
    public Controller2D owner { get; private set; }
    public virtual void Init(Controller2D characterController)
    {
        owner = owner ?? characterController;
    }
}
