using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public enum CharacterActionState
    {
        Idle,
        Moving,
        Jumping,
        Crouching,
        Attacking,
        Guarding,
        Parrying,
        Flinching
    }

    [RequireComponent(typeof(CharacterController))]
    public class WhiteRoomCharacter : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _crouchSpeed = 2.5f;
        [SerializeField] private float _turnSpeed = 720f;
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _gravity = -20f;

        [Header("Crouch Settings")]
        [SerializeField] private float _standingHeight = 2.0f;
        [SerializeField] private float _crouchingHeight = 1.0f;
        [SerializeField] private Vector3 _standingCenter = new Vector3(0, 1.0f, 0);
        [SerializeField] private Vector3 _crouchingCenter = new Vector3(0, 0.5f, 0);

        [Header("Combat Settings")]
        [SerializeField] private float _attackStartupTime = 0.1f;
        [SerializeField] private float _attackActiveTime = 0.15f;
        [SerializeField] private float _attackRecoveryTime = 0.25f;
        [SerializeField] private float _attackRange = 1.2f;
        [SerializeField] private float _attackDamage = 20f;
        [SerializeField] private float _attackKnockback = 8f;
        [SerializeField] private float _parryWindow = 0.18f;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _rightFist;

        // Runtime state
        private CharacterController _controller;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _residualVelocity;
        private float _verticalVelocity;
        private bool _isCrouching;
        private bool _isGuarding;
        private float _actionTimer;
        private float _guardStartTime;
        private CharacterActionState _state = CharacterActionState.Idle;
        private bool _attackHitProcessed;

        public CharacterActionState CurrentState => _state;
        public bool IsGrounded => _controller.isGrounded;
        public bool IsCrouching => _isCrouching;
        public bool IsGuarding => _isGuarding;

        public event Action<string> OnCombatAction;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controller.height = _standingHeight;
            _controller.center = _standingCenter;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        private void Start()
        {
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            HandleResetInput();
            HandleCombatInput();
            UpdateActionState();
            HandleLocomotion();
            ApplyVisualFeedback();
        }

        private void HandleResetInput()
        {
            bool resetPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                resetPressed = true;
#endif
            if (Input.GetKeyDown(KeyCode.R))
                resetPressed = true;

            if (resetPressed)
            {
                ResetCharacter();
            }
        }

        public void ResetCharacter()
        {
            _controller.enabled = false;
            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;
            _controller.enabled = true;

            _verticalVelocity = 0f;
            _residualVelocity = Vector3.zero;
            _isCrouching = false;
            _isGuarding = false;
            _state = CharacterActionState.Idle;
            _actionTimer = 0f;
            SetCrouchState(false);

            OnCombatAction?.Invoke("情境重設：位置與速度已歸零。");
        }

        private void HandleCombatInput()
        {
            if (_state == CharacterActionState.Flinching) return;

            // Attack input (Left Mouse Button)
            bool attackPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                attackPressed = true;
#endif
            if (Input.GetMouseButtonDown(0))
                attackPressed = true;

            // Guard input (Right Mouse Button)
            bool guardHeld = false;
            bool guardPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                guardHeld = Mouse.current.rightButton.isPressed;
                guardPressed = Mouse.current.rightButton.wasPressedThisFrame;
            }
#endif
            if (Input.GetMouseButton(1)) guardHeld = true;
            if (Input.GetMouseButtonDown(1)) guardPressed = true;

            // Crouch input (C key or Left Ctrl)
            bool crouchPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.leftCtrlKey.wasPressedThisFrame))
                crouchPressed = true;
