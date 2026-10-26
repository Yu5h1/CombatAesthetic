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

        private CombatEntity _entity;

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
                intent.GuardHold = InputHandler.GetKey(KeyCode.Keypad2) || InputHandler.GetKey(KeyCode.RightAlt);
                intent.ParryTap = InputHandler.GetKeyDown(KeyCode.Keypad2) || InputHandler.GetKeyDown(KeyCode.RightAlt);
                intent.GlobalReset = InputHandler.GetKeyDown(KeyCode.R);
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
                intent.GuardHold = InputHandler.GetMouseButton(1);
                intent.ParryTap = InputHandler.GetMouseButtonDown(1);
                intent.GlobalReset = InputHandler.GetKeyDown(KeyCode.R);
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
                CombatEntity dummyAttacker = FindFirstObjectByType<CombatEntity>();
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
