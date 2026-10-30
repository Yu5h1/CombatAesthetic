using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    /// <summary>
    /// 握手互動自動化驗證執行器：
    /// 在 Unity PlayMode 實機執行五大核心驗收案例：
    /// 1. 玩家發起後放開（取消收手）
    /// 2. AI 發起、玩家走近回應、握緊至結束（成功）
    /// 3. 玩家發起、AI 走近回應（對調成功）
    /// 4. 握緊期間有人放開（第 2 批情況，直接收手）
    /// 5. AI 發起後放開（AI 取消）
    /// 6. 全場統一快速重設無殘留
    /// </summary>
    public class HandshakeVerificationRunner : MonoBehaviour
    {
        public bool IsFinished { get; private set; } = false;
        public string TestReport { get; private set; } = "";
        public bool AllPassed { get; private set; } = false;

        private readonly List<string> _results = new List<string>();
        private HandshakeCharacter _player;
        private HandshakeAiController _ai;
        private HandshakeArbitrator _arb;

        private bool _case1Pass;
        private bool _case2Pass;
        private bool _case3Pass;
        private bool _case4Pass;
        private bool _case5Pass;
        private bool _case6Pass;

        private void Start()
        {
            StartCoroutine(RunAllTestsRoutine());
        }

        private IEnumerator RunAllTestsRoutine()
        {
            _player = FindAnyObjectByType<HandshakeCharacter>();
            _ai = FindAnyObjectByType<HandshakeAiController>();
            _arb = HandshakeArbitrator.Instance;

            _player.OverrideIntent = true;

            yield return new WaitForSeconds(0.5f);

            _results.Add("======================================================================");
            _results.Add("  WhiteRoom 握手互動系統 Stage 2b 自動化驗證紀錄 (Unity 6000.6.0f1)");
            _results.Add("======================================================================");

            yield return StartCoroutine(Case1_PlayerCancel());
            yield return StartCoroutine(Case2_AiInitiate_PlayerRespond_Success());
            yield return StartCoroutine(Case3_PlayerInitiate_AiRespond_Success());
            yield return StartCoroutine(Case4_EarlyReleaseDuringGrip());
            yield return StartCoroutine(Case5_AiCancel());
            yield return StartCoroutine(Case6_ResetConsistency());

            AllPassed = _case1Pass && _case2Pass && _case3Pass && _case4Pass && _case5Pass && _case6Pass;
            _results.Add("======================================================================");
            _results.Add($"  總體驗收結果: {(AllPassed ? "★ ALL PASSED (全數通過)" : "FAILED (有未達項目)")}");
            _results.Add("======================================================================");

            TestReport = string.Join("\n", _results);
            IsFinished = true;
            _player.OverrideIntent = false;
            Debug.Log(TestReport);
        }

        private IEnumerator Case1_PlayerCancel()
        {
            _arb.ResetAll();
            _ai.Mode = HandshakeAiMode.Idle;
            _player.CustomIntent = CombatActionIntent.Empty;
            yield return new WaitForSeconds(0.2f);

            var p = _player.Participant;

            // 1. 按住 E 發起伸手
            var intent = CombatActionIntent.Empty;
            intent.InteractHold = true;
            intent.InteractDown = true;
            _player.CustomIntent = intent;
            yield return new WaitForSeconds(0.15f);

            bool reaching = (p.State == HandshakeState.Reaching);

            // 2. 放開 E 取消
            intent.InteractHold = false;
            intent.InteractDown = false;
            intent.InteractUp = true;
            _player.CustomIntent = intent;
            yield return new WaitForSeconds(0.1f);

            bool withdrawing = (p.State == HandshakeState.Withdrawing);

            // 3. 等待收手完成
            intent.InteractUp = false;
            _player.CustomIntent = intent;
            yield return new WaitForSeconds(0.6f);
            bool backToNone = (p.State == HandshakeState.None);

            _case1Pass = reaching && withdrawing && backToNone;
            _results.Add($"[案例 1] 玩家發起伸手後放開 E（取消）：{(_case1Pass ? "PASS" : "FAIL")}");
            _results.Add($"         - 伸手狀態: {reaching}, 收手過渡: {withdrawing}, 最終站立: {backToNone}");
        }

        private IEnumerator Case2_AiInitiate_PlayerRespond_Success()
        {
            _arb.ResetAll();
            _ai.Mode = HandshakeAiMode.AiInitiator;
            _player.CustomIntent = CombatActionIntent.Empty;

            var p = _player.Participant;
            var aiP = _ai.GetComponent<HandshakeParticipant>();

            // 等待 AI 伸手並進入 Waiting
            float waitTimer = 0f;
            while (aiP.State != HandshakeState.Waiting && waitTimer < 2.5f)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }

            bool aiWaiting = (aiP.State == HandshakeState.Waiting);

            // 玩家按住 E 並向前走近 AI
            float timeout = 4.0f;
            float elapsed = 0f;
            bool reachedGrip = false;

            while (elapsed < timeout && !reachedGrip)
            {
                var intent = CombatActionIntent.Empty;
                intent.InteractHold = true;
                intent.MoveInput = new Vector2(0f, 1f); // 走近
                _player.CustomIntent = intent;

                if (_arb.SessionState == HandshakeState.Grip)
                {
                    reachedGrip = true;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 握緊期間雙方持續按住 E，直到握手成功進入 Shaking
            float gripElapsed = 0f;
            bool reachedShaking = false;
            while (gripElapsed < 1.2f)
            {
                var intent = CombatActionIntent.Empty;
                intent.InteractHold = true;
                _player.CustomIntent = intent;

                if (_arb.SessionState == HandshakeState.Shaking)
                {
                    reachedShaking = true;
                }
                gripElapsed += Time.deltaTime;
                yield return null;
            }

            bool outcomeSuccess = (_arb.LastOutcome == HandshakeOutcome.Success);

            // 等待搖手結束收手
            yield return new WaitForSeconds(0.8f);

            _case2Pass = aiWaiting && reachedGrip && reachedShaking && outcomeSuccess;
            _results.Add($"[案例 2] AI 發起、玩家走近回應、握緊至結束（成功）：{(_case2Pass ? "PASS" : "FAIL")}");
            _results.Add($"         - AI 等待: {aiWaiting}, 雙手碰到握緊: {reachedGrip}, 進入搖手: {reachedShaking}, 正式結果: {_arb.LastOutcome}");
        }

        private IEnumerator Case3_PlayerInitiate_AiRespond_Success()
        {
            _arb.ResetAll();
            _ai.Mode = HandshakeAiMode.AiResponder;
            _player.CustomIntent = CombatActionIntent.Empty;
            yield return new WaitForSeconds(0.3f);

            var p = _player.Participant;

            // 玩家原地按住 E 伸手
            float timeout = 5.0f;
            float elapsed = 0f;
            bool reachedGrip = false;

            while (elapsed < timeout && !reachedGrip)
            {
                var intent = CombatActionIntent.Empty;
                intent.InteractHold = true;
                _player.CustomIntent = intent;

                if (_arb.SessionState == HandshakeState.Grip)
                {
                    reachedGrip = true;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 握緊推進至成功
            float gripElapsed = 0f;
            bool reachedShaking = false;
            while (gripElapsed < 1.2f)
            {
                var intent = CombatActionIntent.Empty;
                intent.InteractHold = true;
                _player.CustomIntent = intent;

                if (_arb.SessionState == HandshakeState.Shaking)
                {
                    reachedShaking = true;
                }
                gripElapsed += Time.deltaTime;
                yield return null;
            }

            bool outcomeSuccess = (_arb.LastOutcome == HandshakeOutcome.Success);
            yield return new WaitForSeconds(0.8f);

            _case3Pass = reachedGrip && reachedShaking && outcomeSuccess;
            _results.Add($"[案例 3] 玩家發起、AI 走近回應（對調角色成功）：{(_case3Pass ? "PASS" : "FAIL")}");
            _results.Add($"         - 碰到握緊: {reachedGrip}, 進入搖手: {reachedShaking}, 正式結果: {_arb.LastOutcome}");
        }

        private IEnumerator Case4_EarlyReleaseDuringGrip()
        {
            _arb.ResetAll();
            _ai.Mode = HandshakeAiMode.AiEarlyRelease;
            _player.CustomIntent = CombatActionIntent.Empty;
            yield return new WaitForSeconds(0.3f);

            var p = _player.Participant;

            // 玩家按住 E 走近
            float timeout = 4.0f;
            float elapsed = 0f;
            bool reachedGrip = false;

            while (elapsed < timeout && !reachedGrip)
            {
                var intent = CombatActionIntent.Empty;
                intent.InteractHold = true;
                intent.MoveInput = new Vector2(0f, 1f);
                _player.CustomIntent = intent;

                if (_arb.SessionState == HandshakeState.Grip)
                {
                    reachedGrip = true;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // AI 會在握緊 0.15s 後放開，玩家保持按住
            yield return new WaitForSeconds(0.40f);

            bool earlyReleaseTriggered = (_arb.LastOutcome == HandshakeOutcome.EarlyReleaseDuringGrip);
            bool sessionClosed = (_arb.SessionState == HandshakeState.None);

            _case4Pass = reachedGrip && earlyReleaseTriggered && sessionClosed;
            _results.Add($"[案例 4] 握緊期間有人放開（第 2 批情況直接收手）：{(_case4Pass ? "PASS" : "FAIL")}");
            _results.Add($"         - 進入握緊: {reachedGrip}, 提前放開判定: {earlyReleaseTriggered}, 會話中止收手: {sessionClosed}");
        }

        private IEnumerator Case5_AiCancel()
        {
            _arb.ResetAll();
            _ai.Mode = HandshakeAiMode.AiCancel;
            _player.CustomIntent = CombatActionIntent.Empty;
            yield return new WaitForSeconds(0.3f);

            var aiP = _ai.GetComponent<HandshakeParticipant>();

            // AI 伸手
            yield return new WaitForSeconds(1.1f);
            bool aiReached = (aiP.State == HandshakeState.Reaching || aiP.State == HandshakeState.Waiting);

            // 等待 AI 放開 E 並收手
            yield return new WaitForSeconds(1.3f);
            bool aiWithdrawingOrNone = (aiP.State == HandshakeState.Withdrawing || aiP.State == HandshakeState.None);

            _case5Pass = aiReached && aiWithdrawingOrNone;
            _results.Add($"[案例 5] AI 發起後放開 E（AI 取消）：{(_case5Pass ? "PASS" : "FAIL")}");
            _results.Add($"         - AI 曾伸手: {aiReached}, AI 隨後收手: {aiWithdrawingOrNone}");
        }

        private IEnumerator Case6_ResetConsistency()
        {
            _arb.ResetAll();
            _player.CustomIntent = CombatActionIntent.Empty;
            yield return new WaitForSeconds(0.2f);

            var p = _player.Participant;
            var aiP = _ai.GetComponent<HandshakeParticipant>();

            bool pClean = (p.State == HandshakeState.None && Mathf.Abs(p.transform.position.z - (-1.2f)) < 0.2f);
            bool aiClean = (aiP.State == HandshakeState.None && Mathf.Abs(aiP.transform.position.z - 1.2f) < 0.2f);
            bool arbClean = (_arb.SessionState == HandshakeState.None);

            _case6Pass = pClean && aiClean && arbClean;
            _results.Add($"[案例 6] 全場統一快速重設無殘留：{(_case6Pass ? "PASS" : "FAIL")}");
            _results.Add($"         - 玩家位置與狀態乾淨: {pClean}, AI 位置與狀態乾淨: {aiClean}, 判定元件無殘留: {arbClean}");
        }
    }
}
