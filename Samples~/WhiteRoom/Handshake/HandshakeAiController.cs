using UnityEngine;
using Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    public enum HandshakeAiMode
    {
        Idle = 0,
        AiInitiator = 1,       // 情境 1: AI 主動發起伸手並等待，玩家走近回應
        AiResponder = 2,       // 情境 2: 玩家發起，AI 伸手並主動走近回應
        AiCancel = 3,          // 情境 4: AI 發起伸手後，主動放開 E 取消
        AiEarlyRelease = 4     // 情境 5: 握緊期間放開（第 2 批情況）
    }

    /// <summary>
    /// AI 握手對手控制器：
    /// 與玩家走同一套規則，僅產出移動意圖與按住/放開 E 的意圖，送交 HandshakeParticipant。
    /// 支援可設定的發起、取消、走近回應與握緊期間放開情境。
    /// </summary>
    [RequireComponent(typeof(HandshakeParticipant))]
    public class HandshakeAiController : MonoBehaviour
    {
        [Header("AI Scenario Mode")]
        [SerializeField] private HandshakeAiMode _mode = HandshakeAiMode.AiInitiator;
        [SerializeField] private float _delayBeforeAction = 0.8f;
        [SerializeField] private float _desiredDistance = 0.85f;
        [SerializeField] private bool _shouldWalkIn = true;

        private HandshakeParticipant _participant;
        private HandshakeParticipant _targetPlayer;
        private float _timer = 0f;
        private bool _isHoldingInteract = false;

        public HandshakeAiMode Mode { get => _mode; set { _mode = value; _timer = 0f; _isHoldingInteract = false; } }

        private void Awake()
        {
            _participant = GetComponent<HandshakeParticipant>();
        }

        private void Start()
        {
            FindTargetPlayer();
        }

        private void Update()
        {
            if (_participant == null) return;
            if (_targetPlayer == null) FindTargetPlayer();

            CombatActionIntent intent = PollAiIntent();
            _participant.StepParticipant(intent);
        }

        private void FindTargetPlayer()
        {
            var participants = FindObjectsByType<HandshakeParticipant>();
            foreach (var p in participants)
            {
                if (p != _participant && p.TeamId == 0)
                {
                    _targetPlayer = p;
                    break;
                }
            }
        }

        private CombatActionIntent PollAiIntent()
        {
            CombatActionIntent intent = CombatActionIntent.Empty;
            intent.Timestamp = Time.time;

            if (_targetPlayer == null) return intent;

            _timer += Time.deltaTime;

            Vector3 toTarget = _targetPlayer.transform.position - transform.position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            Vector3 targetDir = distance > 0.001f ? toTarget.normalized : transform.forward;

            switch (_mode)
            {
                case HandshakeAiMode.Idle:
                    _isHoldingInteract = false;
                    break;

                case HandshakeAiMode.AiInitiator:
                    // AI 等待 delay 後，主動伸手並按住等待玩家走近
                    if (_timer >= _delayBeforeAction)
                    {
                        _isHoldingInteract = true;
                    }
                    // 發起方留在原地等待，不主動走近
                    break;

                case HandshakeAiMode.AiResponder:
                    // 偵測玩家是否正在伸手或等待
                    bool playerReaching = (_targetPlayer.State == HandshakeState.Reaching || _targetPlayer.State == HandshakeState.Waiting);
                    if (playerReaching)
                    {
                        _isHoldingInteract = true;
                        // 回應方主動走向玩家，直到距離符合握手門檻
                        if (_shouldWalkIn && distance > _desiredDistance && (_participant.State == HandshakeState.Reaching || _participant.State == HandshakeState.Waiting))
                        {
                            Vector3 moveDir = toTarget.normalized;
                            intent.MoveInput = new Vector2(moveDir.x, moveDir.z);
                        }
                    }
                    else
                    {
                        if (_participant.State == HandshakeState.Reaching || _participant.State == HandshakeState.Waiting)
                        {
                            _isHoldingInteract = false; // 玩家未伸手，AI 不按住
                        }
                    }
                    break;

                case HandshakeAiMode.AiCancel:
                    // AI 伸手後放開取消
                    if (_timer >= _delayBeforeAction && _timer < _delayBeforeAction + 1.2f)
                    {
                        _isHoldingInteract = true;
                    }
                    else if (_timer >= _delayBeforeAction + 1.2f)
                    {
                        _isHoldingInteract = false; // 放開 E，觸發取消收手
                    }
                    break;

                case HandshakeAiMode.AiEarlyRelease:
                    // AI 先主動參與，但在握緊 (Grip) 期間放開 E（觸發第 2 批情況）
                    if (_participant.State == HandshakeState.Grip)
                    {
                        if (_timer >= 0.15f)
                        {
                            _isHoldingInteract = false; // 握緊期間放開
                        }
                    }
                    else
                    {
                        _isHoldingInteract = true;
                        _timer = 0f; // 重置 Grip 內計時
                    }
                    break;
            }

            intent.InteractHold = _isHoldingInteract;
            return intent;
        }

        public void ResetAI()
        {
            _timer = 0f;
            _isHoldingInteract = false;
        }
    }
}
