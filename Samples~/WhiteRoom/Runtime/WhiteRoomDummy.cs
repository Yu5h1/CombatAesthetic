using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    /// <summary>
    /// WhiteRoom 訓練假人 / AI 傀儡：
    /// 與本機玩家、遠端玩家共用相同的 CombatEntity 戰鬥判定與操作合法性規則。
    /// </summary>
    [RequireComponent(typeof(CombatEntity))]
    public class WhiteRoomDummy : MonoBehaviour, ICombatInputSource
    {
        [Header("AI Behavior Settings")]
        [SerializeField] private bool _autoEngage = true;
        [SerializeField] private float _attackInterval = 3.5f;
        [SerializeField] private float _desiredDistance = 1.4f;

        private CombatEntity _entity;
        private CombatEntity _targetPlayer;
        private float _actionCooldown = 1.5f;

        public CombatEntity Entity => _entity;
        public float Health => _entity != null ? _entity.CurrentHealth : 0f;
        public float MaxHealth => _entity != null ? _entity.MaxHealth : 100f;
        public bool IsStunned => _entity != null && _entity.State == CombatState.Stunned;
        public bool IsIncapacitated => _entity != null && _entity.IsIncapacitated;

        private void Awake()
        {
            _entity = GetComponent<CombatEntity>();
        }

        private void Start()
        {
            FindTargetPlayer();
        }

        private void Update()
        {
            if (_entity == null) return;

            if (_targetPlayer == null || _targetPlayer.IsIncapacitated)
            {
                FindTargetPlayer();
            }

            CombatActionIntent intent = PollIntent(_entity);
            _entity.StepCombat(intent);
        }

        public CombatActionIntent PollIntent(CombatEntity entity)
        {
            CombatActionIntent intent = CombatActionIntent.Empty;
            intent.Timestamp = Time.time;

            if (!_autoEngage || _targetPlayer == null || _targetPlayer.IsIncapacitated)
            {
                return intent;
            }

            Vector3 toTarget = _targetPlayer.transform.position - transform.position;
            toTarget.y = 0;
            float distance = toTarget.magnitude;

            // 1. 移動逼近目標
            if (distance > _desiredDistance)
            {
                Vector3 moveDir = toTarget.normalized;
                intent.MoveInput = new Vector2(moveDir.x, moveDir.z);
            }
            else
            {
                // 已在出招射程內，停止位移並保持面向玩家
                intent.MoveInput = Vector2.zero;
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        Quaternion.LookRotation(toTarget),
                        360f * Time.deltaTime
                    );
                }

                // 2. 週期性出招進攻（經過 Startup 前搖蓄力 -> Active 打擊 -> Recovery 收招）
                _actionCooldown -= Time.deltaTime;
                if (_actionCooldown <= 0f)
                {
                    _actionCooldown = _attackInterval;
                    intent.Attack = true;
                }
            }

            return intent;
        }

        private void FindTargetPlayer()
        {
            foreach (var e in CombatEntity.ActiveEntities)
            {
                if (e != null && e.TeamId == 0 && !e.IsIncapacitated)
                {
                    _targetPlayer = e;
                    break;
                }
            }
        }

        public void TakeHit(float damage, Vector3 knockback)
        {
            if (_entity != null)
            {
                CombatEntity attacker = _targetPlayer;
                _entity.TakeHit(attacker, damage, knockback);
            }
        }

        public void ApplyParryStun(float duration)
        {
            if (_entity != null)
            {
                _entity.InterruptAttack(duration);
            }
        }

        public void ResetDummy()
        {
            if (_entity != null)
            {
                _entity.ResetEntity();
                _actionCooldown = 2.0f;
            }
        }
    }
}
