using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    /// <summary>
    /// 本機玩家握手輸入控制器：
    /// 採集 WASD 移動、Space 跳躍、E 鍵按住/放開互動意圖，並送入 HandshakeParticipant。
    /// 遵循 UI 焦點隔離規則，游標位於面板內時鎖定輸入。
    /// </summary>
    [RequireComponent(typeof(HandshakeParticipant))]
    public class HandshakeCharacter : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;
        [SerializeField] private bool _usePlayer2Scheme = false;

        private HandshakeParticipant _participant;

        public bool OverrideIntent { get; set; } = false;
        public CombatActionIntent CustomIntent { get; set; } = CombatActionIntent.Empty;

        public KeyCode InteractKey { get => _interactKey; set => _interactKey = value; }
        public HandshakeParticipant Participant => _participant;

        private void Awake()
        {
            _participant = GetComponent<HandshakeParticipant>();
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
            if (_participant == null) return;

            CombatActionIntent intent = OverrideIntent ? CustomIntent : PollIntent();
            _participant.StepParticipant(intent, _cameraTransform);
        }

        public CombatActionIntent PollIntent()
        {
            CombatActionIntent intent = CombatActionIntent.Empty;
            intent.Timestamp = Time.time;

            // R 鍵全場快速重設
            intent.GlobalReset = InputHandler.GetKeyDown(KeyCode.R);

            // 若面板取得焦點，停止採集互動與移動
            if (WhiteRoomCharacter.IsUIFocused)
            {
                return intent;
            }

            if (_usePlayer2Scheme)
            {
                // P2 控制：方向鍵 + Keypad 3 互動
                Vector2 move = Vector2.zero;
                if (InputHandler.GetKey(KeyCode.UpArrow)) move.y += 1f;
                if (InputHandler.GetKey(KeyCode.DownArrow)) move.y -= 1f;
                if (InputHandler.GetKey(KeyCode.LeftArrow)) move.x -= 1f;
                if (InputHandler.GetKey(KeyCode.RightArrow)) move.x += 1f;
                intent.MoveInput = Vector2.ClampMagnitude(move, 1f);

                intent.Jump = InputHandler.GetKeyDown(KeyCode.RightControl) || InputHandler.GetKeyDown(KeyCode.Keypad0);

                intent.InteractHold = InputHandler.GetKey(KeyCode.Keypad3);
                intent.InteractDown = InputHandler.GetKeyDown(KeyCode.Keypad3);
                intent.InteractUp = InputHandler.GetKeyUp(KeyCode.Keypad3);
            }
            else
            {
                // P1 控制：WASD + E 鍵互動
                Vector2 move = Vector2.zero;
                if (InputHandler.GetKey(KeyCode.W)) move.y += 1f;
                if (InputHandler.GetKey(KeyCode.S)) move.y -= 1f;
                if (InputHandler.GetKey(KeyCode.A)) move.x -= 1f;
                if (InputHandler.GetKey(KeyCode.D)) move.x += 1f;
                intent.MoveInput = Vector2.ClampMagnitude(move, 1f);

                intent.Jump = InputHandler.GetKeyDown(KeyCode.Space);

                intent.InteractHold = InputHandler.GetKey(_interactKey);
                intent.InteractDown = InputHandler.GetKeyDown(_interactKey);
                intent.InteractUp = InputHandler.GetKeyUp(_interactKey);
            }

            return intent;
        }

        public void SetControlScheme(bool isPlayer2)
        {
            _usePlayer2Scheme = isPlayer2;
        }
    }
}
