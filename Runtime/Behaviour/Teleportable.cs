using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

namespace Yu5h1Lib
{
    public class Teleportable : BaseMonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        public SpriteRenderer spriteRenderer => _spriteRenderer;

        public bool IsTeleporting { get; private set; }

        [SerializeField]
        private UnityEvent _begin;
        public event UnityAction begin
        {
            add => _begin.AddListener(value);
            remove => _begin.RemoveListener(value);
        }

        [SerializeField]
        private UnityEvent _end;
        public event UnityAction end
        {
            add => _end.AddListener(value);
            remove => _end.RemoveListener(value);
        }
        protected override void OnInitializing()
        {
            TryGetComponent(out _spriteRenderer);
            if (_begin == null)
                _begin = new UnityEvent();
            if (_end == null)
                _end = new UnityEvent();
        }

        private Coroutine coroutine;

        void Start() { }

        public bool TeleportTo(Vector2 destination, Quaternion? rot = null,UnityAction arrived = null)
            => TeleportTo(destination, 0, rot, arrived);
        public bool TeleportTo(Vector2 destination, float fadeDuration ,Quaternion? rot = null, UnityAction arrived = null)
        {
            if (!gameObject.IsBelongToActiveScene() || IsTeleporting)
                return false;
            this.StartCoroutine(ref coroutine, TeleportRoutine(destination, fadeDuration, rot, arrived));
            return true;
        }

        private IEnumerator TeleportRoutine(Vector2 destination, float fadeDuration, Quaternion? rot = null, UnityAction arrived = null)
        {
            IsTeleporting = true;
    
            if (fadeDuration > 0)
                yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
            _begin?.Invoke();
            transform.position = destination;
            if (rot.HasValue)
                transform.rotation = rot.Value;
            arrived?.Invoke();
            _end?.Invoke();
            if (fadeDuration > 0)
                yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
            IsTeleporting = false;
        }
        private IEnumerator Fade(float from, float to, float fadeDuration)
        {
            float elapsed = 0f;
            Color color = spriteRenderer.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                spriteRenderer.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            spriteRenderer.color = new Color(color.r, color.g, color.b, to);
        }
    }
}