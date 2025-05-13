using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Yu5h1Lib.Game.Character {
    [CreateAssetMenu(menuName = "Scriptable Objects/PlayerInput")]
    public class PlayerHost : HostData2D
    {
        public static bool UseWorldInputAxis{
            get => PlayerPrefs.GetInt("UseWorldInputAxis", 0) == 1;
            set => PlayerPrefs.SetInt("UseWorldInputAxis", value ? 1 : 0);
        }

        public override Type GetBehaviourType() => typeof(Behaviour);
        public class Behaviour : Behaviour2D<PlayerHost>
        {
            public override void Init(CharacterController2D controller) => base.Init(controller);

            public BaseInput input => GameManager.InputModule.input;

            public bool IsInteractionKeyDown => input.GetMouseButtonDown(0);
            public bool IsInteractionKey => input.GetMouseButton(0);
            public bool IsInteractionKeyUp => input.GetMouseButtonUp(0);

            private Vector2 lastInputMovement = Vector2.zero;

            public Vector2 GetMovementFromGlobalDirection(Vector2 direction)
            {
                var dir = transform.InverseTransformDirection(direction);
                if (!Body.IsFaceForward)
                    dir.x *= -1;
                return dir;
            }
            public override Vector2 GetMovement()
            {
                if (GameManager.IsSpeaking() || !Body.controllable)
                    return Vector2.zero;

                var y = input.GetAxisRaw("Vertical");
                if (y == 0)
                    y = input.GetAxisRaw("Jump");

                #region Jump
                if (y > 0)
                    Body.TriggerJump = true;
                #endregion

                var axis = new Vector2(input.GetAxisRaw("Horizontal"), y);

                if (UseWorldInputAxis)
                {
                    float angle = Vector2.SignedAngle(Vector2.up, Body.up);

                    if (angle >= -45 && angle < 45) { }
                    else if (angle >= 45 && angle < 135)
                    {
                        //left
                        //axis = new Vector2(axis.y, -axis.x);
                    }
                    else if (angle >= -135 && angle < -45)
                    {
                        //axis = new Vector2(-axis.y, axis.x);
                    }
                    else
                        axis *= -1;
                }
                lastInputMovement = axis;
                return axis;
            }
            public override bool GetInputState(UpdateInput updateInput)
            {
                if (GameManager.IsSpeaking())
                    return false;

                return updateInput(input.GetMouseButtonDown(0), input.GetMouseButton(0), input.GetMouseButtonUp(0));
            }

            public override void GetInputState(string bindingName, UpdateInput updateInput)
            {
                if (GameManager.IsSpeaking())
                    return;
                updateInput(Input.GetButtonDown(bindingName), Input.GetButton(bindingName), Input.GetButtonUp(bindingName));
            }

            #region right click change skill
            public override bool ShiftIndexOfAction(out bool next) => next = input.GetMouseButtonDown(1);
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
