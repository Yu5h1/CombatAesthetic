using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public class Autopilot : HostData2D
    {
        public override Type GetHostType() => typeof(Behaviour);

        public class Behaviour : Behaviour2D<Autopilot>
        {
            public Patrol patrol;

            private Transform transform => owner.transform;
            public override void Init(Controller2D controller) 
            {
                base.Init(controller);
                if (owner.TryGetComponent(out patrol))
                    patrol.Reset();
            }
            public bool NeedTurnAround() {
                if (patrol.RangeDistance < owner.detector.extents.x)
                    return false;
                var p = owner.transform.InverseTransformPoint(patrol.point);
                if (Mathf.Abs(p.x) > patrol.RangeDistance - owner.detector.extents.x && p.x < 0) 
                    return true;
                return false;
            }

            public override Vector2 GetMovement()
            {
                var movement = Vector2.zero;
                if ((patrol && owner.IsGrounded ) || (patrol && owner.Floatable ))
                {
                    movement = owner.InputMovement;
                    if (owner.InputMovement.x == 0)
                        movement.x = owner.transform.localScale.x;

                    if (owner.detector.scanner.collider)
                    {
                        if (owner.detector.scanner.Scan(out Collider2D collider))
                        {
                            if (collider.TryGetComponent(out Controller2D target))
                            {
                                var dir = GetDirection(owner, target.transform);
                                if (dir.magnitude > 2)
                                {
                                    if (owner.detector.CheckCliff())
                                        return Vector2.zero;
                                    else
                                        return dir.x > 0 ? Vector2.left : Vector2.right;
                                }
                                return Vector2.zero;
                            }
                            else
                                return movement * Vector2.left;
                        }
                    }
                    else
                    {

                    }
                    if (owner.detector.CheckObstacle() || owner.detector.CheckCliff() || NeedTurnAround())
                        movement.x = -movement.x;
                }
                return movement;
            }
            public override void GetInputState( UpdateInput updateInput)
            {
                updateInput(false, false, false);
            }
            public override void GetInputState(string bindingName,UpdateInput updateInput)
            {
                throw new System.NotImplementedException();
            }

            public override bool ShiftIndexOfSkill(out bool next)
                => next = false;


            public bool OutOfRange(Controller2D self, Transform target)
            {
                if (target == null)
                    return false;
                if (Vector2.Distance(self.transform.position, target.position) < self.detector.collider.bounds.size.magnitude * 2)
                    return false;
                else
                    return true;
            }
            public Vector2 GetDirection(Controller2D self, Transform target)
                => self.position - (Vector2)target.transform.position;

        }
    }
}
