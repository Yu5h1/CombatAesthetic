using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public class WhiteRoomCamera : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset = new Vector3(0, 1.4f, 0);

        [Header("Orbit Settings")]
        [SerializeField] private float _distance = 4.0f;
        [SerializeField] private float _minDistance = 1.5f;
        [SerializeField] private float _maxDistance = 8.0f;
        [SerializeField] private float _mouseSensitivity = 2.0f;
        [SerializeField] private float _minPitch = -25f;
        [SerializeField] private float _maxPitch = 65f;
        [SerializeField] private float _smoothSpeed = 15f;

        private float _yaw = 0f;
        private float _pitch = 15f;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        private void Start()
        {
            Vector3 angles = transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;

            // Lock cursor for comfortable 3D camera orbit
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Toggle cursor lock with Left Alt or Escape
            bool toggleCursor = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.leftAltKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
                toggleCursor = true;
#endif
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.Escape))
                toggleCursor = true;

            if (toggleCursor)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // Only rotate when cursor is locked or middle mouse is held
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 mouseDelta = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null)
                {
                    mouseDelta = Mouse.current.delta.ReadValue() * 0.1f;
                }
#endif
                if (mouseDelta == Vector2.zero)
                {
                    mouseDelta.x = Input.GetAxis("Mouse X");
                    mouseDelta.y = Input.GetAxis("Mouse Y");
                }

                _yaw += mouseDelta.x * _mouseSensitivity;
                _pitch -= mouseDelta.y * _mouseSensitivity;
                _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 pivot = _target.position + _targetOffset;
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desiredPosition = pivot - (rotation * Vector3.forward * _distance);

            // Simple camera collision prevention against floor/walls
            if (Physics.Raycast(pivot, desiredPosition - pivot, out RaycastHit hit, _distance))
            {
                if (hit.transform != _target)
                {
                    desiredPosition = hit.point + hit.normal * 0.2f;
                }
            }

            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
            transform.LookAt(pivot);
        }
    }
}
