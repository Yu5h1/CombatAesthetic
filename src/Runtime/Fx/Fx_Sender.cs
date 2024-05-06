using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Fx_Sender : MonoBehaviour
{
    public abstract void Perform(Collider2D target);
}
