using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Yu5h1Lib.Game.Character {
    [CreateAssetMenu(menuName = "Scriptable Objects/PlayerInput")]
    public class PlayerHost : HostData2D
    {
        public override Type GetBehaviourType() => typeof(Behaviour);
        public class Behaviour : Behaviour2D<PlayerHost>
        {
            public override void Init(Controller2D controller) => base.Init(controller);

            public BaseInput input => GameManager.InputModule.input;

            public bool IsInteractionKeyDown => input.GetMouseButtonDown(0);
            public bool IsInteractionKey => input.GetMouseButton(0);
            public bool IsInteractionKeyUp => input.GetMouseButtonUp(0);

            public override Vector2 GetMovement()
            {
                if (GameManager.IsSpeaking)
                    return Vector2.zero;
                return new Vector2(input.GetAxisRaw("Horizontal"), Mathf.Max( input.GetAxisRaw("Vertical"), input.GetAxisRaw("Jump")));
            }
            public override bool GetInputState(UpdateInput updateInput)
            {
                if (GameManager.IsSpeaking)
                    return false;

                return updateInput(input.GetMouseButtonDown(0), input.GetMouseButton(0), input.GetMouseButtonUp(0));
            }

            public override void GetInputState(string bindingName, UpdateInput updateInput)
            {
                if (GameManager.IsSpeaking)
                    return;
                updateInput(Input.GetButtonDown(bindingName), Input.GetButton(bindingName), Input.GetButtonUp(bindingName));
            }

            #region right click change skill
            public override bool ShiftIndexOfSkill(out bool next) => next = input.GetMouseButtonDown(1);
            #endregion

            #region Mouse Wheel change skill
            //public override bool ShiftIndexOfSkill(out bool next)
            //{
            //    next = false;
            //    if (input.TryGetScrollWheelDelta(out float delta))
            //    {
            //        next = Mathf.Sign(delta) > 0;
            //        return true;
            //    }
            //    return false;
            //} 
            #endregion
        }
    }
}
