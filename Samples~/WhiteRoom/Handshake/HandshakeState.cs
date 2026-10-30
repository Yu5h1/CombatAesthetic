using System;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    /// <summary>
    /// 握手互動狀態機（依據 WhiteRoom 實作計畫第一批交辦規範）：
    /// 1. None: 常態站立或移動
    /// 2. Reaching: 伸手（0.50s，播完停在伸出姿勢）
    /// 3. Waiting: 等待（按住 E 循環等待對方走近）
    /// 4. Grip: 握緊（雙方接觸碰到後進入，持續 0.50s / 15 影格，為判定時窗）
    /// 5. Shaking: 搖一下（握手成功：雙方上下搖晃 0.50s）
    /// 6. Withdrawing: 收手（0.50s，平滑返回站立姿態）
    /// </summary>
    public enum HandshakeState
    {
        None = 0,
        Reaching = 1,
        Waiting = 2,
        Grip = 3,
        Shaking = 4,
        Withdrawing = 5
    }

    /// <summary>
    /// 握手判定結果
    /// </summary>
    public enum HandshakeOutcome
    {
        None = 0,
        Cancelled = 1,                 // 發起或等待中放開收手（取消）
        Success = 2,                   // 握手成功（握緊全程按住且完成搖手）
        EarlyReleaseDuringGrip = 3     // 握緊期間有人放開（屬第 2 批情況，本批直接收手）
    }
}
