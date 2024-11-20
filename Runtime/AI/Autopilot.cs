using UnityEngine;
using BTAI;


namespace Yu5h1Lib.Game.Character
{
    public class Autopilot : HostData2D
    {
        public override System.Type GetBehaviourType() => typeof(Behaviour);

        public class Behaviour : Behaviour2D<Autopilot>
        {
            public enum NodeState { Success, Failure, Running, Waiting }

            //protected NodeState state;

            //public NodeState Evaluate()
            //{
            //    return state;
            //}

            public Patrol patrol;

            public Vector2 patrolPoint => patrol.offset;
            public Vector2 destination;
            private Vector2 movement;
            private Transform transform => Body.transform;

            public Controller2D target;
            private Root ai;

            public override void Init(Controller2D controller)
            {
                base.Init(controller);
                patrol = controller.GetComponent<Patrol>();
                ai = BT.Root();

                ai.OpenBranch(
                    BT.If(WithinPatrolRange).OpenBranch(
                            BT.Sequence().OpenBranch(
                                BT.Condition(DoesTargetExits),
                                BT.Call(StopMoving),
                                BT.Wait(1),
                                BT.Call(FollowTarget)
                                )
                        )
                    ,
                    BT.Call(PatrolArea)
                 );
            }
            public Vector2 GetMovementFromGlobalDirection(Vector2 direction)
            {
                var dir = transform.InverseTransformDirection(direction);
                if (!Body.IsFaceForward)
                    dir.x *= -1;
                return dir;
            }
            public override Vector2 GetMovement()
            {
                if ("body does not exists !".printWarningIf(!Body))
                    return Vector2.zero;
                ai.Tick();
 
                return movement;
            }
            private bool NoTarget() => target == null;
            private bool HasTarget() => !NoTarget();            
            //public bool IsTargetInSight() => default(bool);
            private void OnEnemyDetected(){
                Body.GetComponent<EmojiController>()?.ShowEmoji("exclamation mark", 2);
            }
            public void DetectEnemy()
            {                
                if (!Body.detector.scanner.collider || !Body.detector.scanner.Scan(out RaycastHit2D hit))
                {
                    target = null;
                    return;
                }
                if (hit.collider.TryGetComponent(out BlueLine blueLine))
                {
                    target = null;
                    var l = new Line2D(Body.transform.position, patrol.currentPoint);
                    if (blueLine.TryGetIntersection(l, out Vector2 intersection))
                    {
                        patrol.SetCurrentPoint(intersection - (l.direction.normalized * blueLine.Width ) );
                        return;
                    }
                }

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
                movement = GetMovementFromGlobalDirection(patrol.GetDirection()).normalized;
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

            public override bool GetInputState( UpdateInput updateInput)
            {
                return updateInput(false, false, false);
            }
            public override void GetInputState(string bindingName,UpdateInput updateInput)
            {
                throw new System.NotImplementedException();
            }

            public override bool ShiftIndexOfSkill(out bool next)
                => next = false;

            bool WithinPatrolRange() => Vector2.Distance(patrolPoint, Body.position) < patrol.RangeDistance;
            bool OutOfPatrolRange() => !WithinPatrolRange();
            bool DoesTargetExits() => target;
            bool DoesTargetNotExits() => !DoesTargetExits();

            public void StopMoving()
            {
                movement = Vector2.zero;
            }


        }
    }
}
