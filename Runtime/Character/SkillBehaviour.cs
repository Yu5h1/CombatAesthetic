using System;

namespace Yu5h1Lib.Game.Character
{
    [Serializable]
    public abstract class SkillBehaviour {
        public SkillData data { get; protected set; }
        public AnimatorCharacterController2D owner { get; protected set; }

        public bool enable = true;
        public abstract bool IsReady { get; }
        
        protected abstract void Init();
        public void Select() => OnSelect();
        protected abstract void OnSelect();
        protected bool Activate()
        {
            if (!enable || !IsReady || !owner.underControl)
                return OnActivated(false);
            owner.attribute.Affect(AffectType.NEGATIVE, AttributePropertyType.Current, data.costs);
            return OnActivated(true);
        }
        protected virtual bool OnActivated(bool successed) => successed;
        public void Update(HostData2D.HostBehaviour2D host)
        {
            if (!enable || owner.IsInteracting || !owner.underControl )
                return;
            if (!data.parallelizable && owner.currentSkillBehaviour != this )
                return;
            if (!data.incantation.IsEmpty()) /// keybinding skill
                host.GetInputState(data.incantation, UpdateInput);
            else if (owner.currentSkillBehaviour == this) /// optinal skill
                host.GetInputState(UpdateInput);

        }
        /// <summary>
        /// return isexcuting
        /// </summary>
        /// <param name="down"></param>
        /// <param name="hold"></param>
        /// <param name="up"></param>
        protected abstract bool UpdateInput(bool down, bool hold, bool up);

        public static void Constructor(SkillData data, AnimatorCharacterController2D character,ref SkillBehaviour behaviour)
        {
            if (behaviour == null || behaviour.GetType() != data.GetBehaviourType() )
                behaviour = (SkillBehaviour)Activator.CreateInstance(data.GetBehaviourType());
            behaviour.owner = character;
            behaviour.data = data;
            behaviour.Init();
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
