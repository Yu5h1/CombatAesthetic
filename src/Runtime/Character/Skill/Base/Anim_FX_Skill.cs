using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game.Character;

public class Anim_FX_Skill : SkillData<Anim_FX_Skill.Behaviour>
{
    public int IndexOfSkillParam;
    public string NameOfSkill;

    public string[] effects;
    public class Behaviour : SkillBehaviour<Anim_FX_Skill>
    {
        public AnimParamSMB animParam => owner.animParam;
   
        protected override void Init()
        {
            
        }
        protected override void OnUpdate(bool down, bool hold, bool stop)
        {
            if (down && Activate() )
            {
                if (data.NameOfSkill.IsEmpty())
                    animParam.IndexOfSkillParam = data.IndexOfSkillParam;
                else
                    animParam.IndexOfSkillParam = Animator.StringToHash(data.NameOfSkill);
                animParam.TriggerSkill();
            }
        }
    }

}

