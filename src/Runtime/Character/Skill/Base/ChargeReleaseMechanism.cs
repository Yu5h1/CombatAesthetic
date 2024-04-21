using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game.Character;


public class ChargeReleaseMechanism : SkillStateMachine<ChargeReleaseMechanism.Behaviour>
{
    public class Behaviour : Behaviour<SkillStateMachine<Behaviour>>
    {
        protected override void Init()
        {
           
        }

        protected override void OnUpdate(bool down, bool hold, bool stop)
        {
            if (!IsReady && !IsExecuting)
                return;
            if (IsExecuting)
            {
                if (!owner.underControl)
                    Interrupt();
                else if (hold)
                    Excute();
                else
                    Release();
            }
            else if (IsReady && down)
                Enter();
        }
    }
}
