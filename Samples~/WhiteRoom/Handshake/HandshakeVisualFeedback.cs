using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    /// <summary>
    /// 握手視覺反饋與人形表現層（嚴格與判定分離）：
    /// 依據 HandshakeState 驅動機械手臂伸展、握緊、上下搖晃與平滑收回，
    /// 視鏡顏色提供即時時機反饋；IK 對齊僅調整畫面呈現，不干涉判定數值。
    /// </summary>
    public class HandshakeVisualFeedback : MonoBehaviour
    {
        [SerializeField] private HandshakeParticipant _participant;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _fistTransform;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private Renderer _visorRenderer;
        [SerializeField] private Renderer _torsoRenderer;

        private Vector3 _fistDefaultLocalPos;
        private Color _defaultVisorColor = new Color(0.1f, 0.7f, 1.0f);
        private Color _defaultTorsoColor = new Color(0.92f, 0.92f, 0.92f);

        private void Awake()
        {
            if (_participant == null) _participant = GetComponent<HandshakeParticipant>();
            if (_fistTransform != null) _fistDefaultLocalPos = _fistTransform.localPosition;
            if (_visorRenderer != null && _visorRenderer.material != null)
                _defaultVisorColor = _visorRenderer.material.color;
            if (_torsoRenderer != null && _torsoRenderer.material != null)
                _defaultTorsoColor = _torsoRenderer.material.color;
        }

        public void BindVisuals(Transform root, Transform fist, Transform head, Renderer visor, Renderer torso)
        {
            _visualRoot = root;
            _fistTransform = fist;
            _headTransform = head;
            _visorRenderer = visor;
            _torsoRenderer = torso;
            if (_fistTransform != null) _fistDefaultLocalPos = _fistTransform.localPosition;
            if (_visorRenderer != null && _visorRenderer.material != null)
                _defaultVisorColor = _visorRenderer.material.color;
            if (_torsoRenderer != null && _torsoRenderer.material != null)
                _defaultTorsoColor = _torsoRenderer.material.color;
        }

        private void Update()
        {
            if (_participant == null || _visualRoot == null) return;

            HandshakeState state = _participant.State;

            // 1. 手部位置與姿態流轉
            if (_fistTransform != null)
            {
                Vector3 targetLocalPos = _fistDefaultLocalPos;

                switch (state)
                {
                    case HandshakeState.None:
                        targetLocalPos = _fistDefaultLocalPos;
                        break;

                    case HandshakeState.Reaching:
                        // 伸手：手部前進伸出 0.65m，向內斜伸 0.30m 對齊身體中線
                        targetLocalPos = _fistDefaultLocalPos + new Vector3(-0.30f, 0.02f, 0.65f);
                        break;

                    case HandshakeState.Waiting:
                        // 等待：維持伸直姿態，附帶微弱自然呼吸微動
                        float breath = Mathf.Sin(Time.time * 2.5f) * 0.015f;
                        targetLocalPos = _fistDefaultLocalPos + new Vector3(-0.30f, 0.02f + breath, 0.65f);
                        break;

                    case HandshakeState.Grip:
                        // 握緊：雙手交扣
                        targetLocalPos = _fistDefaultLocalPos + new Vector3(-0.30f, 0.02f, 0.65f);
                        break;

                    case HandshakeState.Shaking:
                        // 搖手：雙手上下搖動（振幅 0.06m，頻率約 4Hz）
                        float shakeOffset = Mathf.Sin(Time.time * 25f) * 0.06f;
                        targetLocalPos = _fistDefaultLocalPos + new Vector3(-0.30f, 0.02f + shakeOffset, 0.65f);
                        break;

                    case HandshakeState.Withdrawing:
                        // 收手：平滑返回起始站姿
                        targetLocalPos = _fistDefaultLocalPos;
                        break;
                }

                _fistTransform.localPosition = Vector3.Lerp(_fistTransform.localPosition, targetLocalPos, 15f * Time.deltaTime);
            }

            // 2. 視鏡顏色反饋（時序與狀態識別）
            if (_visorRenderer != null && _visorRenderer.material != null)
            {
                Color targetColor = _defaultVisorColor;

                switch (state)
                {
                    case HandshakeState.Reaching:
                    case HandshakeState.Waiting:
                        targetColor = new Color(1f, 0.85f, 0.1f); // 黃色：發起與等待互動
                        break;

                    case HandshakeState.Grip:
                        targetColor = new Color(0.9f, 0.3f, 1.0f); // 亮紫：握緊判定時窗
                        break;

                    case HandshakeState.Shaking:
                        targetColor = new Color(0.2f, 1.0f, 0.4f); // 鮮綠：握手成功！
                        break;

                    case HandshakeState.Withdrawing:
                        targetColor = new Color(0.6f, 0.6f, 0.6f); // 灰色：收手中
                        break;

                    case HandshakeState.None:
                    default:
                        targetColor = _defaultVisorColor;
                        break;
                }

                _visorRenderer.material.color = Color.Lerp(_visorRenderer.material.color, targetColor, 12f * Time.deltaTime);
            }
        }
    }
}
