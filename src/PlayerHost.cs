using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.EventSystems.StandaloneInputModule;
using static Yu5h1Lib.GameManager.IDispatcher;

namespace Yu5h1Lib.Game.Character {
    [CreateAssetMenu(menuName = "Scriptable Objects/PlayerInput")]
    public class PlayerHost : Host2D
    {
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
            throw new System.NotImplementedException();
            //=> updateInput(false, false, false);
        }
        public override int ShiftIndexOfSkill(Controller2D character)
        {
            if (input.TryGetScrollWheelDelta(out float delta))
                return (int)Mathf.Sign(delta);
            return 0;
        }

 
    }
}
