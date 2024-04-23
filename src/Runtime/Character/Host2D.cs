using System;
using UnityEngine;
using Yu5h1Lib.Game.Character;

namespace Yu5h1Lib.Game.Character
{
    public abstract class Host2D : ScriptableObject
    {
        
        public delegate void UpdateInput(bool down, bool hold, bool up);
        public abstract Vector2 GetMovement(Controller2D character);
        public abstract void GetInputState(Controller2D character, UpdateInput updateInput);
        public abstract void GetInputState(string bindingName,Controller2D character, UpdateInput updateInput);
        public abstract int ShiftIndexOfSkill(Controller2D character);
    } 
}
