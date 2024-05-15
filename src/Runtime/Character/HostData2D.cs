using System;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public abstract class HostData2D : ScriptableObject
    {
        public abstract class HostBehaviour2D
        {
            public Controller2D owner;
            public delegate void UpdateInput(bool down, bool hold, bool up);
            public virtual void Init(Controller2D controller) => owner = controller;
            public abstract Vector2 GetMovement();
            public abstract void GetInputState(UpdateInput updateInput);
            public abstract void GetInputState(string bindingName, UpdateInput updateInput);
            public abstract bool ShiftIndexOfSkill(out bool next);
        }
        public abstract class Behaviour2D<T> : HostBehaviour2D where T : HostData2D
        {
            public T data => (T)owner.host;
        }
        public abstract Type GetHostType();
        public HostBehaviour2D Create(Controller2D controller) {
            var result = (HostBehaviour2D)Activator.CreateInstance(GetHostType());
            result.Init(controller);
            return result;
        }
    } 
}
