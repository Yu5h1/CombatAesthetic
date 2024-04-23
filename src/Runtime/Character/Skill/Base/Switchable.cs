using Yu5h1Lib.Game.Character;

public class Switchable : SkillStateMachine<Switchable.Behaviour>
{
    public class Behaviour : Behaviour<SkillStateMachine<Behaviour>>
    {
        protected override void Init()
        {
        }
        protected override void OnUpdate(bool down, bool hold, bool stop)
        {
            base.OnUpdate(down, hold, stop);
        }
        protected override void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnExcute()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnExit(ExitReason reason)
        {
            throw new System.NotImplementedException();
        }
    }
}

