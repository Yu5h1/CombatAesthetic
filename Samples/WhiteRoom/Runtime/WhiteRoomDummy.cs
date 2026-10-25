using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public class WhiteRoomDummy : MonoBehaviour
    {
        [Header("Dummy Settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private bool _autoCounterAttack = true;
        [SerializeField] private float _counterAttackInterval = 4.0f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackDamage = 15f;

        [Header("Visual References")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _normalColor = new Color(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color _hitColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color _parriedColor = new Color(0.2f, 0.6f, 1f);

        private float _currentHealth;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _knockbackVelocity;
        private float _counterTimer;
        private float _flashTimer;
        private bool _isStunned;
        private float _stunTimer;
        private WhiteRoomCharacter _playerTarget;
        private CharacterController _controller;

        public float Health => _currentHealth;
        public float MaxHealth => _maxHealth;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _currentHealth = _maxHealth;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        private void Start()
        {
            _playerTarget = FindFirstObjectByType<WhiteRoomCharacter>();
            if (_renderer != null)
            {
                _renderer.material.color = _normalColor;
            }
        }

        private void Update()
        {
            // Knockback decay
            if (_knockbackVelocity.sqrMagnitude > 0.01f)
            {
                _knockbackVelocity = Vector3.Lerp(_knockbackVelocity, Vector3.zero, 6f * Time.deltaTime);
                if (_controller != null && _controller.enabled)
                {
                    _controller.Move(_knockbackVelocity * Time.deltaTime);
                }
                else
                {
                    transform.position += _knockbackVelocity * Time.deltaTime;
                }
            }

            // Stun recovery
            if (_isStunned)
            {
                _stunTimer -= Time.deltaTime;
                if (_stunTimer <= 0f)
                {
                    _isStunned = false;
                    SetColor(_normalColor);
                }
                return;
            }

            // Color flash recovery
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f && !_isStunned)
                {
                    SetColor(_normalColor);
                }
            }

            // Periodic counter-attack to let player test Guard / Parry
            if (_autoCounterAttack && _playerTarget != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTarget.transform.position);
                if (dist <= _attackRange * 1.5f)
                {
                    // Face player
                    Vector3 lookDir = (_playerTarget.transform.position - transform.position);
                    lookDir.y = 0;
                    if (lookDir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.LookRotation(lookDir);
                    }

                    _counterTimer += Time.deltaTime;
                    if (_counterTimer >= _counterAttackInterval)
                    {
                        _counterTimer = 0f;
                        PerformAttackOnPlayer();
                    }
                }
            }
        }

        private void PerformAttackOnPlayer()
        {
            if (_playerTarget == null) return;

            Vector3 attackDir = (_playerTarget.transform.position - transform.position).normalized;
            attackDir.y = 0.1f;
            Vector3 impulse = attackDir * 6f;

            _playerTarget.TakeHit(_attackDamage, impulse, transform.position);
        }

        public void TakeHit(float damage, Vector3 knockback)
        {
            _currentHealth = Mathf.Max(0f, _currentHealth - damage);
            _knockbackVelocity += knockback;
            _flashTimer = 0.15f;
            SetColor(_hitColor);
        }

        public void ApplyParryStun(float duration)
        {
            _isStunned = true;
            _stunTimer = duration;
            _counterTimer = 0f;
            SetColor(_parriedColor);
        }

        public void ResetDummy()
        {
            if (_controller != null) _controller.enabled = false;
            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;
            if (_controller != null) _controller.enabled = true;

            _currentHealth = _maxHealth;
            _knockbackVelocity = Vector3.zero;
            _isStunned = false;
            _counterTimer = 0f;
            SetColor(_normalColor);
        }

        private void SetColor(Color col)
        {
            if (_renderer != null && _renderer.material != null)
            {
                _renderer.material.color = col;
            }
        }
    }
}
