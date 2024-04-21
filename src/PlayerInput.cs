using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.EventSystems.StandaloneInputModule;
using static Yu5h1Lib.GameManager.IDispatcher;

namespace Yu5h1Lib.Game.Character {
    [CreateAssetMenu(menuName = "Scriptable Objects/PlayerInput")]
    public class PlayerInput : Host2D
    {
        public override Vector2 GetMovement(Controller2D controller)
        {
            return new Vector2(input.GetAxisRaw("Horizontal"), input.GetAxisRaw("Vertical"));
        }
        public override void GetInputState(Controller2D character, out bool down, out bool hold, out bool up)
        {
            down = input.GetMouseButtonDown(0);
            hold = input.GetMouseButton(0);
            up = input.GetMouseButtonUp(0);
        }
        public override void GetInputState(string key,Controller2D character, out bool down, out bool hold, out bool up)
        {
            down = input.GetButtonDown(key);
            hold = input.GetMouseButton(0);
            up = input.GetMouseButtonUp(0);
        }
        public override int ShiftIndexOfSkill(Controller2D character)
        {
            if (input.TryGetScrollWheelDelta(out float delta))
                return (int)Mathf.Sign(delta);
            return 0;
        } 
    }
}
