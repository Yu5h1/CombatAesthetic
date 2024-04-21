using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public abstract class SkillStateMachine<B> : SkillData<B> where B : SkillStateMachine<B>.Behaviour<SkillStateMachine<B>>
    {
        public EnergyInfo[] ExcutingCosts;

        public abstract class Behaviour<TData> : SkillBehaviour<TData> where TData : SkillStateMachine<B>
        {
            public bool IsExecuting { get; private set; }

            protected virtual void Enter()
            {
                IsExecuting = true;
            }
            protected virtual void Excute()
            {


            }
            protected virtual void Exit()
            {
                IsExecuting = false;
            }
            protected virtual void Release()
            {
                Exit();
            }
            protected virtual void Interrupt()
            {
                Exit();
            }
        }
    }

}
