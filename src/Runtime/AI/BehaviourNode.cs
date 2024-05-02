using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourNode : ScriptableObject
{
	public abstract class Behaviour
	{
        /// <param name="result">result of Success or Failure</param>
        /// <returns>get decision return Continue/Abort</returns>
        public abstract bool Excute(out bool result);
	}
    public BehaviourBranch branchs;
    public void Expand(BehaviourBranch nodes)
    {
        branchs.AddRange(nodes);
    }
}
