using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;


public class ChargeReleaseMechanism : SkillStateMachine<ChargeReleaseMechanism.Behaviour>
{
    public int IndexOfSkillParam;
    public string NameOfSkill;
    public class Behaviour : Behaviour<ChargeReleaseMechanism> 
    {        
        public AnimParamSMB animParam => owner.animParam;
        public override bool IsReady => base.IsReady && animParam.DoesParamExist("HoldSkill");

        protected override void Init() { }
        protected override void OnSelect() { }

        protected override void OnEnter()
        {
            if (data.NameOfSkill.IsEmpty())
                animParam.IndexOfSkill = data.IndexOfSkillParam;
            else
                animParam.IndexOfSkill = Animator.StringToHash(data.NameOfSkill);

            owner.animParam.HoldSkill = true;
            animParam.TriggerAction();
        }
        protected override void OnExcute() {}
        protected override void OnExit(ExitReason reason)
        {
            owner.animParam.HoldSkill = false;
        }
    }
}
