using UnityEngine;
using BTAI;


namespace Yu5h1Lib.Game.Character
{
    public class Autopilot : HostData2D
    {
        public bool enable;

        public SkillTriggerCondition[] skillTriggerConditions;

        public override System.Type GetBehaviourType() => typeof(Behaviour);

        public class Behaviour : Behaviour2D<Autopilot>
        {
            public enum NodeState { Success, Failure, Running, Waiting }

            //protected NodeState state;

            //public NodeState Evaluate()
            //{
            //    return state;
            //}
			public bool IsNotReady => GameManager.IsBusy || "body does not exist !".printWarningIf(!Body) || !Body.underControl;
            public Patrol patrol;

            public Vector2 patrolPoint => patrol.offset;
            public Vector2 destination;
            protected Vector2 movement;
            private Transform transform => Body.transform;

            public Controller2D target;

            public override void Init(Controller2D controller)
            {
                base.Init(controller);
                patrol = controller.GetComponent<Patrol>();
            }

            #region input
            public override Vector2 GetMovement()
            {
                if (IsNotReady)
                    return Vector2.zero;
                
                return movement;
            }

            public override bool GetInputState(UpdateInput updateInput)
            {
                if (IsNotReady)
                    return false;
                if (Target == null)
                    return false;

                //if (patrol.scanner.collider && patrol.scanner.Scan(out RaycastHit2D hit) &&
                //    hit.collider.TryGetComponent(out BlueLine blueLine) ||
                //    Vector2.Dot(target.position - Body.position, Body.right) < 0)
                //{
                //    target = null;
                //    return updateInput(false, false, false);
                //}

                return updateInput(true, false, false);
            }
            public override void GetInputState(string bindingName, UpdateInput updateInput)
            {
                throw new System.NotImplementedException();
            }

            public override bool ShiftIndexOfSkill(out bool next)
                => next = false; 
            #endregion


            public Vector2 GetMovementFromGlobalDirection(Vector2 direction)
            {
                var dir = transform.InverseTransformDirection(direction);
                if (!Body.IsFaceForward)
                    dir.x *= -1;
                return dir;
            }
            private bool NoTarget() => target == null;
            private bool HasTarget() => !NoTarget();            
            //public bool IsTargetInSight() => default(bool);
            protected void OnEnemyDetected(){
                Body.GetComponent<EmojiController>()?.ShowEmoji("exclamation mark", 2);
            }
            public virtual bool DetectEnemy()
            {
                if (target != null)
                    return true;
                if (!patrol.scanner.collider || !patrol.scanner.Scan(out RaycastHit2D hit))
                {
                    return false;
                }
                //if (hit.collider.TryGetComponent(out BlueLine blueLine))
                //{
                //    target = null;
                //    if (patrol.route.points.Length > 1)
                //    {
                //        var l = new Line2D(Body.transform.position, patrol.Destination);
                //        if (blueLine.TryGetIntersection(l, out Vector2 intersection))
                //        {
                //            patrol.SetCurrentPoint(intersection - (l.direction.normalized * blueLine.Width));
                //            return;
                //        }
                //    }
                //}

                if (hit.collider.TryGetComponent(out Controller2D c))
                {
                    if (target != c)
                    {
                        target = c;
                        OnEnemyDetected();
                    }
                    //var dir = GetDirection(body, c.transform);
                    //if (dir.magnitude > 2)
                    //{
                    //    if (body.detector.CheckCliff())
                    //        movement = Vector2.zero;
                    //    else
                    //        movement = dir.x > 0 ? Vector2.left : Vector2.right;
                    //}
                    //movement = Vector2.zero;
                }
                //else
                //    movement *= Vector2.left;
                return true;
            }
            public void FollowTarget()
            {
                if (!target)
                    return;
                movement = (target.detector.top - Body.detector.top).normalized;
            }
            public void ReturnToPost()
            {
                movement = (patrolPoint - Body.position).normalized;
            }
            public void PatrolArea()
            {                
                if (target == null)
                    movement = GetMovementFromGlobalDirection(patrol.GetDirection()).normalized;
                else if (Vector2.Distance(patrol.offset, Body.position) < 15)
                    movement = (target.detector.top - Body.detector.top).normalized;
                DetectEnemy();
            }
            public void print(string msg)
            {
#if UNITY_EDITOR
                if (UnityEditor.Selection.activeGameObject == Body.gameObject)
                {
                    msg.print();
                }
#endif
            }
            private bool TraceTarget(out Vector2 movement) 
            {
                movement = Vector2.zero;
                if (!target)
                    return false;
                if (Vector2.Distance(Body.position, patrolPoint) > 2 || Vector2.Distance(Body.position,target.position) > 2)
                {
                    target = null;
                    return false;
                }
                movement = (target.detector.top - Body.detector.top).normalized ;
                return true;
            }
            public bool Arrived() => Mathf.Approximately(Vector2.Distance(Body.position, destination), 0);

            bool WithinPatrolRange() => Vector2.Distance(patrolPoint, Body.position) < patrol.RangeDistance;
            bool OutOfPatrolRange() => !WithinPatrolRange();
            bool DoesTargetExit() => target;
            bool DoesTargetNotExit() => !DoesTargetExit();

            public void StopMoving()
            {
                movement = Vector2.zero;
            }


        }
        [System.Serializable]
        public class SkillTriggerCondition
        {
            public SkillData skill;
            public Vector2 direction;
            public float distance;
        }
    }
}
