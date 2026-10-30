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

2026-10-27 規劃補查：最新實作已為 `c23094b`，新增 `CombatEntity`、`CombatActionIntent` 與 `SimulatedNetworkChannel`。玩家與 AI 已有共同 `StepCombat` 入口，並有精準防禦中止、生命與全場重設的程式路徑；本次僅查來源，未重新執行驗收，上述舊 commit 的缺口不能直接當成新版本的失敗結論。

`Runtime/WhiteRoomNetwork.cs` 的 `SimulatedNetworkChannel` 使用同程序 `List<QueuedPacket>`、`Time.time` 與事件交付，提供輸入延遲／丟包實驗；此檔沒有跨程序傳輸或遠端狀態同步實作，因此不能以它或同程序 2P 模式證明同機雙執行個體／LAN 已完成。新共用戰鬥型別仍在 Sample Runtime，接下來先依計畫確認能力歸屬與隔離邊界，再驗證實際連線。

使用者於 2026-10-27 確認更新計畫：以線上版同步需求設計，先以同機／LAN 的最小戰鬥案例及延遲測試驗證，再持續擴充 gameplay。有效階段與案例由 [實作計畫的連線驗證路線](../Samples~/WhiteRoom/Implementation-Plan.md#連線驗證路線) 擁有；本紀錄保留上述 commit 的九項首交案例檢查，不將新增計畫追溯計入當時驗收結果。


## 交辦前來源補查

2026-10-27，釘選套件 `c23094b` 與當時宿主副本，僅讀取來源與比對雜湊，未開啟 PlayMode、編譯或執行建置。九支 Runtime C# 與 asmdef 逐檔 SHA-256 均一致。套件場景 SHA-256 為 `CFD62F2F690EB91BF7D13CF8EDD8F721EF8950D7919356E72965C2AB78333138`，宿主場景仍為 `0E08028159D941B40689796039AAB3ADDB534F5B5E3E862C08A01F4D1DE94A4A`；差異內容未解析，須由 Unity 工具檢查，不據此判定場景錯誤。

宿主 ProjectVersion 為 `6000.6.0f1`（`f7f8ed4d1e24`），manifest 指向本機 Combat-Aesthetics，URP 為 `17.6.0`。本輪版本選擇由實作計畫擁有。

| 來源 | 已有內容與交辦缺口 |
|---|---|
| `CombatEntity`、`WhiteRoomCharacter` | 共用戰鬥入口、攻擊區間、生命／KO、受擊與中斷已存在。`HandleLocomotion` 的 `canMove` 排除 Jumping，落地還原又要求 `canMove`，有無其他事件介入時落地後停留 Jumping 的路徑，優先實跑重現與修正。Guarding 同樣不能主動移步，與警戒慢步目標有差異。 |
| 精準防禦 | `ParryTap` 由 MouseButtonDown 產生，未採集放開；普通按住防禦的起始時間也能命中精準判定。與已確認按下再放開的操作不同，需補輸入語意與普通／精準區別案例。 |
| `WhiteRoomDummy` | 能接近與週期攻擊，未產生 GuardHold／ParryTap。近距離直接修改旋轉，未先查共同動作限制；須驗證受擊／暈眩期間是否繞過姿態限制。 |
| 全場重設 | `ResetAllArenaEntities` 只呼叫 `ResetEntity`；AI 的冷卻與目標未一起重設，`ResetDummy` 無呼叫。HUD 重設另清模擬佇列，R 鍵路徑沒有相同步驟，重設入口不一致。 |
| `WhiteRoomSceneBuilder`、HUD | 建立一名玩家與一名 AI；2P 以停用 AI、附加玩家控制來替換同一角色，未形成 2 名玩家加 2 名 AI 的情境配置。 |
| `WhiteRoomNetwork` 與引用搜尋 | `SendPacket` 的 Sample 呼叫只有 HUD 手動送 Empty 心跳，`OnPacketDelivered` 沒有訂閱者。玩家／AI 直接呼叫 StepCombat，模擬延遲尚未影響攻防，更沒有跨程序傳輸、預測修正或延遲補償。 |
| `WhiteRoomHUD` | 顯示生命、狀態與最多七筆文字；未見匯出路徑，`_showNetworkPanel` 只有初值與顯示判斷，未提供使用者切換。不是已確認的完整診斷工具。 |
| README | 仍有「實機驗證通過」、遠端同步與動作資料接用等描述；本輪未取得相應執行證據，不能據此當成驗收完成。下一批依實測證據校正。 |

以上為有來源定位的修正／驗證目標，納入 [戰鬥驗證交辦範圍（暫停）](../Samples~/WhiteRoom/Implementation-Plan.md#戰鬥驗證交辦範圍暫停)，不改寫先前回合的結果。角色為程式建立的幾何外殼，畫面與動作品質本輪未實測。

## 本輪實作修復與驗證紀錄 (2026-10-27)

依據交辦範圍，本輪針對既有 WhiteRoom 本機攻防循環、AI 防禦、診斷工具與建置進行實作修復與執行驗證。

### 1. 場景比對與未提交變更保護
- **場景比對**：比對套件與宿主 `WhiteRoom.unity`，確認結構與遊戲物件（Camera, Light, Global Volume, WhiteRoom_Bootstrap）完全一致；差異純為 Unity `6000.6.0f1` 重新存檔所引發之 URP 與引擎序列化版本欄位更新（LightmapSettings v13、TaaSettings 欄位名、UniversalAdditionalLightData 陰影遮罩）。未覆蓋宿主場景。
- **未提交變更**：保留他人未提交修改 `Editor/2D/PSDImporterWindow.cs` 與 `Editor/RecordsWindow.cs`。

### 2. 來源與依賴隔離盤點
- Runtime asmdef 引用 `Yu5h1Lib.CombatAesthetic`、`Yu5h1Lib.Common`、`Yu5h1Lib.Library`、`Yu5h1Lib.InputHandler` 與 `Unity.InputSystem`。
- 完全隔離於舊 2D `CharacterController2D`、`AttributeBehaviour` 與 `GameManager`，不處理亦不依賴 KFC 遷移。

### 3. 本機攻防循環缺口修正
1. **跳躍著地恢復**：
   - 修正 `CombatEntity.HandleLocomotion`：允許空中微調控制，著地時偵測 `_state == CombatState.Jumping` 並立即還原為 `Idle` / `Moving` / `Crouching`。
   - 解決起跳後卡死在 `Jumping` 導致無法移動與出招的嚴重缺陷。
2. **普通格擋 vs 按下再放開的精準彈刀**：
   - 在 `WhiteRoomCharacter` 採集右鍵按下時長：<=0.22s 快速放開判定為 `ParryTap`（精準彈刀）；>0.16s 長按判定為 `GuardHold`（普通格擋）。
   - 在 `CombatEntity` 中，只有處於 `Parrying` 姿態受擊才觸發彈刀與攻擊者中斷；處於 `Guarding` 姿態受擊固定為 80% 減傷，長按絕不偷渡為彈刀。
3. **警戒慢步 (Guarded Walk)**：
   - `canMove` 納入 `CombatState.Guarding`，給予 `_guardSpeed`（2.2 m/s）。防禦時可平滑位移調整走位，維持防禦姿態。
4. **全場統一重設一致性**：
   - `CombatEntity.ResetAllArenaEntities()` 觸發 `OnGlobalArenaReset` 事件。
   - `WhiteRoomDummy` 訂閱重置其決策冷卻（1.5s）與目標重新鎖定。
   - `SimulatedNetworkChannel` 訂閱清空待送封包佇列與計數。
   - R 鍵與 HUD 按鈕路徑完全一致。
5. **可配置 AI 防禦與姿態約束**：
   - `WhiteRoomDummy` 增加 `_enableDefense`、`_guardProbability` (0.60)、`_parryProbability` (0.30) 與防禦維持時序。
   - AI 依據目標攻擊前搖（Startup 橘紅預警）正面判定防禦。
   - 嚴格遵守姿態限制：在 `Stunned`、`Flinching`、`Incapacitated` 期間鎖定主動位移與旋轉。

### 4. 診斷面板、焦點隔離與紀錄匯出
- **操作焦點規則**：當滑鼠游標移入 HUD 面板時，設定 `WhiteRoomCharacter.IsUIFocused = true`，停止玩家出招與防禦輸入，並解鎖滑鼠游標且鎖定相機旋轉；後台戰鬥世界與 AI 即時推進，不因面板暫停。
- **真實網路標記**：標明真實傳輸層為「未連接 (不可用 / N/A)」，主機判定延遲為不可用，明確標示本地佇列為單程序記憶體模擬。
- **結構化日誌匯出**：提供一鍵匯出 JSON 日誌至 `Application.persistentDataPath`。

### 5. 實跑驗證數據與證據
- **編譯檢查**：Unity `6000.6.0f1` 宿主重新編譯，**0 錯誤**，過時 API 已清理完畢。
- **PlayMode 驗證**：
  - 普通格擋：承受 20 點打擊，減傷 80%（扣 4 滴），攻擊者未被中斷 (PASS)。
  - 精準防禦：0.18s 時窗內受擊，傷害歸零，攻擊者立即中斷並陷入 1.0s 暈眩 (PASS)。
  - 警戒慢步：防禦中移動維持 `Guarding` 姿態，速度 2.2 m/s (PASS)。
  - 失去戰力 (KO)：致死傷害後 HP 歸零，進入 `Incapacitated` 姿態 (PASS)。
  - 全場重設：生命、位置、狀態、AI 冷卻與網路佇列完整清空 (PASS)。
  - 情境配置切換：實測 1P vs 1 AI (2人)、1P vs 2 AI (3人)、2P vs 2 AI (4人)、2P PvP (2人) 均成功生成 (PASS)。
  - 日誌匯出：成功產出 `WhiteRoom_Logs_20261027_212858.json` (12,334 bytes)。
- **Windows Standalone 建置**：
  - 目標：`StandaloneWindows64`，場景：`WhiteRoom.unity`。
  - 結果：建置成功，產出執行檔 `W:\UnityProject\Yu5h1LibTest\Builds\WhiteRoom\WhiteRoom.exe`（667,136 bytes）。

### 6. 未執行項目明列
- 雙台實體電腦 LAN 連線實測（本輪尚未接通真實傳輸層）。
- 手感與數值平衡驗收（由使用者本人回饋定案）。
- 外部跨平台動作資料檔案格式（Web Viewer / Clip 載入器尚未實裝）。
