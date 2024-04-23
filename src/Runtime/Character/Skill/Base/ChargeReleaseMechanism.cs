using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib.Game.Character;


public class ChargeReleaseMechanism : SkillStateMachine<ChargeReleaseMechanism.Behaviour>
{
    public int IndexOfSkillParam;
    public string NameOfSkill;
    public class Behaviour : Behaviour<ChargeReleaseMechanism> 
    {        
        public AnimParamSMB animParam => owner.animParam;
        public override bool IsReady => base.IsReady && animParam.DoesParamExists("HoldSkill");
        protected override void Init()
        {
            
        }
        protected override void OnEnter()
        {
            if (data.NameOfSkill.IsEmpty())
                animParam.IndexOfSkillParam = data.IndexOfSkillParam;
            else
                animParam.IndexOfSkillParam = Animator.StringToHash(data.NameOfSkill);
            owner.animator.SetBool("HoldSkill", true);
            animParam.TriggerSkill();
        }
        protected override void OnExcute() {}
        protected override void OnExit(ExitReason reason)
        {
            owner.animator.SetBool("HoldSkill", false);
        }
    }
}
