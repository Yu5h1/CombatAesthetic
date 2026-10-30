using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    /// <summary>
    /// 握手判定元件（Arbitrator）：
    /// 依照「判定與表現分開」原則，由本元件全權裁定「雙手碰到」、「握手成功」、「中途放開收手」等正式結果。
    /// 玩家與 AI 僅送出移動與按住/放開互動按鍵意圖，視覺表現（IK 對準與位移）不干預正式判定。
    /// </summary>
    public class HandshakeArbitrator : MonoBehaviour
    {
        private static HandshakeArbitrator s_Instance;
        public static HandshakeArbitrator Instance
        {
            get
            {
                if (s_Instance == null) s_Instance = ObjectUtility.FindObject<HandshakeArbitrator>(false);
                return s_Instance;
            }
            private set => s_Instance = value;
        }

        [Header("Timing Parameters (依 Handshake-Clips 規範)")]
        [SerializeField] private float _gripDuration = 0.50f;  // 握緊時間窗 0.50s (15影格@30fps)
        [SerializeField] private float _shakeDuration = 0.50f; // 搖一下時間 0.50s
        [SerializeField] private float _contactDistanceThreshold = 0.65f; // 手部幾何接觸門檻
        [SerializeField] private float _contactBodyDistanceThreshold = 1.05f; // 骨盆中心接觸門檻 (標準 0.8m)

        private readonly List<HandshakeParticipant> _participants = new List<HandshakeParticipant>();
        private HandshakeParticipant _participantA;
        private HandshakeParticipant _participantB;

        private HandshakeState _sessionState = HandshakeState.None;
        private float _sessionTimer = 0f;
        private HandshakeOutcome _lastOutcome = HandshakeOutcome.None;

        public HandshakeState SessionState => _sessionState;
        public HandshakeOutcome LastOutcome => _lastOutcome;
        public HandshakeParticipant ParticipantA => _participantA;
        public HandshakeParticipant ParticipantB => _participantB;

        public event Action<string> OnArbitrationLog;
        public event Action<HandshakeOutcome> OnHandshakeFinished;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void RegisterParticipant(HandshakeParticipant participant)
        {
            if (participant == null || _participants.Contains(participant)) return;

            _participants.Add(participant);
            participant.OnParticipantLog += msg => OnArbitrationLog?.Invoke(msg);

            if (participant.ContactBridge != null)
            {
                participant.ContactBridge.OnTriggerContact += OnHandTriggerContact;
            }
        }

        public void UnregisterParticipant(HandshakeParticipant participant)
        {
            if (participant == null) return;
            _participants.Remove(participant);
            if (participant.ContactBridge != null)
            {
                participant.ContactBridge.OnTriggerContact -= OnHandTriggerContact;
            }
        }

        private void OnHandTriggerContact(HandContactBridge otherBridge)
        {
            if (_sessionState != HandshakeState.None) return;

            // 尋找兩個參與者
            HandshakeParticipant p1 = null;
            HandshakeParticipant p2 = otherBridge.Owner;

            foreach (var p in _participants)
            {
                if (p != p2 && p.ContactBridge != null)
                {
                    p1 = p;
                    break;
                }
            }

            if (p1 != null && p2 != null)
            {
                TryInitiateGrip(p1, p2);
            }
        }

        private void Update()
        {
            // 1. 若無活躍 Session，執行幾何接觸輔助偵測（確保物理 Trigger 與距離均能精確觸發）
            if (_sessionState == HandshakeState.None && _participants.Count >= 2)
            {
                CheckHandProximityContact();
            }

            // 2. 活躍 Session 推進
            if (_sessionState == HandshakeState.Grip)
            {
                UpdateGripState();
            }
            else if (_sessionState == HandshakeState.Shaking)
            {
                UpdateShakingState();
            }
        }

        private void CheckHandProximityContact()
        {
            for (int i = 0; i < _participants.Count; i++)
            {
                for (int j = i + 1; j < _participants.Count; j++)
                {
                    var p1 = _participants[i];
                    var p2 = _participants[j];

                    if (p1 == null || p2 == null) continue;

                    // 雙方必須皆處於伸手 (Reaching) 或等待 (Waiting) 姿態
                    bool p1Ready = (p1.State == HandshakeState.Reaching || p1.State == HandshakeState.Waiting);
                    bool p2Ready = (p2.State == HandshakeState.Reaching || p2.State == HandshakeState.Waiting);

                    if (p1Ready && p2Ready)
                    {
                        float bodyDist = Vector3.Distance(p1.transform.position, p2.transform.position);
                        bool handTouch = false;

                        if (p1.RightHandTransform != null && p2.RightHandTransform != null)
                        {
                            float handDist = Vector3.Distance(p1.RightHandTransform.position, p2.RightHandTransform.position);
                            if (handDist <= _contactDistanceThreshold) handTouch = true;
                        }

                        if (handTouch || bodyDist <= _contactBodyDistanceThreshold)
                        {
                            TryInitiateGrip(p1, p2);
                            return;
                        }
                    }
                }
            }
        }

        private void TryInitiateGrip(HandshakeParticipant p1, HandshakeParticipant p2)
        {
            if (_sessionState != HandshakeState.None) return;

            bool p1Ready = (p1.State == HandshakeState.Reaching || p1.State == HandshakeState.Waiting);
            bool p2Ready = (p2.State == HandshakeState.Reaching || p2.State == HandshakeState.Waiting);

            if (!p1Ready || !p2Ready) return;

            _participantA = p1;
            _participantB = p2;

            _sessionState = HandshakeState.Grip;
            _sessionTimer = _gripDuration;

            _participantA.SetState(HandshakeState.Grip, _gripDuration);
            _participantB.SetState(HandshakeState.Grip, _gripDuration);

            OnArbitrationLog?.Invoke($"[握手判定] 雙手接觸（碰到）！進入握緊階段（{_gripDuration:F2}s 判定時窗）");
        }

        private void UpdateGripState()
        {
            if (_participantA == null || _participantB == null)
            {
                _sessionState = HandshakeState.None;
                return;
            }

            // 握緊期間檢查雙方是否仍按住互動鍵
            bool aHolding = _participantA.CurrentIntent.InteractHold;
            bool bHolding = _participantB.CurrentIntent.InteractHold;

            if (!aHolding || !bHolding)
            {
                // 握緊期間有人放開：依交辦規範屬第 2 批情況，本批直接收手並記錄標明
                _lastOutcome = HandshakeOutcome.EarlyReleaseDuringGrip;
                string who = !aHolding && !bHolding ? "雙方" : (!aHolding ? _participantA.EntityId : _participantB.EntityId);
                OnArbitrationLog?.Invoke($"[握手判定] 握緊期間【{who}】放開互動（屬第 2 批情況），雙方直接收手返回。");

                _participantA.SetState(HandshakeState.Withdrawing, 0.50f);
                _participantB.SetState(HandshakeState.Withdrawing, 0.50f);

                _sessionState = HandshakeState.None;
                OnHandshakeFinished?.Invoke(_lastOutcome);
                return;
            }

            _sessionTimer -= Time.deltaTime;
            if (_sessionTimer <= 0f)
            {
                // 握緊時窗結束且雙方全程按住：握手成功！
                _lastOutcome = HandshakeOutcome.Success;
                OnArbitrationLog?.Invoke($"[握手判定] ★ 握手成功！雙手緊扣，開始搖一下（{_shakeDuration:F2}s）。");

                _sessionState = HandshakeState.Shaking;
                _sessionTimer = _shakeDuration;

                _participantA.SetState(HandshakeState.Shaking, _shakeDuration);
                _participantB.SetState(HandshakeState.Shaking, _shakeDuration);
            }
        }

        private void UpdateShakingState()
        {
            _sessionTimer -= Time.deltaTime;
            if (_sessionTimer <= 0f)
            {
                OnArbitrationLog?.Invoke("[握手判定] 搖手結束，雙方收手返回常態姿態。");

                if (_participantA != null) _participantA.SetState(HandshakeState.Withdrawing, 0.50f);
                if (_participantB != null) _participantB.SetState(HandshakeState.Withdrawing, 0.50f);

                _sessionState = HandshakeState.None;
                OnHandshakeFinished?.Invoke(_lastOutcome);
            }
        }

        public void ResetAll()
        {
            _sessionState = HandshakeState.None;
            _sessionTimer = 0f;
            _lastOutcome = HandshakeOutcome.None;

            foreach (var p in _participants)
            {
                if (p != null) p.ResetParticipant();
            }

            var ai = FindAnyObjectByType<HandshakeAiController>();
            if (ai != null) ai.ResetAI();

            OnArbitrationLog?.Invoke("【全場快速重設】握手狀態、角色位置及 AI 設定已還原至初始配置。");
        }
    }
}
