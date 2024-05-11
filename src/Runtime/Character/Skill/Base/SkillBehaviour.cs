using System;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public abstract class SkillBehaviour {
        public Controller2D owner { get; protected set; }
        public SkillData data { get; protected set; }
        public abstract bool IsReady { get; }

        protected abstract void Init();
        protected bool Activate()
        {
            if (!IsReady || !owner.underControl)
                return false;
            owner.attribute.Affect(AffectType.NEGATIVE, data.costs);
            return true;
        }
        public void Update(Host2D host)
        {
            if (!data.parallelizable && owner.currentSkillBehaviour != this)
                return;
            if (!data.incantation.IsEmpty()) /// keybinding skill
                host.GetInputState(data.incantation, owner, UpdateInput);
            else if (owner.currentSkillBehaviour == this) /// optinal skill
                host.GetInputState(owner, UpdateInput);

        }
        protected abstract void UpdateInput(bool down, bool hold, bool up);

        public static SkillBehaviour Constructor(SkillData skill, Controller2D character)
        {
            var result = (SkillBehaviour)Activator.CreateInstance(skill.GetBehaviourType());
            result.owner = character;
            result.data = skill;
            result.Init();
            return result;
        }
        protected static bool Validate(SkillData data) => false;
    }
    public abstract class SkillBehaviour<Data> : SkillBehaviour where Data : SkillData
    {
        public new Data data => (Data)base.data;
        public override bool IsReady => owner.attribute.Validate(base.data.preCalculatedCost);
        protected SkillBehaviour() {}
    }

}
