using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Fx_Receiver<T> : MonoBehaviourEnhance where T : Fx_Sender
{
    public T sender { get; protected set; }
    public abstract void Perform(T sender);
}
