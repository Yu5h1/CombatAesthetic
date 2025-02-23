using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Yu5h1Lib.Game.Character
{
    public class Autopilot : HostData2D
    {
        public bool enable;
        public float waitDuration = 1;
        public bool ActionDelay;

        public override System.Type GetBehaviourType() => typeof(Behaviour);

        public class Behaviour : Behaviour2D<Autopilot>
        {
            public enum NodeState { Success, Failure, Running, Waiting }
			public bool IsNotReady => GameManager.IsBusy() || "body does not exist !".printWarningIf(!Body) || !Body.underControl;
            protected Patrol patrol;
            protected EmojiController emojiControl;

            public Vector2 patrolPoint => patrol.offset;
            public Vector2 destination;
            protected Vector2 movement;         

            private CharacterController2D _target;
            public CharacterController2D target 
            {
                get => _target;
                set {
                    if (_target == value)
                        return;
                    _target = value;                                            
                    OnTargetChanged();
                }
            }
            public bool IsTargetInSkillRange;
            public bool waiting;
            Coroutine waitCoroutine;
            public override void Init(CharacterController2D controller)
            {
                base.Init(controller);
                patrol = controller.GetComponent<Patrol>();
                emojiControl = controller.GetComponent<EmojiController>();
            }

            #region input
            public override Vector2 GetMovement()
            {
                if (IsNotReady)
                    return Vector2.zero;
                IsTargetInSkillRange = false;
                if (target)
                {
                    var distanceBetweenTarget = GetDistanceBetweenTarget(out Vector2 selfPoint,out Vector2 targetPoint);

                    RaycastHit2D obstacleHit = default(RaycastHit2D);
                    if (patrol.scanner.ObstacleMask.value != 0)
                        obstacleHit = Physics2D.Linecast(Body.position, target.position, patrol.scanner.ObstacleMask);

                    var DirectionToTarget = (targetPoint - selfPoint).normalized;

                    if (obstacleHit)
                    {
                        target = null;
                        return Vector2.zero;
                    }

                    if (IsWithinSkillRange(distanceBetweenTarget))
                    {
                        if (Vector2.Dot(DirectionToTarget, Body.right) > 0)
                        {
                            IsTargetInSkillRange = true;
                            movement = Vector2.zero;
                            Debug.DrawLine(selfPoint, targetPoint, Color.red);
                        }
                        else //turn around
                            movement = new Vector2(-Body.forwardSign,0);
                    }else if (waiting)
                        movement = Vector2.zero;
                    else 
                    {
                        Debug.DrawLine(selfPoint, targetPoint, Color.yellow);
                        movement = GetMovementFromGlobalDirection(DirectionToTarget);
                    }
                }
                else
                    PatrolArea();
                
                return movement;
            }

            public override bool GetInputState(UpdateInput updateInput)
            {
                if (IsNotReady || target == null || waiting)
                    return false;
                if (IsTargetInSkillRange)
                {
                    if (target.attribute.exhausted)
                    {
                        target = null;
                        IsTargetInSkillRange = false;
                        return false;
                    }
                    Wait(3);
                    return updateInput(true, false, false);
                }else
                    return false;
            }
            public override void GetInputState(string bindingName, UpdateInput updateInput)
            {
                throw new System.NotImplementedException();
            }

            public override bool ShiftIndexOfSkill(out bool next)
                => next = false;
            #endregion
            #region Events
            protected virtual void OnTargetChanged()
            {
                if (target)
                {
                    patrol.target = target.transform;
                    emojiControl?.ShowEmoji("exclamation mark", 2);
                    Body.StartCoroutine(ref waitCoroutine, Wait(data.waitDuration));
                }
                else
                {
                    waiting = false;
                    Body.StopCoroutine(waitCoroutine);
                    patrol.target = null;
                    emojiControl?.HideEmoji();
                }
            }
            #endregion
            #region Process
            IEnumerator Wait(float delay){
                waiting = true;
                yield return new WaitForSeconds(delay);
                waiting = false;
            }
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
 
            public float GetDistanceBetweenTarget(out Vector2 selfPoint, out Vector2 targetPoint)
            {
                selfPoint = Body.detector.ClosestPoint(target.position);
                targetPoint  = target.detector.ClosestPoint(Body.position);
                return Vector2.Distance(selfPoint, targetPoint);
            }
            public bool IsWithinSkillRange(float distanceBetweenTarget)
            {
                if (!target || !(Body is AnimatorCharacterController2D animBody) || !animBody.currentSkill)
                    return false;
                return distanceBetweenTarget < animBody.currentSkill.distance;
            }

            public virtual bool DetectEnemy()
            {
                if (!patrol.enabled || waiting)
                    return false;
                if (target != null && Vector2.Distance(Body.position, patrol.Destination) < 5 )
                    return true;

                if (patrol.Scan(out RaycastHit2D hit))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(patrol.scanner.start, hit.point, Color.cyan);
#endif
                    if (hit.collider.TryGetComponent(out CharacterController2D c))
                        return target = c;
                }
                return target = null;

                //return (patrol.Scan(out RaycastHit2D hit) && hit.collider.TryGetComponent(out Controller2D c)) ?
                //       (target = c) :  (target = null);
            }
            //public void FollowTarget()
            //{
            //    if (!target)
            //        return;
            //    if (GetDistanceBetweenTarget() < 2)
            //        movement = Vector2.zero;
            //    movement = (target.detector.top - Body.detector.center).normalized;
            //}
            public virtual void PatrolArea()
            {
                if (patrol.enabled)
                    movement = GetMovementFromGlobalDirection(patrol.GetDirection()).normalized;
                else
                {
                    if (Body.detector.CheckCliff())
                        movement = movement.x > 0 ? Vector2.left : Vector2.right;
                    else
                        movement = new Vector2(Body.forwardSign,0);
                }
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
            public bool Arrived() => Mathf.Approximately(Vector2.Distance(Body.position, destination), 0);

            protected bool WithinPatrolRange() => Vector2.Distance(patrolPoint, Body.position) < patrol.RangeDistance;
            protected bool OutOfPatrolRange() => !WithinPatrolRange();
            protected bool DoesTargetExit() => target;
            protected bool DoesTargetNotExit() => !DoesTargetExit();

            public void StopMoving()
            {
                movement = Vector2.zero;
            }
            protected Vector2 GetDirectionToTarget()
            {
                if (!target)
                    return Vector2.zero;
                return target.transform.position - Body.transform.position;
            }

        }

    }
}
