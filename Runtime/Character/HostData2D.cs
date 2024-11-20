using System;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public abstract class HostData2D : ScriptableObject
    {
        public abstract class HostBehaviour2D
        {
            public Controller2D Body;
            public delegate bool UpdateInput(bool down, bool hold, bool up);
            public virtual void Init(Controller2D body) => Body = body;
            public abstract Vector2 GetMovement();
            public abstract bool GetInputState(UpdateInput updateInput);
            public abstract void GetInputState(string bindingName, UpdateInput updateInput);
            public abstract bool ShiftIndexOfSkill(out bool next);
        }
        public abstract class Behaviour2D<T> : HostBehaviour2D where T : HostData2D
        {
            public T data => (T)Body.host;
        }
        public abstract Type GetBehaviourType();
        public HostBehaviour2D CreateBehaviour(Controller2D controller) {
            var result = (HostBehaviour2D)Activator.CreateInstance(GetBehaviourType());
            result.Init(controller);
            return result;
        }
    } 
}
