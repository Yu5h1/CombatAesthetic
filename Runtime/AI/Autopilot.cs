using System.Collections;
using UnityEngine;
using Yu5h1Lib.Runtime;


namespace Yu5h1Lib.Game.Character
{
    public class Autopilot : HostData2D
    {
        public float waitDuration = 1;
        public float frequency = 1;        
        public MinMax PatrolWaitTimeOnNode;

        public bool StopActing = false;
        public bool StopMoving = false;

        public bool randomSkill;
        public int primarySkill = -1;

        public bool keepChasing;

        public bool backToPatrolPoint;

        public float targetfoundWaitTime = 0.5f;
        public float targetLostWaitTime = 0.1f;

        [AutoFill(typeof(AutoFillResources), "Texture","*png|*.jpg|*.bmp")]
        public string exclamationMark = "exclamation mark";

        public string aimsolverName;

        public override System.Type GetBehaviourType() => typeof(Behaviour);

        

        public class Behaviour : Behaviour2D<Autopilot>
        {
            public AnimatorCharacterController2D animBody { get; private set; }
            private AimSolver aimSolver;

            public Scanner2D scanner => Body.scanner;
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
            public bool KeepChasing { get; set; }
            public bool IsTargetInSkillRange { get; private set; }
            public bool IsWaiting { get; private set; }

            private Coroutine waitCoroutine;

            public override void Init(CharacterController2D controller)
            {
                base.Init(controller);
                animBody = controller as AnimatorCharacterController2D;
                patrol = controller.GetComponent<Patrol>();
                emojiControl = controller.GetComponent<EmojiController>();
                KeepChasing = data.keepChasing;

                if (!data.aimsolverName.IsEmpty())
                    if (Body.transform.TryFind(data.aimsolverName, out Transform t))
                        t.TryGetComponent(out aimSolver);
            }

            #region input
            public override Vector2 GetMovement()
            {
                if (IsNotReady || Body.IsActing || IsWaiting || data.StopMoving)
                    return Vector2.zero;
                IsTargetInSkillRange = false;
                if (target)
                {
                    var distanceBetweenTarget = GetDistanceBetweenTarget(out Vector2 selfPoint,out Vector2 targetPoint);

                    //RaycastHit2D obstacleHit = default(RaycastHit2D);
                    //if (scanner.ObstacleMask.value != 0)
                    //    obstacleHit = Physics2D.Linecast(Body.position, target.position, scanner.ObstacleMask);
                    scanner.ObstacleHitTest(Body.position, target.position, out RaycastHit2D obstacleHit);

                    var DirectionToTarget = (targetPoint - selfPoint).normalized;

                    if (obstacleHit || distanceBetweenTarget > scanner.distance)
                    {
                        target = null;
                        return Vector2.zero;
                    }

                    //(aimSolver == null ? true : aimSolver.IsWithInRange(target.transform.position, animBody.currentSkill.angle))
                    ///change mode to Action
                    if (IsWithinSkillRange(distanceBetweenTarget))
                    {
                        if (Vector2.Dot(DirectionToTarget, Body.right) > 0)
                        {
                            IsTargetInSkillRange = true;
                            if (aimSolver != null)
                            {
                                IsTargetInSkillRange = aimSolver.Aim(targetPoint, animBody.currentSkill.angle);
                            }
                            movement = Vector2.zero;
                            Debug.DrawLine(selfPoint, targetPoint, Color.red);
                        }
                        else //turn around
                            movement = new Vector2(-Body.forwardSign,0);
                    }else if (IsWaiting)
                        movement = Vector2.zero;
                    else 
                    {
                        Debug.DrawLine(selfPoint, targetPoint, Color.yellow);
                        movement = GetMovementFromGlobalDirection(DirectionToTarget);
                    }
                }
                else
                    PatrolAreaAndScanning();
                
                return movement;
            }

