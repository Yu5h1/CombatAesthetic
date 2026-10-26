using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public class WhiteRoomHUD : MonoBehaviour
    {
        [SerializeField] private WhiteRoomCharacter _playerCharacter;
        [SerializeField] private WhiteRoomDummy _dummy;

        private readonly List<string> _actionLogs = new List<string>();
        private const int MaxLogs = 7;

        // 不良網路模擬器
        private readonly SimulatedNetworkChannel _networkChannel = new SimulatedNetworkChannel();
        private bool _showNetworkPanel = true;
        private bool _twoPlayerMode = false;

        public SimulatedNetworkChannel NetworkChannel => _networkChannel;

        private void Start()
        {
            if (_playerCharacter == null) _playerCharacter = FindFirstObjectByType<WhiteRoomCharacter>();
            if (_dummy == null) _dummy = FindFirstObjectByType<WhiteRoomDummy>();

            if (_playerCharacter != null && _playerCharacter.Entity != null)
            {
                _playerCharacter.Entity.OnCombatLog += LogAction;
            }
            if (_dummy != null && _dummy.Entity != null)
            {
                _dummy.Entity.OnCombatLog += LogAction;
            }

            _networkChannel.OnPacketDropped += msg =>
            {
                LogAction($"<color=red>[網路丟包] 序號 #{msg.SequenceId} 遺失！</color>");
            };
        }

        private void Update()
        {
            _networkChannel.UpdateQueue();
        }

        private void OnDestroy()
        {
            if (_playerCharacter != null && _playerCharacter.Entity != null)
            {
                _playerCharacter.Entity.OnCombatLog -= LogAction;
            }
            if (_dummy != null && _dummy.Entity != null)
            {
                _dummy.Entity.OnCombatLog -= LogAction;
            }
        }

        private void LogAction(string message)
        {
            _actionLogs.Add($"[{Time.time:F2}s] {message}");
            if (_actionLogs.Count > MaxLogs)
            {
                _actionLogs.RemoveAt(0);
            }
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = 13;
            GUI.skin.box.fontSize = 13;
            GUI.skin.button.fontSize = 13;

            // 1. 左上方：戰鬥狀態與生命條
            GUILayout.BeginArea(new Rect(15, 15, 380, 260), GUI.skin.box);
            GUILayout.Label("<b><size=15>WhiteRoom 3D 連線戰鬥測試場</size></b>");
            GUILayout.Space(4);

            // 玩家 1 狀態
            if (_playerCharacter != null && _playerCharacter.Entity != null)
            {
                var pe = _playerCharacter.Entity;
                GUILayout.Label($"<b>玩家 1 (P1)</b>：HP {pe.CurrentHealth:F0}/{pe.MaxHealth:F0}  [{pe.State}]");
                DrawHealthBar(pe.CurrentHealth, pe.MaxHealth, Color.green);
            }

            GUILayout.Space(6);

            // 假人 / 對手狀態
            if (_dummy != null && _dummy.Entity != null)
            {
                var de = _dummy.Entity;
                GUILayout.Label($"<b>訓練假人 (AI)</b>：HP {de.CurrentHealth:F0}/{de.MaxHealth:F0}  [{de.State}]");
                DrawHealthBar(de.CurrentHealth, de.MaxHealth, Color.red);
            }

            GUILayout.Space(8);

            // 全場統一重設按鈕
            if (GUILayout.Button("【全場統一重設 [R]】"))
            {
                CombatEntity.ResetAllArenaEntities();
                _networkChannel.ResetStats();
            }

            // 同機雙人模式切換
            bool new2P = GUILayout.Toggle(_twoPlayerMode, "啟用同機雙人模式 (2P: 方向鍵+小鍵盤)");
            if (new2P != _twoPlayerMode)
            {
                _twoPlayerMode = new2P;
                if (_dummy != null)
                {
                    _dummy.enabled = !_twoPlayerMode;
                    var p2 = _dummy.GetComponent<WhiteRoomCharacter>();
                    if (_twoPlayerMode && p2 == null)
                    {
                        p2 = _dummy.gameObject.AddComponent<WhiteRoomCharacter>();
                        p2.SetControlScheme(true);
                    }
                    else if (p2 != null)
                    {
                        p2.enabled = _twoPlayerMode;
                    }
                }
            }

            GUILayout.EndArea();

            // 2. 右上方：不良網路條件模擬面板
            if (_showNetworkPanel)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 330, 15, 315, 230), GUI.skin.box);
                GUILayout.Label("<b><size=14>不良網路模擬器 (Adverse Network)</size></b>");

                GUILayout.Label($"延遲 (Latency)：{_networkChannel.LatencyMs:F0} ms");
                _networkChannel.LatencyMs = GUILayout.HorizontalSlider(_networkChannel.LatencyMs, 0f, 300f);

                GUILayout.Label($"抖動 (Jitter)：±{_networkChannel.JitterMs:F0} ms");
                _networkChannel.JitterMs = GUILayout.HorizontalSlider(_networkChannel.JitterMs, 0f, 50f);

                GUILayout.Label($"丟包率 (Packet Loss)：{(_networkChannel.PacketLossRate * 100f):F0} %");
                _networkChannel.PacketLossRate = GUILayout.HorizontalSlider(_networkChannel.PacketLossRate, 0f, 0.25f);

                GUILayout.Space(6);
                GUILayout.Label($"已發送封包: {_networkChannel.TotalSent} | 遺失: {_networkChannel.TotalDropped}");

                if (GUILayout.Button("測試發送同步心跳封包"))
                {
                    _networkChannel.SendPacket("P1", CombatActionIntent.Empty);
                }

                GUILayout.EndArea();
            }

            // 3. 左下方：操作指南
            GUILayout.BeginArea(new Rect(15, Screen.height - 180, 420, 165), GUI.skin.box);
            GUILayout.Label("<b>操作說明 (UX Verified)</b>");
            GUILayout.Label("• <b>WASD</b>：8 方向移動（相對於第 3 人稱鏡頭）");
            GUILayout.Label("• <b>Space</b>：起跳（具有重力與貼地）");
            GUILayout.Label("• <b>Left Ctrl</b>：按住蹲下（放開自動起身）；<b>C</b>：切換蹲下");
            GUILayout.Label("• <b>滑鼠左鍵</b>：發動攻擊（蓄力前搖 0.2s -> 打擊 0.15s -> 收招 0.25s）");
            GUILayout.Label("• <b>滑鼠右鍵</b>：長按格擋（減傷80%）；0.18s 內點按觸發<b>精準彈刀</b>中斷敵人！");
            GUILayout.Label("• <b>R 鍵</b>：全場重設（重置生命、位置、速度、計時）");
            GUILayout.EndArea();

            // 4. 右下方：即時戰鬥事件日誌
            GUILayout.BeginArea(new Rect(Screen.width - 460, Screen.height - 210, 445, 195), GUI.skin.box);
            GUILayout.Label("<b>即時攻防判定與網路紀錄</b>");
            foreach (var log in _actionLogs)
            {
                GUILayout.Label(log);
            }
            GUILayout.EndArea();
        }

        private void DrawHealthBar(float current, float max, Color barColor)
        {
            Rect r = GUILayoutUtility.GetRect(200, 14);
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
