using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.StandaloneInputModule;

namespace Yu5h1Lib.Game.Character {
    [CreateAssetMenu(menuName = "Scriptable Objects/PlayerInput")]
    public class PlayerHost : Host2D
    {
        public BaseInput input => GameManager.InputModule.input;
        public override Vector2 GetMovement(Controller2D controller)
        {
            return new Vector2(input.GetAxisRaw("Horizontal"), input.GetAxisRaw("Vertical"));
        }
        public override void GetInputState(Controller2D character, UpdateInput updateInput)
        {
            updateInput(input.GetMouseButtonDown(0), input.GetMouseButton(0), input.GetMouseButtonUp(0));
        }

        public override void GetInputState(string bindingName, Controller2D character, UpdateInput updateInput)
        {
            updateInput(Input.GetButtonDown(bindingName), Input.GetButton(bindingName), Input.GetButtonUp(bindingName));
        }
        public override bool ShiftIndexOfSkill(Controller2D character,out bool next)
        {
            next = false;
            if (input.TryGetScrollWheelDelta(out float delta))
            {
                next = Mathf.Sign(delta) > 0;
                return true;
            }
            return false;
        }

 
    }
}
