using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public class Autopilot : Host2D
    {
        public bool patrol;
        public override Vector2 GetMovement(Controller2D self)
        {
            var movement = Vector2.zero;
            if (patrol)
            {
                movement = self.InputMovement;
                if (self.InputMovement.x == 0)
                    movement.x = self.transform.localScale.x;
                if (self.groundDetector.scanner.Scan(out Collider2D collider))
                {
                    if (collider.TryGetComponent(out Controller2D target))
                    {
                        var dir = GetDirection(self, target.transform);
                        if (dir.magnitude > 2)
                        {
                            if (self.groundDetector.CheckCliff())
                                return Vector2.zero;
                            else
                                return dir.x > 0 ? Vector2.left : Vector2.right;
                        }
                        return Vector2.zero;
                    }else
                       return movement * Vector2.left;
                }
      
                if (self.groundDetector.CheckCliff())
                    movement.x = -movement.x;
            }
            return movement;
        }
        public override void GetInputState(Controller2D character, UpdateInput updateInput)
        {
            updateInput(false, false, false);
        }
        public override void GetInputState(string bindingName, Controller2D character, UpdateInput updateInput)
        {
            throw new System.NotImplementedException();
        }

        public override bool ShiftIndexOfSkill(Controller2D character,out bool next)
            => next = false;


        public bool OutOfRange(Controller2D self,Transform target)
        {
            if (target == null)
                return false;
            if (Vector2.Distance(self.transform.position, target.position) < self.groundDetector.collider.bounds.size.magnitude * 2)
                return false;
            else
                return true;
        }
        public Vector2 GetDirection(Controller2D self,Transform target)
            => self.position - (Vector2)target.transform.position;
    }
}
