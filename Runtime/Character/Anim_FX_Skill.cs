using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class Anim_FX_Skill : SkillData<Anim_FX_Skill.Behaviour>
{
    public int IndexOfSkillParam;
    public string NameOfSkill;

    public Caster.Info[] castInfos;

    public bool IsValid(int index) => castInfos.IsValid(index) && !castInfos[index].source.IsEmpty();
    public class Behaviour : SkillBehaviour<Anim_FX_Skill>
    {
        private Autopilot autopilot;
        public AnimParamSMB animParam => owner.animParam;

        protected override void Init() {
        }
        protected override void OnSelect() { }
        protected override bool UpdateInput(bool down, bool hold, bool stop)
        {
            if (down && Activate())
            {
                if (data.NameOfSkill.IsEmpty())
                    animParam.IndexOfSkill = data.IndexOfSkillParam;
                else
                    animParam.IndexOfSkill = Animator.StringToHash(data.NameOfSkill);
                animParam.TriggerAction();

                return true;
            }
            return false;
        }
    }
}

