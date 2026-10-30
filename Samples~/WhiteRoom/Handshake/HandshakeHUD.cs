using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    [Serializable]
    public struct HandshakeLogEntry
    {
        public float timestamp;
        public string message;
    }

    [Serializable]
    public class HandshakeSessionLogExport
    {
        public string sessionTimestamp;
        public string unityVersion;
        public string interactionName = "Handshake";
        public List<HandshakeLogEntry> logs = new List<HandshakeLogEntry>();
    }

    public class HandshakeHUD : MonoBehaviour
    {
        private readonly List<string> _displayLogs = new List<string>();
        private readonly List<HandshakeLogEntry> _allHistoryLogs = new List<HandshakeLogEntry>();
        private const int MaxDisplayLogs = 8;

        private Rect _rectStatusPanel;
        private Rect _rectGuidePanel;
        private Rect _rectScenarioPanel;
        private Rect _rectLogPanel;

        private HandshakeCharacter _player;
        private HandshakeAiController _ai;

        private void Start()
        {
            FindEntities();

            if (HandshakeArbitrator.Instance != null)
            {
                HandshakeArbitrator.Instance.OnArbitrationLog += LogMessage;
            }

            LogMessage("【WhiteRoom 握手互動系統就緒】請按 E 伸手，或點擊下方情境按鈕進行測試。");
        }

        private void FindEntities()
        {
            _player = FindAnyObjectByType<HandshakeCharacter>();
            _ai = FindAnyObjectByType<HandshakeAiController>();
        }

        private void Update()
        {
            if (_player == null || _ai == null)
            {
                FindEntities();
            }
        }

        public void LogMessage(string message)
        {
            string formatted = $"[{Time.time:F2}s] {message}";
            _displayLogs.Add(formatted);
            if (_displayLogs.Count > MaxDisplayLogs)
            {
                _displayLogs.RemoveAt(0);
            }

            _allHistoryLogs.Add(new HandshakeLogEntry
            {
                timestamp = Time.time,
                message = message
            });
        }

        private void ExportLogs()
        {
            var export = new HandshakeSessionLogExport
            {
                sessionTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                unityVersion = Application.unityVersion,
                interactionName = "Handshake",
                logs = new List<HandshakeLogEntry>(_allHistoryLogs)
            };

            string json = JsonUtility.ToJson(export, true);
            string filename = $"WhiteRoom_Handshake_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = Path.Combine(Application.persistentDataPath, filename);

            try
            {
                File.WriteAllText(path, json);
                LogMessage($"<color=cyan>【紀錄匯出成功】路徑: {path}</color>");
                Debug.Log($"[HandshakeHUD] 握手結構化日誌匯出至: {path}");
            }
            catch (Exception ex)
            {
                LogMessage($"<color=red>【紀錄匯出失敗】{ex.Message}</color>");
            }
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = 12;
            GUI.skin.box.fontSize = 12;
            GUI.skin.button.fontSize = 12;

            _rectStatusPanel = new Rect(15, 15, 380, 240);
            _rectScenarioPanel = new Rect(15, 265, 380, 185);
            _rectGuidePanel = new Rect(15, Screen.height - 180, 420, 165);
            _rectLogPanel = new Rect(Screen.width - 480, Screen.height - 230, 465, 215);

            // 操作焦點判定：滑鼠游標在面板內時鎖定玩家輸入與鏡頭旋轉
            Vector2 mousePos = Event.current.mousePosition;
            bool isMouseOverUI = _rectStatusPanel.Contains(mousePos) ||
                                 _rectScenarioPanel.Contains(mousePos) ||
                                 _rectGuidePanel.Contains(mousePos) ||
                                 _rectLogPanel.Contains(mousePos);
            WhiteRoomCharacter.IsUIFocused = isMouseOverUI;

            // 1. 左上方：握手互動狀態面板
            GUILayout.BeginArea(_rectStatusPanel, GUI.skin.box);
            GUILayout.Label("<b><size=14>WhiteRoom 握手互動系統 (Handshake)</size></b>");
            GUILayout.Label("<color=#4da6ff><b>當前選擇互動: 握手 (Handshake)</b></color>");
            GUILayout.Space(4);

            var arb = HandshakeArbitrator.Instance;
            if (arb != null)
            {
                GUILayout.Label($"<b>判定狀態:</b> <color=yellow>[{arb.SessionState}]</color>");
                GUILayout.Label($"<b>最新結果:</b> <color=cyan>[{arb.LastOutcome}]</color>");
            }

            GUILayout.Space(4);
            if (_player != null && _player.Participant != null)
            {
                bool hold = _player.Participant.CurrentIntent.InteractHold;
                GUILayout.Label($"<b>Player 1:</b> 姿態: [{_player.Participant.State}] | 按住 E: {(hold ? "<color=green>YES</color>" : "NO")}");
            }

            if (_ai != null && _ai.GetComponent<HandshakeParticipant>() != null)
            {
                var aiP = _ai.GetComponent<HandshakeParticipant>();
                bool hold = aiP.CurrentIntent.InteractHold;
                GUILayout.Label($"<b>AI 對手:</b> 模式: [{_ai.Mode}] | 姿態: [{aiP.State}] | 按住 E: {(hold ? "<color=green>YES</color>" : "NO")}");
            }

            GUILayout.Space(6);
            if (GUILayout.Button("【全場快速重設 [R]】"))
            {
                HandshakeArbitrator.Instance?.ResetAll();
            }
            GUILayout.EndArea();

            // 2. 左中央：情境快速切換面板
            GUILayout.BeginArea(_rectScenarioPanel, GUI.skin.box);
            GUILayout.Label("<b>情境快速切換（驗收測試）：</b>");
            GUILayout.Space(2);

            if (GUILayout.Button("1. [情境 1: AI 發起 ➔ 玩家走近回應 (成功)]"))
            {
                if (_ai != null) _ai.Mode = HandshakeAiMode.AiInitiator;
                HandshakeArbitrator.Instance?.ResetAll();
                LogMessage("【設定情境 1】AI 將主動伸手並等待，請按住 E 並以 WASD 走近對手。");
            }

            if (GUILayout.Button("2. [情境 2: 玩家發起 ➔ AI 走近回應 (成功)]"))
            {
                if (_ai != null) _ai.Mode = HandshakeAiMode.AiResponder;
                HandshakeArbitrator.Instance?.ResetAll();
                LogMessage("【設定情境 2】請原地按住 E 伸手，AI 偵測到後將主動伸手並走近回應。");
            }

            if (GUILayout.Button("3. [情境 3: 玩家發起 ➔ 玩家放開 (取消)]"))
            {
                if (_ai != null) _ai.Mode = HandshakeAiMode.Idle;
                HandshakeArbitrator.Instance?.ResetAll();
                LogMessage("【設定情境 3】請按住 E 伸手後隨即放開，觀察收手返回站姿。");
            }

            if (GUILayout.Button("4. [情境 4: AI 發起 ➔ AI 放開 (取消)]"))
            {
                if (_ai != null) _ai.Mode = HandshakeAiMode.AiCancel;
                HandshakeArbitrator.Instance?.ResetAll();
                LogMessage("【設定情境 4】AI 發起伸手後，將在 1.2 秒後主動放開 E 取消。");
            }

            if (GUILayout.Button("5. [情境 5: 握緊期間放開 (第 2 批情況)]"))
            {
                if (_ai != null) _ai.Mode = HandshakeAiMode.AiEarlyRelease;
                HandshakeArbitrator.Instance?.ResetAll();
                LogMessage("【設定情境 5】雙方碰觸進入握緊後，AI 將提前放開 E，觸發第 2 批情況直接收手。");
            }

            GUILayout.EndArea();

            // 3. 左下方：操作指南
            GUILayout.BeginArea(_rectGuidePanel, GUI.skin.box);
            GUILayout.Label("<b>操作與互動指南 (UX Verified)：</b>");
            GUILayout.Label("• <b>[E 鍵] (按住)</b>：伸手 (0.5s) ➔ 進入等待姿態 (循環)");
            GUILayout.Label("• <b>[E 鍵] (放開)</b>：收手 (0.5s) 返回常態姿態（取消）");
            GUILayout.Label("• <b>[W / A / S / D]</b>：8 方向移動（伸手/等待時可警戒慢步 2.2m/s 走近）");
            GUILayout.Label("• <b>[Space]</b>：跳躍（常態下可起跳）");
            GUILayout.Label("• <b>[R 鍵]</b>：全場快速重設（還原角色位置、握手狀態與 AI）");
            GUILayout.Label("• <b>[雙手碰觸]</b>：雙方右手接觸 ➔ 握緊 (0.5s 判定窗) ➔ 成功搖手 (0.5s)");
            GUILayout.EndArea();

            // 4. 右下方：事件日誌與匯出
            GUILayout.BeginArea(_rectLogPanel, GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>握手判定事件紀錄：</b>");
            if (GUILayout.Button("匯出 JSON 紀錄", GUILayout.Width(130)))
            {
                ExportLogs();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            for (int i = 0; i < _displayLogs.Count; i++)
            {
                GUILayout.Label(_displayLogs[i]);
            }
            GUILayout.EndArea();
        }
    }
}