            public override bool GetInputState(UpdateInput updateInput)
            {
                if (IsNotReady || target == null || IsWaiting || Body.IsActing || data.StopActing)
                    return false;
                if (IsTargetInSkillRange)
                {
                    if (target.attribute.exhausted)
                    {
                        target = null;
                        IsTargetInSkillRange = false;
                        return false;
                    }
                    Wait(Random.Range(0,data.frequency));
                    if (data.randomSkill && animBody)
                        animBody.RandomCurrentSkill(data.primarySkill);

                    return updateInput(true, false, false);
                }else
                    return false;
            }
            public override void GetInputState(string bindingName, UpdateInput updateInput)
            {
                //throw new System.NotImplementedException();
            }

            public override bool ShiftIndexOfAction(out bool next)
                => next = false;

            public void Wait(float duration)
            {
                if (duration > 0)
                    Body.StartCoroutine(ref waitCoroutine, WaitProcess(duration));
            }

            public void Wait()
                => Wait(data.waitDuration);
            #endregion
            #region Events
            protected virtual void OnTargetChanged()
            {

                if (target)
                {
                    patrol.target = target.transform;
                    emojiControl?.ShowEmoji(data.exclamationMark, 2);
                    AudioManager.Play($"�o�{���a", transform.position);
                    Wait(data.targetfoundWaitTime);
                }
                else
                {
                    if (aimSolver)
                        aimSolver.StopAim();
                    patrol.target = null;
                    emojiControl?.HideEmoji();
                    Wait(data.targetLostWaitTime);
                }


            }
            #endregion
            #region Process
            IEnumerator WaitProcess(float delay) {
                IsWaiting = true;
                yield return new WaitForSeconds(delay);
                IsWaiting = false;
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
            public bool IsWithinSkillRange(float distanceToTarget)
            {
                if (!target || !animBody || !animBody.currentSkill)
                    return false;
                return distanceToTarget < animBody.currentSkill.distance * ((Vector2)Body.transform.localScale).magnitude;
            }
            public virtual void DetectEnemy()
            {
                if (!KeepChasing && target != null && Vector2.Distance(Body.position, patrol.Destination) > scanner.distance )
                {
                    target = null;
                    return;
                }
                if (scanner.Scan(out RaycastHit2D hit))
                {
#if UNITY_EDITOR
                    Debug.DrawLine(scanner.offset, hit.point, Color.cyan);
#endif
                    if (hit.collider.TryGetComponent(out CharacterController2D c))
                    {
                        target = c;
                        return;
                    }
                }
                if (!KeepChasing)
                    target = null;
            }
            //public void FollowTarget()
            //{
            //    if (!target)
            //        return;
            //    if (GetDistanceBetweenTarget() < 2)
            //        movement = Vector2.zero;
            //    movement = (target.detector.top - Body.detector.center).normalized;
            //}
            protected virtual void PatrolAreaAndScanning()
            {
                if (data.backToPatrolPoint && !patrol.WithinPatrolRange() && !Body.teleportable.IsTeleporting)//&& !Teleporter.IsTeleporting(Body))
                    Body.teleportable.TeleportTo(patrol.offset,0.5f);

                if (patrol.IsAvailable())
                    movement = GetMovementFromGlobalDirection(patrol.GetDirection(Body.detector.CheckCliff(), OnNodeArrived)).normalized;
                else
                {
                    if (Body.detector.CheckCliff())
                    {
                        movement = movement.x > 0 ? Vector2.left : Vector2.right;
                    }
                    else
                        movement = new Vector2(Body.forwardSign,0);
                }
                DetectEnemy();
            }
            private void OnNodeArrived()
            {
                if (data.PatrolWaitTimeOnNode.Length > 0)
                    Wait(Random.Range(data.PatrolWaitTimeOnNode.Min, data.PatrolWaitTimeOnNode.Max));
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

            protected bool OutOfPatrolRange() => !patrol.WithinPatrolRange();
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
