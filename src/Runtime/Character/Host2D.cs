using UnityEngine;
using Yu5h1Lib.Game.Character;

namespace Yu5h1Lib.Game.Character
{
    public abstract class Host2D : ScriptableObject
    {
        public abstract Vector2 GetMovement(Controller2D character);
        public abstract void GetInputState(Controller2D character, out bool down, out bool hold, out bool up);
        public abstract void GetInputState(string bindingName,Controller2D character,out bool down, out bool hold, out bool up);
        public abstract int ShiftIndexOfSkill(Controller2D character);
    } 
}
