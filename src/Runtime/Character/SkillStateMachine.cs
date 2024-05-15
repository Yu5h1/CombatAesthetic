using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public abstract class SkillStateMachine<T> : SkillData<T> where T : SkillBehaviour
    {
        public EnergyInfo[] ExcutingCosts;
        public abstract class Behaviour<TData> : SkillBehaviour<TData> where TData : SkillStateMachine<T>
        {
            public enum ExitReason
            {
                none = 0,
                release = 1,
                interrupt = 2
            }
            public bool IsExecuting { get; private set; }

            protected virtual bool keepholding
                => ConsumeExecutionCosts() == AttributeType.None;

            protected virtual AttributeType ConsumeExecutionCosts()
                => owner.attribute.Affect(AffectType.NEGATIVE, data.ExcutingCosts);

            protected override void UpdateInput(bool down, bool hold, bool release)
            {
                if (!IsReady && !IsExecuting)
                    return;
                if (IsExecuting)
                {
                    if (hold)
                        OnExcute();
                    if (!owner.underControl)
                        Exit(ExitReason.interrupt);
                    if (release || !keepholding)
                        Exit(ExitReason.release);
                }                
                else if (down && Activate())
                {
                    IsExecuting = true;
                    OnEnter();
                }
            }

            protected abstract void OnEnter();
            protected abstract void OnExcute();
            private void Exit(ExitReason reason)
            {
                IsExecuting = false;
                OnExit(reason);
            }
            protected abstract void OnExit(ExitReason reason);
        }
    }

}
