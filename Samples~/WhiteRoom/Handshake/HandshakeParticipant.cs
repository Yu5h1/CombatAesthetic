using System;
using UnityEngine;
using Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    /// <summary>
    /// 握手互動參與者實體：
    /// 本機玩家與 AI 對手皆具備此元件，遵守相同的操作合法性、狀態流轉與位移規則。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class HandshakeParticipant : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string _entityId = "Participant";
        [SerializeField] private int _teamId = 0; // 0: Player, 1: AI

        [Header("Locomotion")]
        [SerializeField] private float _walkSpeed = 5.0f;
        [SerializeField] private float _reachWalkSpeed = 2.2f; // 伸手/等待時接近對方的慢步速度
        [SerializeField] private float _turnSpeed = 720f;
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _gravity = -20f;

        [Header("Timings (依照 Handshake-Clips 規範)")]
        [SerializeField] private float _reachDuration = 0.50f;     // 伸手長度 0.5s
        [SerializeField] private float _withdrawDuration = 0.50f;  // 收手長度 0.5s

        private CharacterController _controller;
        private HandshakeState _state = HandshakeState.None;
        private float _stateTimer = 0f;
        private float _verticalVelocity = 0f;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private CombatActionIntent _currentIntent;

        public string EntityId => _entityId;
        public int TeamId => _teamId;
        public HandshakeState State => _state;
        public float StateTimer => _stateTimer;
        public CombatActionIntent CurrentIntent => _currentIntent;
        public Transform RightHandTransform { get; set; }
        public HandContactBridge ContactBridge { get; set; }

        public event Action<HandshakeParticipant, HandshakeState> OnStateChanged;
        public event Action<string> OnParticipantLog;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (_controller != null)
            {
                _controller.height = 2.0f;
                _controller.center = new Vector3(0, 1.0f, 0);
            }
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        private void Start()
        {
            if (RightHandTransform == null)
            {
                var fist = transform.Find("VisualRoot/RightArmRoot/RightFist");
                if (fist != null) SetRightHand(fist);
            }
        }

        public void Initialize(string id, int team)
        {
            _entityId = id;
            _teamId = team;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        public void SetRightHand(Transform handTransform)
        {
            RightHandTransform = handTransform;
            if (RightHandTransform != null)
            {
                var col = RightHandTransform.GetComponent<SphereCollider>();
                if (col == null)
                {
                    col = RightHandTransform.gameObject.AddComponent<SphereCollider>();
                }
                col.radius = 0.22f;
                col.isTrigger = true;

                ContactBridge = RightHandTransform.GetComponent<HandContactBridge>();
                if (ContactBridge == null)
                {
                    ContactBridge = RightHandTransform.gameObject.AddComponent<HandContactBridge>();
                }
                ContactBridge.Initialize(this);
            }
        }

        public void SetState(HandshakeState newState, float duration = 0f)
        {
            if (_state == newState) return;

            HandshakeState prev = _state;
            _state = newState;
            _stateTimer = duration;

            OnStateChanged?.Invoke(this, _state);
        }

        /// <summary>
        /// 單幀意圖推進：由本機玩家或 AI 驅動
        /// </summary>
        public void StepParticipant(CombatActionIntent intent, Transform cameraTransform = null)
        {
            _currentIntent = intent;

            // 1. 全場重設意圖處理
            if (intent.GlobalReset)
            {
                HandshakeArbitrator.Instance?.ResetAll();
                return;
            }

            // 2. 狀態機時序流轉
            UpdateHandshakeState(intent);

            // 3. 位移與跳躍推進
            HandleLocomotion(intent, cameraTransform);
        }

        private void UpdateHandshakeState(CombatActionIntent intent)
        {
            switch (_state)
            {
                case HandshakeState.None:
                    // 常態下按住互動（預設 E）：發起伸手
                    if (intent.InteractHold || intent.InteractDown)
                    {
                        SetState(HandshakeState.Reaching, _reachDuration);
                        OnParticipantLog?.Invoke($"[{_entityId}] 按住 E 鍵發起伸手（0.50s）");
                    }
                    break;

                case HandshakeState.Reaching:
                    // 伸手期間若放開互動鍵：取消收手
                    if (!intent.InteractHold)
                    {
                        SetState(HandshakeState.Withdrawing, _withdrawDuration);
                        OnParticipantLog?.Invoke($"[{_entityId}] 放開互動鍵，收手取消");
                    }
                    else
                    {
                        _stateTimer -= Time.deltaTime;
                        if (_stateTimer <= 0f)
                        {
                            // 伸手完畢，進入等待循環
                            SetState(HandshakeState.Waiting);
                            OnParticipantLog?.Invoke($"[{_entityId}] 伸手就緒，等待對方走近接觸");
                        }
                    }
                    break;

                case HandshakeState.Waiting:
                    // 等待期間若放開互動鍵：取消收手
                    if (!intent.InteractHold)
                    {
                        SetState(HandshakeState.Withdrawing, _withdrawDuration);
                        OnParticipantLog?.Invoke($"[{_entityId}] 放開互動鍵，等待中止並收手");
                    }
                    break;

                case HandshakeState.Grip:
                case HandshakeState.Shaking:
                    // Grip 與 Shaking 由 HandshakeArbitrator 權威判定
                    break;

                case HandshakeState.Withdrawing:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f)
                    {
                        SetState(HandshakeState.None);
                    }
                    break;
            }
        }

        private void HandleLocomotion(CombatActionIntent intent, Transform cameraTransform)
        {
            bool isGrounded = _controller.isGrounded;
            if (isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            // 在 Grip 或 Shaking 期間，角色固定於原地，不可位移
            bool canMove = (_state == HandshakeState.None || _state == HandshakeState.Withdrawing ||
                            _state == HandshakeState.Reaching || _state == HandshakeState.Waiting);

            if (_state == HandshakeState.Grip || _state == HandshakeState.Shaking)
            {
                canMove = false;
            }

            Vector3 moveDirection = Vector3.zero;

            if (canMove && intent.MoveInput.sqrMagnitude > 0.001f)
            {
                Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
                Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();

                moveDirection = (forward * intent.MoveInput.y + right * intent.MoveInput.x).normalized;

                // 面向移動方向
                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
                }
            }

            // 速度選擇：伸手或等待時為警戒慢步 (2.2 m/s)，常態為 5.0 m/s
            float currentSpeed = (_state == HandshakeState.Reaching || _state == HandshakeState.Waiting) ? _reachWalkSpeed : _walkSpeed;

            // 跳躍：只有在常態下可跳躍
            if (canMove && isGrounded && intent.Jump && _state == HandshakeState.None)
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }

            _verticalVelocity += _gravity * Time.deltaTime;

            Vector3 motion = (canMove ? moveDirection * currentSpeed : Vector3.zero) + Vector3.up * _verticalVelocity;
            _controller.Move(motion * Time.deltaTime);
        }

        public void ResetParticipant()
        {
            if (_controller != null) _controller.enabled = false;
            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;
            Physics.SyncTransforms();
            if (_controller != null) _controller.enabled = true;

            _verticalVelocity = 0f;
            _currentIntent = CombatActionIntent.Empty;
            SetState(HandshakeState.None);
        }
    }
}
