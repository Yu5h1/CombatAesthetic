using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    [Serializable]
    public struct CombatLogEntry
    {
        public float timestamp;
        public string message;
    }

    [Serializable]
    public class CombatSessionLogExport
    {
        public string sessionTimestamp;
        public string unityVersion;
        public string scenario;
        public List<CombatLogEntry> logs = new List<CombatLogEntry>();
    }

    public class WhiteRoomHUD : MonoBehaviour
    {
        private readonly List<string> _actionLogs = new List<string>();
        private readonly List<CombatLogEntry> _allHistoryLogs = new List<CombatLogEntry>();
        private const int MaxDisplayLogs = 7;

        // 不良網路模擬器
        private readonly SimulatedNetworkChannel _networkChannel = new SimulatedNetworkChannel();
        private bool _showDebugPanel = true;

        // UI 區塊 Rect 用於焦點偵測
        private Rect _rectStatusPanel;
        private Rect _rectDebugPanel;
        private Rect _rectHelpPanel;
        private Rect _rectLogsPanel;

        public SimulatedNetworkChannel NetworkChannel => _networkChannel;

        private void Start()
        {
            RefreshBindings();

            CombatEntity.OnGlobalArenaReset += HandleGlobalReset;

            _networkChannel.OnPacketDropped += msg =>
            {
                LogAction($"<color=red>[網路丟包] 序號 #{msg.SequenceId} 遺失！</color>");
            };
        }

        private void OnDestroy()
        {
            CombatEntity.OnGlobalArenaReset -= HandleGlobalReset;
            UnbindAll();
        }

        private void Update()
        {
            _networkChannel.UpdateQueue();

            // F1 快速切換診斷面板
            if (InputHandler.GetKeyDown(KeyCode.F1))
            {
                _showDebugPanel = !_showDebugPanel;
            }
        }

        public void RefreshBindings()
        {
            UnbindAll();
            foreach (var entity in CombatEntity.ActiveEntities)
            {
                if (entity != null)
                {
                    entity.OnCombatLog += LogAction;
                }
            }
        }

        private void UnbindAll()
        {
            foreach (var entity in CombatEntity.ActiveEntities)
            {
                if (entity != null)
                {
                    entity.OnCombatLog -= LogAction;
                }
            }
        }

        private void HandleGlobalReset()
        {
            _networkChannel.ResetStats();
            LogAction("【全場統一重設】角色生命、位置、AI 冷卻/目標及網路佇列已清空還原。");
        }

        private void LogAction(string message)
        {
            string formatted = $"[{Time.time:F2}s] {message}";
            _actionLogs.Add(formatted);
            if (_actionLogs.Count > MaxDisplayLogs)
            {
                _actionLogs.RemoveAt(0);
            }

            _allHistoryLogs.Add(new CombatLogEntry
            {
                timestamp = Time.time,
                message = message
            });
        }

        private void ExportStructuredLogs()
        {
            var builder = FindAnyObjectByType<WhiteRoomSceneBuilder>();
            var export = new CombatSessionLogExport
            {
                sessionTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                unityVersion = Application.unityVersion,
                scenario = builder != null ? builder.CurrentScenario.ToString() : "Default",
                logs = new List<CombatLogEntry>(_allHistoryLogs)
            };

            string json = JsonUtility.ToJson(export, true);
            string filename = $"WhiteRoom_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = Path.Combine(Application.persistentDataPath, filename);

            try
            {
                File.WriteAllText(path, json);
                LogAction($"<color=cyan>【紀錄匯出成功】路徑: {path}</color>");
                Debug.Log($"[WhiteRoomHUD] 結構化日誌成功匯出至: {path}");
            }
            catch (Exception ex)
            {
                LogAction($"<color=red>【紀錄匯出失敗】{ex.Message}</color>");
            }
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = 12;
            GUI.skin.box.fontSize = 12;
            GUI.skin.button.fontSize = 12;

            _rectStatusPanel = new Rect(15, 15, 380, 275);
            _rectDebugPanel = new Rect(Screen.width - 340, 15, 325, 295);
            _rectHelpPanel = new Rect(15, Screen.height - 185, 420, 170);
            _rectLogsPanel = new Rect(Screen.width - 480, Screen.height - 215, 465, 200);

            // 操作焦點規則：當滑鼠游標位於任一面板內時，由面板獲取焦點，停止玩家戰鬥按鍵採集與鏡頭旋轉
            Vector2 mousePos = Event.current.mousePosition;
            bool isMouseOverUI = _rectStatusPanel.Contains(mousePos) ||
                                (_showDebugPanel && _rectDebugPanel.Contains(mousePos)) ||
                                _rectHelpPanel.Contains(mousePos) ||
                                _rectLogsPanel.Contains(mousePos);
            WhiteRoomCharacter.IsUIFocused = isMouseOverUI;

            // 1. 左上方：戰鬥狀態、全場實體與情境切換
            GUILayout.BeginArea(_rectStatusPanel, GUI.skin.box);
            GUILayout.Label("<b><size=14>WhiteRoom 戰鬥與情境控制</size></b>");
            GUILayout.Space(2);

            // 動態列出所有活動實體狀態
            var entities = CombatEntity.ActiveEntities;
            for (int i = 0; i < entities.Count; i++)
            {
                var ent = entities[i];
                if (ent == null) continue;

                string teamLabel = ent.TeamId == 0 ? "<color=#4da6ff>[隊伍0-友方]</color>" : "<color=#ff6666>[隊伍1-敵方]</color>";
                GUILayout.Label($"<b>{ent.EntityId}</b> {teamLabel} HP: {ent.CurrentHealth:F0}/{ent.MaxHealth:F0} [{ent.State}]");
                Color barColor = ent.TeamId == 0 ? Color.green : Color.red;
                DrawHealthBar(ent.CurrentHealth, ent.MaxHealth, barColor);
            }

            GUILayout.Space(6);

            // 全場統一重設按鈕
            if (GUILayout.Button("【全場統一重設 [R]】"))
            {
                CombatEntity.ResetAllArenaEntities();
            }

            // 情境配置切換
            GUILayout.Space(4);
            GUILayout.Label("<b>情境配置切換：</b>");
            GUILayout.BeginHorizontal();
            var builder = FindAnyObjectByType<WhiteRoomSceneBuilder>();
            if (GUILayout.Button("1P vs 1 AI"))
            {
                if (builder != null) builder.ApplyScenario(ArenaScenario.OnePlayerVsOneAI);
            }
            if (GUILayout.Button("1P vs 2 AI"))
            {
                if (builder != null) builder.ApplyScenario(ArenaScenario.OnePlayerVsTwoAI);
            }
            if (GUILayout.Button("2P vs 2 AI"))
            {
                if (builder != null) builder.ApplyScenario(ArenaScenario.TwoPlayerVsTwoAI);
            }
            if (GUILayout.Button("2P PvP"))
            {
                if (builder != null) builder.ApplyScenario(ArenaScenario.TwoPlayerPvP);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            // 2. 右上方：不良網路條件模擬與真實網路狀態標記
            if (_showDebugPanel)
            {
                GUILayout.BeginArea(_rectDebugPanel, GUI.skin.box);
                GUILayout.Label("<b><size=13>診斷面板 (F1 開關) - 網路狀態標記</size></b>");
                GUILayout.Space(2);

                // 明確標記真實網路資訊（未接通真實網路時不可用）
                GUILayout.Label("• <b>真實傳輸層</b>：<color=yellow>未連接 (不可用 / N/A)</color>");
                GUILayout.Label("• <b>主機判定延遲 / 修正</b>：<color=orange>[不可用 - 尚未接入真實網路]</color>");
                GUILayout.Label("• <b>本地模擬通道</b>：單一程序記憶體佇列 (僅供延遲參數實驗)");
                GUILayout.Space(4);

                GUILayout.Label($"模擬延遲 (Latency)：{_networkChannel.LatencyMs:F0} ms");
                _networkChannel.LatencyMs = GUILayout.HorizontalSlider(_networkChannel.LatencyMs, 0f, 300f);

                GUILayout.Label($"模擬抖動 (Jitter)：±{_networkChannel.JitterMs:F0} ms");
                _networkChannel.JitterMs = GUILayout.HorizontalSlider(_networkChannel.JitterMs, 0f, 50f);

                GUILayout.Label($"模擬丟包率 (Packet Loss)：{(_networkChannel.PacketLossRate * 100f):F0} %");
                _networkChannel.PacketLossRate = GUILayout.HorizontalSlider(_networkChannel.PacketLossRate, 0f, 0.25f);

                GUILayout.Space(4);
                GUILayout.Label($"佇列送出: {_networkChannel.TotalSent} | 遺失: {_networkChannel.TotalDropped}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("發送心跳封包"))
                {
                    _networkChannel.SendPacket("P1", CombatActionIntent.Empty);
                }
                if (GUILayout.Button("【匯出結構化紀錄 (JSON)】"))
                {
                    ExportStructuredLogs();
                }
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }

            // 3. 左下方：操作指南
            GUILayout.BeginArea(_rectHelpPanel, GUI.skin.box);
            GUILayout.Label("<b>操作說明 (UX Verified)</b>");
            GUILayout.Label("• <b>WASD</b>：8 方向移動；<b>Space</b>：起跳落地恢復");
            GUILayout.Label("• <b>Left Ctrl</b>：按住蹲下（放開起身）；<b>C</b>：切換蹲下");
            GUILayout.Label("• <b>滑鼠左鍵</b>：出招（前搖 0.2s -> 打擊 0.15s -> 收招 0.25s）");
            GUILayout.Label("• <b>滑鼠右鍵</b>：長按防禦（減傷80%，可<b>警戒慢步</b>）；<b>快速點按放開</b>觸發<b>精準彈刀 (Parry)</b> 中止敵方攻擊！");
            GUILayout.Label("• <b>R 鍵</b>：全場重設（重置生命、位置、AI冷卻與目標、網路佇列）");
            GUILayout.Label("• <b>F1</b>：開關診斷面板；滑鼠進入面板時自動釋放游標並暫停戰鬥輸入");
            GUILayout.EndArea();

            // 4. 右下方：即時戰鬥事件日誌
            GUILayout.BeginArea(_rectLogsPanel, GUI.skin.box);
            GUILayout.Label("<b>即時攻防判定與戰鬥紀錄</b>");
            foreach (var log in _actionLogs)
            {
                GUILayout.Label(log);
            }
            GUILayout.EndArea();
        }

        private void DrawHealthBar(float current, float max, Color barColor)
        {
            Rect r = GUILayoutUtility.GetRect(200, 12);
            GUI.Box(r, "");
            float pct = Mathf.Clamp01(current / Mathf.Max(1f, max));
            Rect fillRect = new Rect(r.x + 1, r.y + 1, (r.width - 2) * pct, r.height - 2);

            Color oldColor = GUI.color;
            GUI.color = barColor;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
    }
}