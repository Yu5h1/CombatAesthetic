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
            if (IsExecuting)
            {

            }
        }
    }
}

