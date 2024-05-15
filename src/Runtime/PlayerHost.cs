using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Yu5h1Lib.Game.Character {
    [CreateAssetMenu(menuName = "Scriptable Objects/PlayerInput")]
    public class PlayerHost : HostData2D
    {
        public override Type GetHostType() => typeof(Behaviour);

        public class Behaviour : Behaviour2D<PlayerHost>
        {
            public override void Init(Controller2D controller) => base.Init(controller);

            public BaseInput input => GameManager.InputModule.input;

            public bool IsInteractionKeyDown => input.GetMouseButtonDown(0);
            public override Vector2 GetMovement()
            {
                if (GameManager.IsSpeaking)
                    return Vector2.zero;
                return new Vector2(input.GetAxisRaw("Horizontal"), input.GetAxisRaw("Vertical"));
            }
            public override void GetInputState(UpdateInput updateInput)
            {
                if (GameManager.IsSpeaking)
                    return;
                updateInput(input.GetMouseButtonDown(0), input.GetMouseButton(0), input.GetMouseButtonUp(0));
            }

            public override void GetInputState(string bindingName, UpdateInput updateInput)
            {
                if (GameManager.IsSpeaking)
                    return;
                updateInput(Input.GetButtonDown(bindingName), Input.GetButton(bindingName), Input.GetButtonUp(bindingName));
            }
            public override bool ShiftIndexOfSkill(out bool next)
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
}
