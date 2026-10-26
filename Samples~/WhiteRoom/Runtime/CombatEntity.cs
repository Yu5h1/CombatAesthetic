using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public enum CombatState
    {
        Idle,
        Moving,
        Jumping,
        Crouching,
        AttackStartup,
        AttackActive,
        AttackRecovery,
        Guarding,
        Parrying,
        Flinching,
        Stunned,
        Incapacitated
    }

    [RequireComponent(typeof(CharacterController))]
    public class CombatEntity : MonoBehaviour
    {
        private static readonly List<CombatEntity> s_ActiveEntities = new List<CombatEntity>();
        public static IReadOnlyList<CombatEntity> ActiveEntities => s_ActiveEntities;

        [Header("Identity & Team")]
        [SerializeField] private string _entityId = "Entity";
        [SerializeField] private int _teamId = 0; // 0: Player/Ally, 1: Enemy/Dummy

        [Header("Health & Stats")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth = 100f;

        [Header("Locomotion Settings")]
        [SerializeField] private float _walkSpeed = 5.0f;
        [SerializeField] private float _crouchSpeed = 2.5f;
        [SerializeField] private float _turnSpeed = 720f;
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _gravity = -20f;

        [Header("Crouch Collider Settings")]
        [SerializeField] private float _standingHeight = 2.0f;
        [SerializeField] private float _crouchingHeight = 1.0f;
        [SerializeField] private Vector3 _standingCenter = new Vector3(0, 1.0f, 0);
        [SerializeField] private Vector3 _crouchingCenter = new Vector3(0, 0.5f, 0);

        [Header("Attack Windows & Parameters")]
        [SerializeField] private float _attackStartupTime = 0.20f; // 前搖蓄力（對手可見的預警）
        [SerializeField] private float _attackActiveTime = 0.15f;  // 判定有效打擊區間
        [SerializeField] private float _attackRecoveryTime = 0.25f;// 後搖收招區間
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackDamage = 20f;
        [SerializeField] private float _attackKnockback = 7.5f;

        [Header("Defense Parameters")]
        [SerializeField] private float _parryWindow = 0.18f; // 精準彈刀判定時窗（按下的前0.18秒）
        [SerializeField] private float _parryStunDuration = 1.0f; // 彈刀反制使攻擊者硬直時間

        // Components & Runtime State
        private CharacterController _controller;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _residualVelocity;
        private float _verticalVelocity;
        private CombatState _state = CombatState.Idle;
        private float _stateTimer = 0f;
        private float _guardStartTime = -1f;
        private bool _isCrouching = false;
        private bool _hitRegisteredInCurrentAttack = false;

        // Presentation callback hooks
        public Transform VisualRoot { get; set; }
        public Transform FistTransform { get; set; }

        // Events
        public event Action<string> OnCombatLog;
        public event Action<CombatEntity, float, float> OnHealthChanged; // (entity, current, max)
        public event Action<CombatEntity, CombatState> OnStateChanged;

        public string EntityId => _entityId;
        public int TeamId => _teamId;
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public CombatState State => _state;
        public bool IsCrouching => _isCrouching;
        public bool IsIncapacitated => _state == CombatState.Incapacitated;
        public bool IsGuarding => _state == CombatState.Guarding || _state == CombatState.Parrying;

        protected virtual void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            _currentHealth = _maxHealth;
        }

        protected virtual void OnEnable()
        {
            if (!s_ActiveEntities.Contains(this)) s_ActiveEntities.Add(this);
        }

        protected virtual void OnDisable()
        {
            s_ActiveEntities.Remove(this);
        }

        /// <summary>
        /// 執行單幀共同戰鬥循環（由本機 Player、AI 傀儡或遠端網路接收器驅動）
        /// </summary>
        public void StepCombat(CombatActionIntent intent, Transform cameraTransform = null)
        {
            if (intent.GlobalReset)
            {
                ResetAllArenaEntities();
                return;
            }

            if (_state == CombatState.Incapacitated)
            {
                HandleLocomotion(CombatActionIntent.Empty, null);
                return;
            }

            // 1. 處理防禦輸入
            HandleDefenseInput(intent);

            // 2. 處理蹲姿輸入
            HandleCrouchInput(intent);

            // 3. 處理出招時序（Startup -> Active -> Recovery）
            UpdateAttackTimeline();

            // 4. 處理起招觸發
            if (intent.Attack && CanInitiateAttack())
            {
                InitiateAttack(intent.MoveInput, cameraTransform);
            }

            // 5. 處理受擊與擊暈計時恢復
            UpdateFlinchAndStunTimers();

            // 6. 處理位移、跳躍與物理重力整合
            HandleLocomotion(intent, cameraTransform);
        }

        private void HandleDefenseInput(CombatActionIntent intent)
        {
            if (_state == CombatState.AttackStartup || _state == CombatState.AttackActive ||
                _state == CombatState.Flinching || _state == CombatState.Stunned)
            {
                return;
            }

            if (intent.ParryTap)
            {
                _guardStartTime = Time.time;
                SetState(CombatState.Parrying);
                _stateTimer = _parryWindow;
                OnCombatLog?.Invoke($"[{_entityId}] 進入精準彈刀姿態（{_parryWindow:F2}s 時窗）");
            }
            else if (intent.GuardHold)
            {
                if (_state != CombatState.Guarding && _state != CombatState.Parrying)
                {
                    _guardStartTime = Time.time;
                    SetState(CombatState.Guarding);
                    OnCombatLog?.Invoke($"[{_entityId}] 進入正面格擋防禦姿態");
                }
            }
            else
            {
                if (_state == CombatState.Guarding || _state == CombatState.Parrying)
                {
                    SetState(CombatState.Idle);
                    _guardStartTime = -1f;
                }
            }
        }

        private void HandleCrouchInput(CombatActionIntent intent)
        {
            if (_state == CombatState.AttackStartup || _state == CombatState.AttackActive ||
                _state == CombatState.Flinching || _state == CombatState.Stunned)
            {
                return;
            }

            bool wantCrouch = _isCrouching;
            if (intent.CrouchToggle)
            {
                wantCrouch = !_isCrouching;
            }
            else if (intent.CrouchHold)
            {
                wantCrouch = true;
            }
            else if (!intent.CrouchHold && _isCrouching && !intent.CrouchToggle)
            {
                // 按住放開後自動起身
                wantCrouch = false;
            }

            if (wantCrouch != _isCrouching && _controller.isGrounded)
            {
                _isCrouching = wantCrouch;
                _controller.height = _isCrouching ? _crouchingHeight : _standingHeight;
                _controller.center = _isCrouching ? _crouchingCenter : _standingCenter;
                OnCombatLog?.Invoke($"[{_entityId}] {(_isCrouching ? "進入蹲下姿態" : "恢復站立姿態")}");
            }
        }

        private bool CanInitiateAttack()
        {
            return _state == CombatState.Idle || _state == CombatState.Moving || _state == CombatState.Crouching;
        }

        private void InitiateAttack(Vector2 moveInput, Transform cameraTransform)
        {
            // 若有移動輸入則朝向輸入方向出招（Directional Attack），否則原地出招（Stationary Attack）
            if (moveInput.sqrMagnitude > 0.05f)
            {
                Vector3 fwd = transform.forward;
                if (cameraTransform != null)
                {
                    Vector3 camFwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                    Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                    fwd = (camFwd * moveInput.y + camRight * moveInput.x).normalized;
                }
                transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
                OnCombatLog?.Invoke($"[{_entityId}] 發起【方向攻擊】（蓄力前搖 {_attackStartupTime:F2}s）");
            }
            else
            {
                OnCombatLog?.Invoke($"[{_entityId}] 發起【原地攻擊】（蓄力前搖 {_attackStartupTime:F2}s）");
            }

            SetState(CombatState.AttackStartup);
            _stateTimer = _attackStartupTime;
            _hitRegisteredInCurrentAttack = false;
        }

        private void UpdateAttackTimeline()
        {
            if (_state == CombatState.AttackStartup)
            {
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    // 轉入有效打擊判定階段
                    SetState(CombatState.AttackActive);
                    _stateTimer = _attackActiveTime;
                    PerformHitCheck();
                }
            }
            else if (_state == CombatState.AttackActive)
            {
                // 有效判定區間內若尚未命中，持續進行命中查詢
                if (!_hitRegisteredInCurrentAttack)
                {
                    PerformHitCheck();
                }

                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    // 轉入後搖收招階段
                    SetState(CombatState.AttackRecovery);
                    _stateTimer = _attackRecoveryTime;
                }
            }
            else if (_state == CombatState.AttackRecovery)
            {
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    SetState(_isCrouching ? CombatState.Crouching : CombatState.Idle);
                }
            }
        }

        private void PerformHitCheck()
        {
            if (_hitRegisteredInCurrentAttack) return;

            Vector3 hitCenter = transform.position + Vector3.up * 1.1f + transform.forward * (_attackRange * 0.7f);
            Collider[] colliders = Physics.OverlapSphere(hitCenter, _attackRange * 0.6f);

            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue;

                CombatEntity target = col.GetComponent<CombatEntity>();
                if (target != null && target.TeamId != _teamId && !target.IsIncapacitated)
                {
                    _hitRegisteredInCurrentAttack = true;
                    Vector3 knockbackDir = (target.transform.position - transform.position).normalized;
                    knockbackDir.y = 0.15f;
                    Vector3 impulse = knockbackDir.normalized * _attackKnockback;

                    target.TakeHit(this, _attackDamage, impulse);
                    break;
                }
            }
        }

        /// <summary>
        /// 受擊判定核心：普通格擋、精準彈刀反制與實質中斷攻擊
        /// </summary>
        public void TakeHit(CombatEntity attacker, float damage, Vector3 knockback)
        {
            if (_state == CombatState.Incapacitated) return;

            Vector3 toAttacker = (attacker.transform.position - transform.position).normalized;
            float facingDot = Vector3.Dot(transform.forward, toAttacker);

            // 正面 120 度角防禦判定
            if (IsGuarding && facingDot > 0.3f)
            {
                // 檢查精準彈刀（進入防禦姿態 0.18 秒內）
                if (Time.time - _guardStartTime <= _parryWindow || _state == CombatState.Parrying)
                {
                    SetState(CombatState.Parrying);
                    _stateTimer = 0.3f;
                    OnCombatLog?.Invoke($"【精準防禦 (Parry) 成功！】[{_entityId}] 完美化解傷害，並擊暈反制 [{attacker.EntityId}]！");

                    // 實質中止攻擊者的進攻！打斷其動作並施加暈眩！
                    attacker.InterruptAttack(_parryStunDuration);
                    return;
                }
                else
                {
                    // 普通防禦：80% 減傷、70% 減擊退
                    damage *= 0.20f;
                    knockback *= 0.30f;
                    _residualVelocity += knockback;
                    ApplyDamage(damage);
                    OnCombatLog?.Invoke($"【普通防禦】[{_entityId}] 格擋承受 {damage:F1} 傷害。");
                    return;
                }
            }

            // 直擊命中：承受全部傷害與擊退，進入硬直
            _residualVelocity += knockback;
            SetState(CombatState.Flinching);
            _stateTimer = 0.35f;
            ApplyDamage(damage);
            OnCombatLog?.Invoke($"【直擊受創】[{_entityId}] 受到 {damage:F1} 點重擊！");
        }

        /// <summary>
        /// 精準防禦成功時呼叫：打斷攻擊者的出招時序，並令其進入暈眩 (Stunned) 狀態
        /// </summary>
        public void InterruptAttack(float stunDuration)
        {
            _hitRegisteredInCurrentAttack = true; // 廢除本次出招的剩餘判定
            SetState(CombatState.Stunned);
            _stateTimer = stunDuration;
            OnCombatLog?.Invoke($"【攻擊被中斷！】[{_entityId}] 招式遭對手彈開，陷入暈眩 {stunDuration:F1} 秒！");
        }

        private void ApplyDamage(float damage)
        {
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            OnHealthChanged?.Invoke(this, _currentHealth, _maxHealth);

            if (_currentHealth <= 0f)
            {
                SetState(CombatState.Incapacitated);
                OnCombatLog?.Invoke($"【失去戰力 (KO)】[{_entityId}] 生命值歸零，倒下！");
            }
        }

        private void UpdateFlinchAndStunTimers()
        {
            if (_state == CombatState.Flinching || _state == CombatState.Stunned || _state == CombatState.Parrying)
            {
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    SetState(_isCrouching ? CombatState.Crouching : CombatState.Idle);
                }
            }
        }

        private void HandleLocomotion(CombatActionIntent intent, Transform cameraTransform)
        {
            Vector3 moveDir = Vector3.zero;
            Vector2 input = intent.MoveInput;

            bool canMove = (_state == CombatState.Idle || _state == CombatState.Moving || _state == CombatState.Crouching);
            if (canMove && input.sqrMagnitude > 0.01f)
            {
                if (cameraTransform != null)
                {
                    Vector3 camFwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                    Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                    moveDir = (camFwd * input.y + camRight * input.x).normalized;
                }
                else
                {
                    moveDir = new Vector3(input.x, 0, input.y).normalized;
                }

                // 朝向移動方向平滑旋轉
                if (moveDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
                }
            }

            float currentSpeed = _isCrouching ? _crouchSpeed : _walkSpeed;
            if (_state == CombatState.AttackStartup || _state == CombatState.AttackActive)
            {
                currentSpeed *= 0.15f; // 出招時大幅限制主動速度
            }
            else if (!canMove)
            {
                currentSpeed = 0f; // 硬直／暈眩／倒地時鎖定主動位移
            }

            Vector3 driveVelocity = moveDir * (currentSpeed * input.magnitude);

            // 跳躍與重力處理
            if (_controller.isGrounded)
            {
                _verticalVelocity = -2f; // 貼地穩定力

                if (intent.Jump && canMove && !_isCrouching)
                {
                    _verticalVelocity = Mathf.Sqrt(2f * _jumpHeight * Mathf.Abs(_gravity));
                    SetState(CombatState.Jumping);
                    OnCombatLog?.Invoke($"[{_entityId}] 起跳！");
                }
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
                if (canMove) SetState(CombatState.Jumping);
            }

            // 外力衝量指數衰減
            _residualVelocity = Vector3.Lerp(_residualVelocity, Vector3.zero, 5f * Time.deltaTime);

            Vector3 totalVelocity = driveVelocity + _residualVelocity;
            totalVelocity.y = _verticalVelocity;

            if (_controller.enabled)
            {
                _controller.Move(totalVelocity * Time.deltaTime);
            }

            // 更新常態位移狀態
            if (_controller.isGrounded && canMove)
            {
                if (_isCrouching)
                    SetState(CombatState.Crouching);
                else if (input.sqrMagnitude > 0.05f)
                    SetState(CombatState.Moving);
                else
                    SetState(CombatState.Idle);
            }
        }

        private void SetState(CombatState newState)
        {
            if (_state != newState)
            {
                _state = newState;
                OnStateChanged?.Invoke(this, _state);
            }
        }

        /// <summary>
        /// 全場統一重設：還原全場所有角色至初始位置、生命值、狀態與計時
        /// </summary>
        public static void ResetAllArenaEntities()
        {
            foreach (var entity in s_ActiveEntities)
            {
                if (entity != null)
                {
                    entity.ResetEntity();
                }
            }
        }

        public void ResetEntity()
        {
            if (_controller != null) _controller.enabled = false;
            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;
            if (_controller != null) _controller.enabled = true;

            _currentHealth = _maxHealth;
            _residualVelocity = Vector3.zero;
            _verticalVelocity = 0f;
            _isCrouching = false;
            _stateTimer = 0f;
            _guardStartTime = -1f;
            _hitRegisteredInCurrentAttack = false;

            if (_controller != null)
            {
                _controller.height = _standingHeight;
                _controller.center = _standingCenter;
            }

            SetState(CombatState.Idle);
            OnHealthChanged?.Invoke(this, _currentHealth, _maxHealth);
            OnCombatLog?.Invoke($"[{_entityId}] 狀態重設完成（生命值 100/100，位置已歸零）");
        }

        public void InitializeSettings(string id, int team, float maxHp)
        {
            _entityId = id;
            _teamId = team;
            _maxHealth = maxHp;
            _currentHealth = maxHp;
        }
    }
}
