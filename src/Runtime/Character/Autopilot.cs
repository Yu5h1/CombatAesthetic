using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public class Autopilot : Host2D
    {
        public bool patrol;
        public override Vector2 GetMovement(Controller2D character)
        {
            var nextMovement = Vector2.zero;
            if (patrol)
            {
                nextMovement = character.InputMovement;
                if (character.InputMovement.x == 0)
                    nextMovement.x = character.transform.localScale.x;
                if (character.colliderDetector.CheckEdge())
                    nextMovement.x = -nextMovement.x;
            }
            return nextMovement;
        }
        public override void GetInputState(Controller2D character, out bool down, out bool hold, out bool up)
        {
            down = hold = up = false;
        }

        public override int ShiftIndexOfSkill(Controller2D character)
            => 0;

        public override void GetInputState(string bindingName, Controller2D character, out bool down, out bool hold, out bool up)
        {
            down = hold = up = false;
        }
    }
}
