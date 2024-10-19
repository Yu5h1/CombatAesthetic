using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : ScriptableObject, ITree
{
    public enum Result
    {
        Failure,
        Success,
        Continue,
        Abort
    }
    public ITree.IBranch root { get; set; }
}
