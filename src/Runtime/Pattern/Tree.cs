using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITree
{
    public interface IBranch
    {
        IBranch parent { get; set; }
        List<IBranch> children { get; set; }
    }
    IBranch root { get; set; }
}
