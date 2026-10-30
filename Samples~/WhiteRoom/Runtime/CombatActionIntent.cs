using System;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    /// <summary>
    /// 抽象操作意圖：分離「輸入來源」（本機鍵鼠、AI 決策、遠端網路封包）與「戰鬥判定」。
    /// </summary>
    [Serializable]
    public struct CombatActionIntent
    {
        public Vector2 MoveInput;
        public bool Jump;
        public bool CrouchHold;
        public bool CrouchToggle;
        public bool Attack;
        public bool GuardHold;
        public bool ParryTap;
        public bool InteractHold;
        public bool InteractDown;
        public bool InteractUp;
        public bool GlobalReset;
        public float Timestamp;

        public static CombatActionIntent Empty => new CombatActionIntent
        {
            MoveInput = Vector2.zero,
            Jump = false,
            CrouchHold = false,
            CrouchToggle = false,
            Attack = false,
            GuardHold = false,
            ParryTap = false,
            InteractHold = false,
            InteractDown = false,
            InteractUp = false,
            GlobalReset = false,
            Timestamp = 0f
        };
    }

    /// <summary>
    /// 操作來源介面：本機玩家、AI 傀儡或遠端網路客戶端皆實作此介面
    /// </summary>
    public interface ICombatInputSource
    {
        CombatActionIntent PollIntent(CombatEntity entity);
    }
}
