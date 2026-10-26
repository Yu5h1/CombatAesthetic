# WhiteRoom 靜態驗收紀錄

本紀錄對照 [WhiteRoom 首交驗收案例](../Samples~/WhiteRoom/Implementation-Plan.md#首交驗收案例)，檢查指定宿主副本與套件程式，並提出連線驗證的優先順序。

## 本輪範圍與證據

- 日期：2026-10-27。
- 計畫與實作基準：Combat-Aesthetics commit `08ce96f89d00173ab49b3ee945faea46ace7a053`。
- 指定場景：`W:\UnityProject\Yu5h1LibTest\Assets\Samples\Combat Aesthetic\1.0.0\WhiteRoom\Scenes\WhiteRoom.unity`。
- 場景 SHA-256：`0E08028159D941B40689796039AAB3ADDB534F5B5E3E862C08A01F4D1DE94A4A`。僅作檔案識別，不以 YAML 代替 Unity 場景檢查。
- 宿主 `ProjectVersion.txt`：Unity `6000.6.0f1`，revision `f7f8ed4d1e24`。計畫與 Sample README 仍記錄 `6000.3.9f1`，兩版本的相容性證據須分開。
- 宿主匯入副本的六支 Runtime C# 檔逐一 SHA-256 比對，與上述 commit 的套件 Sample 相同：Character、Dummy、SceneBuilder、Visuals、Camera、HUD。
- 本輪未開啟 Play Mode、未建置 Windows、未執行連線或實際畫面驗收；目前工具未提供可呼叫的 Unity bridge。下列為程式檢查，不冒充操作測試。
- 套件既有未提交修改 `Editor/2D/PSDImporterWindow.cs`、`Editor/RecordsWindow.cs` 不在本輪範圍，保持原狀。

## 驗收映射

本輪完整體驗案例通過數為 **0 / 9**，原因是未執行端到端操作驗收；不能解讀為九項實跑全部失敗。程式檢查已確認數個阻礙，足以判定此版本尚未達成首交目標。

| 計畫案例 | 實作入口 | 本輪證據與限制 |
|---|---|---|
| Sample 匯入與建置 | 宿主 Sample、Runtime asmdef | 匯入檔存在，六支程式與套件一致。未重跑編譯或建置；Report 的完成標記不替代本輪證據。版本與計畫不一致。 |
| runtime 製作與重載 | `WhiteRoomVisuals`、`WhiteRoomCharacter.ApplyVisualFeedback` | 現有呈現為幾何物件、縮放身體及平移手臂；六支 Sample Runtime 中未找到跨平台動作資料載入／重播接用。案例未執行。 |
| 移動與鏡頭 | `HandleLocomotion`、`WhiteRoomCamera` | 有鏡頭相對八方向移動、重力及跳躍、第三人稱鏡頭程式。起跳落地可讀性、碰撞與手感未實測。 |
| 原地／方向攻擊 | `StartAttack`、`PerformAttackHitCheck` | 有方向選擇、攻擊時間與一次球形接觸查詢；有效期間只查一次，後續進入範圍是否應命中需案例確認。畫面與判定對齊未實測。 |
| 普通防禦 | `WhiteRoomCharacter.TakeHit` | 依警戒旗標與面向判斷；未呈現角色反應能力／姿態調整所形成的限制。未實測連續來襲、背擊。 |
| 精準防禦 | `TakeHit`、`WhiteRoomDummy.ApplyParryStun` | 依進入警戒後經過時間判定，未要求放開右鍵；成功分支只改玩家狀態並返回，沒有將中止結果傳回攻擊者。Sample 及宿主 Assets C# 搜尋中，`ApplyParryStun` 只有定義。 |
| 受擊與恢復 | `TakeHit`、`HandleLocomotion` | 玩家傷害只顯示訊息，未更新生命／失去戰力狀態；`Flinching` 時仍採集方向驅動移動。保留外力變數不等於已符合受擊控制區間。 |
| PvE 與多方配置 | `WhiteRoomDummy`、`WhiteRoomSceneBuilder` | AI 依距離／計時直接呼叫玩家 `TakeHit`，沒有經過玩家的出招有效區間流程，也沒有同套防禦流程。Builder 建一個膠囊對手；缺多方共用規則的驗收證據。 |
| 重設與主題 | `ResetCharacter`、`ResetDummy`、HUD | R 只呼叫玩家重設；Sample 與宿主 Assets C# 搜尋中，`ResetDummy` 只有定義。無全場統一重設呼叫或亮／暗主題切換證據。 |

## 主要阻礙與影響

1. **玩家與 AI 的戰鬥流程不同。** 玩家有出招區間，AI 以計時直接造成受擊。尚不能比較同條件下的攻擊、防禦與恢復，也不適合作為未來玩家／AI 切換控制來源的基礎。
2. **精準防禦缺攻擊者結果。** 玩家端的成功訊息不能證明敵方進攻被中止。接網路前要先讓攻防結果成為可辨識、可傳遞的狀態變更。
3. **重設不涵蓋全場。** 玩家回到原點後，AI 的位置、生命與計時仍由原狀態延續，無法可靠比較同一情境。
4. **動作接用與文件不一致。** README 第 5 節描述 `RuntimePoseController`、`HumanPoseHandler`、`CharacterSMB` 已在 WhiteRoom 接用；本輪六支 Runtime 沒有這些引用。需要真實資料到播放的證據，不能以套件相依宣告替代。
5. **目前是本機直接驅動。** 六支 Runtime 沒有連線會話、角色控制權、跨端出招識別、共同時間或正式結果同步流程；輸入、計時、傷害與畫面集中在 Sample 腳本。這是指定 Sample 的發現，不代表 Yu5h1Lib 其他位置沒有可重用網路能力。

附帶操作差異：Ctrl 實作為按一下切換蹲姿，與已確認的「按住維持、放開起身」不同；首次完整操作回歸應納入此項。

## 驗收後的計畫接續

使用者於 2026-10-27 確認更新計畫：以線上版同步需求設計，先以同機／LAN 的最小戰鬥案例及延遲測試驗證，再持續擴充 gameplay。有效階段與案例由 [實作計畫的連線驗證路線](../Samples~/WhiteRoom/Implementation-Plan.md#連線驗證路線) 擁有；本紀錄保留上述 commit 的九項首交案例檢查，不將新增計畫追溯計入當時驗收結果。