#endif
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
                crouchPressed = true;

            if (crouchPressed && _controller.isGrounded && _state != CharacterActionState.Attacking)
            {
                SetCrouchState(!_isCrouching);
            }

            // Trigger Attack
            if (attackPressed && CanAttack())
            {
                StartAttack();
                return;
            }

            // Update Guard
            if (guardHeld && _controller.isGrounded && _state != CharacterActionState.Attacking)
            {
                if (!_isGuarding)
                {
                    _isGuarding = true;
                    _guardStartTime = Time.time;
                    _state = CharacterActionState.Guarding;
                    OnCombatAction?.Invoke("進入防禦姿態。");
                }
            }
            else if (_isGuarding)
            {
                _isGuarding = false;
                if (_state == CharacterActionState.Guarding)
                {
                    _state = CharacterActionState.Idle;
                }
            }
        }

        private bool CanAttack()
        {
            if (_state == CharacterActionState.Attacking)
            {
                // Can cancel in recovery window
                return _actionTimer > (_attackStartupTime + _attackActiveTime);
            }
            return _state != CharacterActionState.Flinching;
        }

        private void StartAttack()
        {
            _state = CharacterActionState.Attacking;
            _actionTimer = 0f;
            _attackHitProcessed = false;
            _isGuarding = false;

            // Check if directional attack
            Vector2 input = ReadMoveInput();
            if (input.sqrMagnitude > 0.05f && _cameraTransform != null)
            {
                Vector3 camFwd = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
                Vector3 targetDir = (camFwd * input.y + camRight * input.x).normalized;
                transform.rotation = Quaternion.LookRotation(targetDir, Vector3.up);
                OnCombatAction?.Invoke("方向攻擊出手！");
            }
            else
            {
                OnCombatAction?.Invoke("原地直拳出手！");
            }
        }

        private void UpdateActionState()
        {
            if (_state == CharacterActionState.Attacking)
            {
                _actionTimer += Time.deltaTime;

                // Active Window: Hit Detection
                if (_actionTimer >= _attackStartupTime && _actionTimer <= (_attackStartupTime + _attackActiveTime))
                {
                    if (!_attackHitProcessed)
                    {
                        PerformAttackHitCheck();
                    }
                }

                // Finish attack
                if (_actionTimer >= (_attackStartupTime + _attackActiveTime + _attackRecoveryTime))
                {
                    _state = CharacterActionState.Idle;
                }
            }
            else if (_state == CharacterActionState.Flinching)
            {
                _actionTimer += Time.deltaTime;
                if (_actionTimer >= 0.4f)
                {
                    _state = CharacterActionState.Idle;
                }
            }
        }

        private void PerformAttackHitCheck()
        {
            _attackHitProcessed = true;
            Vector3 hitCenter = transform.position + transform.forward * (_attackRange * 0.8f) + Vector3.up * 1.0f;
            Collider[] colliders = Physics.OverlapSphere(hitCenter, _attackRange * 0.6f);

            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue;

                var dummy = col.GetComponent<WhiteRoomDummy>();
                if (dummy != null)
                {
                    Vector3 knockbackDir = (dummy.transform.position - transform.position).normalized;
                    knockbackDir.y = 0.2f;
                    dummy.TakeHit(_attackDamage, knockbackDir.normalized * _attackKnockback);
                    OnCombatAction?.Invoke($"擊中目標！造成 {_attackDamage} 傷害，施加衝量。");
                }
            }
        }

        private void HandleLocomotion()
        {
            Vector2 input = ReadMoveInput();
            Vector3 moveDir = Vector3.zero;

            if (_cameraTransform != null && input.sqrMagnitude > 0.01f)
            {
                Vector3 camFwd = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
                moveDir = (camFwd * input.y + camRight * input.x).normalized;

                // Rotate towards movement direction unless attacking
                if (_state != CharacterActionState.Attacking)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
                }
            }

            // Speed calculation
            float currentSpeed = _isCrouching ? _crouchSpeed : _walkSpeed;
            if (_isGuarding) currentSpeed *= 0.4f;
            if (_state == CharacterActionState.Attacking) currentSpeed *= 0.2f;

            Vector3 driveVelocity = moveDir * (currentSpeed * input.magnitude);

            // Jumping & Gravity
            if (_controller.isGrounded)
            {
                _verticalVelocity = -2f; // Slight downward snap

                bool jumpPressed = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                    jumpPressed = true;
#endif
                if (Input.GetKeyDown(KeyCode.Space))
                    jumpPressed = true;

                if (jumpPressed && !_isCrouching && _state != CharacterActionState.Attacking && _state != CharacterActionState.Flinching)
                {
                    _verticalVelocity = Mathf.Sqrt(2f * _jumpHeight * Mathf.Abs(_gravity));
                    _state = CharacterActionState.Jumping;
                    OnCombatAction?.Invoke("起跳！");
                }
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
                if (_state != CharacterActionState.Attacking && _state != CharacterActionState.Flinching)
                {
                    _state = CharacterActionState.Jumping;
                }
            }

            // Decay residual velocity (Exponential decay)
            _residualVelocity = Vector3.Lerp(_residualVelocity, Vector3.zero, 5f * Time.deltaTime);

            // Total velocity integration: Drive + Impulse + Vertical
            Vector3 totalVelocity = driveVelocity + _residualVelocity;
            totalVelocity.y = _verticalVelocity;

            _controller.Move(totalVelocity * Time.deltaTime);

            // Update grounded action state
            if (_controller.isGrounded && _state != CharacterActionState.Attacking && _state != CharacterActionState.Flinching)
            {
                if (_isGuarding)
                    _state = CharacterActionState.Guarding;
                else if (_isCrouching)
                    _state = CharacterActionState.Crouching;
                else if (input.sqrMagnitude > 0.05f)
                    _state = CharacterActionState.Moving;
                else
                    _state = CharacterActionState.Idle;
            }
        }

        private void SetCrouchState(bool crouch)
        {
            _isCrouching = crouch;
            _controller.height = _isCrouching ? _crouchingHeight : _standingHeight;
            _controller.center = _isCrouching ? _crouchingCenter : _standingCenter;
            OnCombatAction?.Invoke(_isCrouching ? "進入蹲下姿態。" : "恢復站立姿態。");
        }

        public void TakeHit(float damage, Vector3 knockback, Vector3 attackerPos)
        {
            Vector3 toAttacker = (attackerPos - transform.position).normalized;
            float facingDot = Vector3.Dot(transform.forward, toAttacker);

            // Frontal defense check (within 120 degree cone: dot > 0.5)
            if (_isGuarding && facingDot > 0.3f)
            {
                // Check for precise parry (tapped within parry window)
                if (Time.time - _guardStartTime <= _parryWindow)
                {
                    _state = CharacterActionState.Parrying;
                    OnCombatAction?.Invoke("【精準防禦 (Parry) 成功！】敵方攻擊被化解！");
                    return;
                }
                else
                {
                    // Ordinary guard: 80% damage reduction, 70% knockback reduction
                    damage *= 0.2f;
                    knockback *= 0.3f;
                    _residualVelocity += knockback;
                    OnCombatAction?.Invoke($"【普通格擋】正面防禦承受 {damage:F1} 傷害。");
                    return;
                }
            }

            // Direct hit: apply full knockback and flinch
            _residualVelocity += knockback;
            _state = CharacterActionState.Flinching;
            _actionTimer = 0f;
            OnCombatAction?.Invoke($"【受到打擊】承受 {damage:F1} 傷害，被擊退！");
        }

        private Vector2 ReadMoveInput()
        {
            Vector2 input = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) input.y += 1f;
                if (Keyboard.current.sKey.isPressed) input.y -= 1f;
                if (Keyboard.current.aKey.isPressed) input.x -= 1f;
                if (Keyboard.current.dKey.isPressed) input.x += 1f;
            }
#endif
            if (input == Vector2.zero)
            {
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");
            }
            return Vector2.ClampMagnitude(input, 1f);
        }

        private void ApplyVisualFeedback()
        {
            if (_visualRoot == null) return;

            // Visual crouch scale
            float targetYScale = _isCrouching ? 0.6f : 1.0f;
            Vector3 scale = _visualRoot.localScale;
            scale.y = Mathf.Lerp(scale.y, targetYScale, 15f * Time.deltaTime);
            _visualRoot.localScale = scale;

            // Visual punch extension
            if (_rightFist != null)
            {
                if (_state == CharacterActionState.Attacking)
                {
                    float punchPhase = Mathf.PingPong(_actionTimer * 6f, 1f);
                    _rightFist.localPosition = Vector3.forward * (punchPhase * 0.7f);
                }
                else if (_state == CharacterActionState.Guarding)
                {
                    _rightFist.localPosition = new Vector3(0.2f, 0.4f, 0.4f);
                }
                else
                {
                    _rightFist.localPosition = Vector3.Lerp(_rightFist.localPosition, Vector3.zero, 15f * Time.deltaTime);
                }
            }
        }
    }
}
