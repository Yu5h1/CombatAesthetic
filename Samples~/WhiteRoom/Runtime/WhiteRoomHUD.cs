using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public class WhiteRoomHUD : MonoBehaviour
    {
        [SerializeField] private WhiteRoomCharacter _character;
        [SerializeField] private WhiteRoomDummy _dummy;

        private readonly List<string> _actionLogs = new List<string>();
        private const int MaxLogs = 5;

        private void Start()
        {
            if (_character == null)
            {
                _character = FindFirstObjectByType<WhiteRoomCharacter>();
            }
            if (_dummy == null)
            {
                _dummy = FindFirstObjectByType<WhiteRoomDummy>();
            }

            if (_character != null)
            {
                _character.OnCombatAction += LogAction;
            }
        }

        private void OnDestroy()
        {
            if (_character != null)
            {
                _character.OnCombatAction -= LogAction;
            }
        }

        private void LogAction(string message)
        {
            _actionLogs.Add($"[{Time.time:F1}s] {message}");
            if (_actionLogs.Count > MaxLogs)
            {
                _actionLogs.RemoveAt(0);
            }
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = 14;
            GUI.skin.box.fontSize = 14;

            // Header Box
            GUILayout.BeginArea(new Rect(15, 15, 360, 220), GUI.skin.box);
            GUILayout.Label("<b><size=16>WhiteRoom 3D PvE 測試場</size></b>");
            GUILayout.Space(5);

            string stateName = _character != null ? _character.CurrentState.ToString() : "N/A";
            string crouchText = (_character != null && _character.IsCrouching) ? "<color=yellow>蹲下中</color>" : "站立";
            string guardText = (_character != null && _character.IsGuarding) ? "<color=cyan>防禦中</color>" : "無";

            GUILayout.Label($"角色狀態：<b>{stateName}</b> (姿態: {crouchText}, 防禦: {guardText})");
            if (_dummy != null)
            {
                GUILayout.Label($"訓練標靶 HP：{_dummy.Health:F0} / {_dummy.MaxHealth:F0}");
            }
            GUILayout.Space(5);

            GUILayout.Label("<b>操作說明：</b>");
            GUILayout.Label("• <b>W A S D</b>：八方向移動（鏡頭水平投影視角）");
            GUILayout.Label("• <b>Space</b>：跳躍與落地");
            GUILayout.Label("• <b>C / Left Ctrl</b>：切換蹲下姿態");
            GUILayout.Label("• <b>滑鼠左鍵</b>：出招（原地／指定方向攻擊）");
            GUILayout.Label("• <b>滑鼠右鍵 (長按)</b>：普通格擋防禦");
            GUILayout.Label("• <b>滑鼠右鍵 (抓時機點擊)</b>：精準防禦 (Parry)");
            GUILayout.Label("• <b>R 鍵</b>：立即重設測試情境");
            GUILayout.Label("• <b>Left Alt / Esc</b>：切換滑鼠視角鎖定");
            GUILayout.EndArea();

            // Combat Log Box
            if (_actionLogs.Count > 0)
            {
                GUILayout.BeginArea(new Rect(15, Screen.height - 130, 420, 115), GUI.skin.box);
                GUILayout.Label("<b>戰鬥與動作反饋：</b>");
                for (int i = _actionLogs.Count - 1; i >= 0; i--)
                {
                    GUILayout.Label(_actionLogs[i]);
                }
                GUILayout.EndArea();
            }
        }
    }
}
