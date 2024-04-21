using System;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public abstract class SkillBehaviour {
        public Controller2D owner { get; protected set; }
        public Host2D host => owner.host;       
        public SkillData _Data { get; protected set; }
        public abstract bool IsReady { get; }
        protected bool Activate()
        {
            if (!IsReady || !owner.underControl)
                return false;
            owner.statBehaviour.Affect(AffectType.NEGATIVE, _Data.costs);
            return true;
        }
        public void update()
        {
            if (!owner.host)
            {
                throw new MissingReferenceException("Missing host");
            }
            bool down, hold, up;
            if (!_Data.incantation.IsEmpty()) /// keybinding skill
                host.GetInputState(_Data.incantation, owner, out down, out hold, out up);
            else if (owner.currentSkillBehaviour == this) /// optinal skill
                host.GetInputState(owner, out down, out hold, out up);
            else
                return;
            OnUpdate(down, hold, up);
        }
        protected abstract void Init();
        protected abstract void OnUpdate(bool down, bool hold, bool stop);
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
            result._Data = skill;
            result.Init();
            return result;
        }
        protected static bool Validate(SkillData data) => false;
    }
    public abstract class SkillBehaviour<Data> : SkillBehaviour where Data : SkillData
    {
        public Data data => (Data)_Data;
        public override bool IsReady => owner.statBehaviour.Validate(_Data.preCalculatedCost);
        protected SkillBehaviour() {}
    }

}
