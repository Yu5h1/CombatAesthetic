using System;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public abstract class SkillBehaviour {
        public Controller2D owner { get; protected set; }
        public Host2D host => owner.host;       
        public SkillData data { get; protected set; }
        public abstract bool IsReady { get; }
        protected bool Activate()
        {
            if (!IsReady || !owner.underControl)
                return false;
            owner.statBehaviour.Affect(AffectType.NEGATIVE, data.costs);
            return true;
        }
        public void update()
        {
            if (!owner.host)
                throw new MissingReferenceException("Missing host");

            if (owner.currentSkillBehaviour != this)
                return;

            if (!data.incantation.IsEmpty()) /// keybinding skill
                host.GetInputState(data.incantation, owner, OnUpdate);
            else if (owner.currentSkillBehaviour == this) /// optinal skill
                host.GetInputState(owner, OnUpdate);

        }
        protected abstract void Init();
        protected abstract void OnUpdate(bool down, bool hold, bool up);
        public void SpawnFx(string fx)
        {
            //if (Fx.IsEmpty() || !Fx.Validate(index) || !ResourcesEx.TryLoad(Fx[index], out GameObject source))
            //    return;
            //var fx = GameObject.Instantiate(source);
        }

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
        public override bool IsReady => owner.statBehaviour.Validate(base.data.preCalculatedCost);
        protected SkillBehaviour() {}
    }

}
