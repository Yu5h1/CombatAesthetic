using System;
using UnityEngine;
using Yu5h1Lib;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    [RequireComponent(typeof(CombatEntity))]
    public class WhiteRoomCharacter : MonoBehaviour, ICombatInputSource
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private bool _usePlayer2ControlScheme = false;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;

        public KeyCode InteractKey { get => _interactKey; set => _interactKey = value; }

        private CombatEntity _entity;

        // UI 互動焦點：當滑鼠在除錯面板操作時，鎖定戰鬥輸入，避免點擊面板誤觸發出招或防禦
        public static bool IsUIFocused { get; set; } = false;

        // 防禦與彈刀時序識別（點按 vs 長按）
        private float _p1RmbDownTime = -1f;
        private bool _p1RmbWasDown = false;

        private float _p2GuardDownTime = -1f;
        private bool _p2GuardWasDown = false;

        private const float ParryTapThreshold = 0.22f; // 快速按下並放開（<=0.22s）判定為精準彈刀
        private const float GuardHoldThreshold = 0.16f; // 長按（>0.16s）進入常態防禦姿態

        public CombatEntity Entity => _entity;
        public CombatState CurrentState => _entity != null ? _entity.State : CombatState.Idle;
        public bool IsCrouching => _entity != null && _entity.IsCrouching;
        public bool IsGuarding => _entity != null && _entity.IsGuarding;
        public float Health => _entity != null ? _entity.CurrentHealth : 0f;
        public float MaxHealth => _entity != null ? _entity.MaxHealth : 100f;

        public event Action<string> OnCombatAction
        {
            add { if (_entity != null) _entity.OnCombatLog += value; }
            remove { if (_entity != null) _entity.OnCombatLog -= value; }
        }

        private void Awake()
        {
            _entity = GetComponent<CombatEntity>();
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
            if (_entity == null) return;

            CombatActionIntent intent = PollIntent(_entity);
            _entity.StepCombat(intent, _cameraTransform);
        }

        public CombatActionIntent PollIntent(CombatEntity entity)
        {
            CombatActionIntent intent = CombatActionIntent.Empty;
            intent.Timestamp = Time.time;

            // 全場統一重設隨時可由 R 鍵觸發
            intent.GlobalReset = InputHandler.GetKeyDown(KeyCode.R);

            // 若面板取得操作焦點，停止採集戰鬥與位移指令
            if (IsUIFocused)
            {
                return intent;
            }

            if (_usePlayer2ControlScheme)
            {
                // Player 2 控制方案（同機雙人測試驗證：方向鍵 + 右側數字鍵）
                Vector2 move = Vector2.zero;
                if (InputHandler.GetKey(KeyCode.UpArrow)) move.y += 1f;
                if (InputHandler.GetKey(KeyCode.DownArrow)) move.y -= 1f;
                if (InputHandler.GetKey(KeyCode.LeftArrow)) move.x -= 1f;
                if (InputHandler.GetKey(KeyCode.RightArrow)) move.x += 1f;
                intent.MoveInput = Vector2.ClampMagnitude(move, 1f);

                intent.Jump = InputHandler.GetKeyDown(KeyCode.RightControl) || InputHandler.GetKeyDown(KeyCode.Keypad0);
                intent.CrouchHold = InputHandler.GetKey(KeyCode.DownArrow) && InputHandler.GetKey(KeyCode.RightShift);
                intent.CrouchToggle = InputHandler.GetKeyDown(KeyCode.KeypadPeriod);
                intent.Attack = InputHandler.GetKeyDown(KeyCode.Keypad1) || InputHandler.GetKeyDown(KeyCode.Return);

                // P2 互動按鍵（Keypad 3）
                intent.InteractHold = InputHandler.GetKey(KeyCode.Keypad3);
                intent.InteractDown = InputHandler.GetKeyDown(KeyCode.Keypad3);
                intent.InteractUp = InputHandler.GetKeyUp(KeyCode.Keypad3);

                // P2 防禦採集（Keypad 2 或 Right Alt）：區分點按彈刀 vs 長按格擋
                bool p2GuardKey = InputHandler.GetKey(KeyCode.Keypad2) || InputHandler.GetKey(KeyCode.RightAlt);
                bool p2GuardKeyDown = InputHandler.GetKeyDown(KeyCode.Keypad2) || InputHandler.GetKeyDown(KeyCode.RightAlt);
                bool p2GuardKeyUp = InputHandler.GetKeyUp(KeyCode.Keypad2) || InputHandler.GetKeyUp(KeyCode.RightAlt);

                if (p2GuardKeyDown)
                {
                    _p2GuardDownTime = Time.time;
                    _p2GuardWasDown = true;
                }

                if (p2GuardKeyUp && _p2GuardWasDown)
                {
                    if (Time.time - _p2GuardDownTime <= ParryTapThreshold)
                    {
                        intent.ParryTap = true; // 點按彈刀
                    }
                    _p2GuardWasDown = false;
                    _p2GuardDownTime = -1f;
                }
                else if (p2GuardKey && _p2GuardWasDown)
                {
                    if (Time.time - _p2GuardDownTime > GuardHoldThreshold)
                    {
                        intent.GuardHold = true; // 長按格擋
                    }
                }
            }
            else
            {
                // Player 1 預設控制方案（WASD + 鍵鼠）
                Vector2 move = Vector2.zero;
                if (InputHandler.GetKey(KeyCode.W)) move.y += 1f;
                if (InputHandler.GetKey(KeyCode.S)) move.y -= 1f;
                if (InputHandler.GetKey(KeyCode.A)) move.x -= 1f;
                if (InputHandler.GetKey(KeyCode.D)) move.x += 1f;
                intent.MoveInput = Vector2.ClampMagnitude(move, 1f);

                intent.Jump = InputHandler.GetKeyDown(KeyCode.Space);
                // UX 規範：Ctrl 按住為維持蹲下，放開自動起身；C 鍵為切換蹲下
                intent.CrouchHold = InputHandler.GetKey(KeyCode.LeftControl);
                intent.CrouchToggle = InputHandler.GetKeyDown(KeyCode.C);

                intent.Attack = InputHandler.GetMouseButtonDown(0);

                // P1 互動按鍵（預設 E 鍵）
                intent.InteractHold = InputHandler.GetKey(_interactKey);
                intent.InteractDown = InputHandler.GetKeyDown(_interactKey);
                intent.InteractUp = InputHandler.GetKeyUp(_interactKey);

                // P1 防禦採集（右鍵）：嚴格區分點按彈刀 vs 長按格擋
                bool rmbKey = InputHandler.GetMouseButton(1);
                bool rmbDown = InputHandler.GetMouseButtonDown(1);
                bool rmbUp = InputHandler.GetMouseButtonUp(1);

                if (rmbDown)
                {
                    _p1RmbDownTime = Time.time;
                    _p1RmbWasDown = true;
                }

                if (rmbUp && _p1RmbWasDown)
                {
                    // 快速按下放開（<=0.22s）：觸發精準彈刀 (Parry)
                    if (Time.time - _p1RmbDownTime <= ParryTapThreshold)
                    {
                        intent.ParryTap = true;
                    }
                    _p1RmbWasDown = false;
                    _p1RmbDownTime = -1f;
                }
                else if (rmbKey && _p1RmbWasDown)
                {
                    // 按住超過點按時限：進入普通防禦姿態 (GuardHold)
                    if (Time.time - _p1RmbDownTime > GuardHoldThreshold)
                    {
                        intent.GuardHold = true;
                    }
                }
            }

            return intent;
        }

        public void SetControlScheme(bool isPlayer2)
        {
            _usePlayer2ControlScheme = isPlayer2;
        }

        public void TakeHit(float damage, Vector3 knockback, Vector3 attackerPos)
        {
            // 向後相容或由假人觸發
            if (_entity != null)
            {
                CombatEntity dummyAttacker = FindAnyObjectByType<CombatEntity>();
                _entity.TakeHit(dummyAttacker, damage, knockback);
            }
        }

        public void ResetCharacter()
        {
            if (_entity != null)
            {
                _entity.ResetEntity();
            }
        }
    }
}
