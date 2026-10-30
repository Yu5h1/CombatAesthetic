# WhiteRoom：3D 測試場 Sample（握手互動與徒手對戰）

> 套件名稱：`com.yu5h1.combataesthetic`  
> 驗證宿主：Unity `6000.6.0f1` URP（`W:/UnityProject/Yu5h1LibTest` 實測 0 編譯錯誤、PlayMode 握手互動 6 大案例全數通過、攻防循環驗證通過、Windows x64 建置成功）  
> 規範文件：[WhiteRoom 實作計畫](Implementation-Plan.md) ｜ [既有能力對照](Existing-Capabilities.md) ｜ [WhiteRoom 靜態與動態驗收紀錄](../../Documentation/WhiteRoom-Acceptance.md)

---

## 1. 簡介與定位

WhiteRoom 是 CombatAesthetic 套件的官方 3D 最小測試場範例（Sample）。包含兩大測試場景：
1. **第一個交付：移動＋握手互動場景 (`Handshake/Handshake.unity`)**：
   - 依照 Stage 2b 規範，建立第 1 批握手互動：發起伸手、中途取消收手、走近回應、碰觸握緊至成功搖手。
   - 嚴格落實**判定與表現分離**：由 `HandshakeArbitrator` 權威判定碰到與成功；玩家與 AI 僅送出移動與按住/放開 E 的意圖；兩手對準的人形 IK 與顏色反饋僅負責畫面呈現。
   - 提供自主 `HandshakeAiController`，支援 5 種可設定情境（AI 發起、AI 走近回應、AI 取消、握緊期間放開、Idle）。
2. **戰鬥驗證場景 (`Combat/WhiteRoom.unity`)**（目前暫停，保留至戰鬥驗證重啟）：
   - 驗證第三人稱徒手攻防美學：角色八方向移動、跳躍落地恢復、原地/方向出招、警戒慢步、普通防禦、精準彈刀反制、受擊反應與受力因果。
   - 本機玩家、AI 傀儡與遠端連線角色共用同一套 `CombatEntity` 與 `CombatActionIntent` 合法性規則。
   - 可配置情境（1P vs 1 AI、1P vs 2 AI、2P vs 2 AI、2P PvP）與 R 鍵全場統一重設。

> **【網路狀態聲明】**：目前 HUD 內建之「不良網路模擬器」為單一程序內的記憶體模擬佇列（用於驗證延遲/丟包下的時序參數與日誌格式）。尚未接入真實網路傳輸層（Host/Client 或 LAN Socket），面板明確標示真實傳輸層為不可用 (N/A)，不將心跳模擬器宣稱為 LAN 或連線對戰已完成。

---

## 2. 目錄架構與 Assembly 邊界

WhiteRoom 遵循獨立 Assembly 邊界原則，嚴格隔離宿主專案腳本（不依賴 `Assembly-CSharp`），避免範例邏輯污染套件核心 Runtime：

```text
Samples~/WhiteRoom/
├─ README.md                  本文件：匯入、操作、重設與驗收紀錄
├─ Implementation-Plan.md     實作計畫與連線方案評估
├─ Existing-Capabilities.md   既有能力與模組重用分析
├─ Preparation/               階段 0 盤點報告
├─ Combat/                    WhiteRoom.unity（可以對戰的 URP 測試場景，暫停到戰鬥驗證）
├─ Handshake/                 Handshake.unity（移動＋握手互動場景，第一個交付）
│  ├─ HandContactBridge.cs    右手碰撞體接觸觸發器
│  ├─ HandshakeAiController.cs AI 握手情境控制器（發起、回應、取消、提前放開）
│  ├─ HandshakeArbitrator.cs  權威握手判定核心（碰觸、握緊、搖手、收手判定）
│  ├─ HandshakeCharacter.cs   本機玩家控制器（WASD + E 鍵互動，支援覆寫意圖）
│  ├─ HandshakeHUD.cs         IMGUI 互動狀態顯示、情境測試按鈕與 JSON 匯出
│  ├─ HandshakeParticipant.cs 參與者運動學實體（移動、跳躍、姿態流轉、重設）
│  ├─ HandshakeSceneBuilder.cs 握手測試場景自動建造器
│  ├─ HandshakeState.cs       握手狀態機與正式裁決結果列舉
│  ├─ HandshakeVerificationRunner.cs PlayMode 6 大驗收案例自動測試器
│  ├─ HandshakeVisualFeedback.cs 程序化人形手臂 IK 與視鏡反饋表現層
│  └─ Yu5h1Lib...Handshake.asmdef 獨立握手 Assembly
├─ Runtime/                   共用 Sample Runtime assembly
│  ├─ CombatActionIntent.cs   抽象操作意圖（含移動、出招、防禦、InteractHold/Down/Up）
│  ├─ CombatEntity.cs         共用戰鬥實體狀態機與受擊/中斷判定核心
│  ├─ WhiteRoomCharacter.cs   本機玩家戰鬥控制器
│  ├─ WhiteRoomDummy.cs       自主 AI 戰鬥傀儡
│  ├─ WhiteRoomVisuals.cs     程序化機械人形與姿態/時機顏色反饋
│  ├─ WhiteRoomCamera.cs      第三人稱軌道鏡頭（支援 UI 焦點隔離）
│  ├─ WhiteRoomHUD.cs         戰鬥 IMGUI 狀態、情境切換、診斷與 JSON 紀錄匯出
│  ├─ WhiteRoomNetwork.cs     模擬不良網路通道（延遲/抖動/丟包佇列）
│  └─ WhiteRoomSceneBuilder.cs 戰鬥場景環境與情境角色生成器
└─ Tests/                     戰鬥規則與重設測試
```

* **Handshake Assembly**：`Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake`
  * 參照：`Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Runtime`、`Yu5h1Lib.CombatAesthetic`、`Yu5h1Lib.Common`、`Yu5h1Lib.Library`、`Yu5h1Lib.InputHandler`、`Unity.InputSystem`、`Unity.RenderPipelines.Universal.Runtime`。
* **Runtime Assembly**：`Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Runtime`
  * 參照：`Yu5h1Lib.CombatAesthetic`、`Yu5h1Lib.Common`、`Yu5h1Lib.Library`、`Yu5h1Lib.InputHandler`、`Unity.InputSystem`、`Unity.RenderPipelines.Universal.Runtime`。

---

## 3. 宿主相容性與依賴盤點

### 3.1 宿主環境
* **Unity 版本**：`6000.6.0f1`（宿主實機驗證通過，0 編譯錯誤）
* **Render Pipeline**：Universal Render Pipeline (URP 17.6.0)
* **輸入後端**：Unity New Input System，全數透過 `Yu5h1Lib.InputHandler` 統一調度，禁止直接呼叫 `UnityEngine.Input`。

### 3.2 來源與依賴隔離盤點
* **共用層**：引用 `Yu5h1Lib.Common`（輸入抽象 `InputHandler`）與 `Yu5h1Lib.Library`。
* **套件邊界**：完全隔離於舊 2D `CharacterController2D`、`AttributeBehaviour` 及舊 `GameManager`，不依賴任何 2D 專案元件。
* **KFC 專案獨立性**：不涉及 KFC 遷移，維持獨立 Sample 命名空間。

---

## 4. 操作與控制指南 (UX Verified)

### 4.1 握手互動操作（場景：`Handshake/Handshake.unity`）

| 操作輸入 | 動作行為 | 可觀察標準與反饋 |
|---|---|---|
| **E 鍵 (Hold)** | 發起伸手與維持等待 | 站立常態下按住 E：發起伸手（0.50s，右臂向前伸展 0.65m 並內斜 0.30m 對齊身體中線，視鏡轉黃色）。伸手完成後進入等待姿態（維持伸臂微動）。 |
| **E 鍵 (Release)** | 收手取消 | 在伸手或等待期間放開 E：中止互動，平滑收手（0.50s，視鏡還原藍色），返回站立常態。 |
| **W / A / S / D + E 鍵 (Hold)** | 走近回應與握手接觸 | 對方伸手時，按住 E 並操作 WASD 走近（伸手慢步 2.2 m/s）。進入接觸門檻（手部 <=0.65m 或身軀 <=1.05m）時自動判定碰觸，進入握緊 (Grip 0.50s，視鏡轉紫色)。 |
| **持續按住 E 鍵** | 握手成功與搖一下 | 握緊 0.50s 期間雙方持續按住 E：裁定**握手成功**！雙臂上下搖動（Shaking 0.50s，振幅 0.06m，視鏡亮綠色），結束後自動平滑收手。 |
| **握緊期間放開 E 鍵** | 中途放開直接收手 | 握緊期間若任一方提前放開 E：裁定中途放開（第 2 批情況），雙方直接收手返回站立，視鏡熄滅轉灰。 |
| **R 鍵 (Reset)** | 全場統一快速重設 | 重設玩家與 AI 至初始起始位置（相距 2.4m 面對面），狀態還原常態，清除 AI 計時與會話殘留。 |

### 4.2 徒手對戰操作（場景：`Combat/WhiteRoom.unity`，暫停中）

| 操作輸入 | 動作行為 | 可觀察標準與反饋 |
|---|---|---|
| **W / A / S / D** | 視角 8 方向移動 | 角色以鏡頭水平面為基準轉向輸入方向並前進；支援面向畫面近處出招。 |
| **Space** | 起跳與空中位移 | 垂直起跳（高度 1.5m，重力 -20m/s²）；著地時自動恢復常態姿態，**落地後立即恢復移動與出招**。 |
| **Left Ctrl (Hold)** | 蹲下維持 | 按住維持蹲姿（碰撞高度減半）；放開自動平滑恢復站立。 |
| **C 鍵 (Toggle)** | 蹲下切換 | 點按切換蹲下／站立狀態。 |
| **滑鼠左鍵 (Click)** | 原地／方向出招 | 未輸入方向為原地出招；輸入方向時朝指定方位出招。依序歷經：前搖蓄力 (Startup 0.20s，視鏡橘紅預警) ➔ 有效打擊 (Active 0.15s，拳部伸展 0.75m，單次命中保證) ➔ 收招後搖 (Recovery 0.25s)。 |
| **滑鼠右鍵 (Hold)** | 普通防禦與**警戒慢步** | 長按進入防禦姿態（視鏡呈黃色、雙臂交叉），正面 120 度錐形範圍內減傷 80%、減少擊退 70%；**防禦中可操作移動進行警戒慢步 (2.2 m/s)**。長按防禦絕不誤觸發精準彈刀。 |
| **滑鼠右鍵 (Tap)** | **精準防禦 (Parry)** | 快速按下並放開（<=0.22s）進入彈刀姿態（時窗 0.18s，視鏡亮青色）。正面受擊時免疫傷害與擊退，並**實質打斷攻擊者的出招時序**，令攻擊者陷入 1.0 秒暈眩 (Stunned，視鏡呈紫藍色)。 |
| **R 鍵 (Reset)** | 全場統一重設 | 立即重設全場所有角色至起始位置、速度歸零、恢復 100 點生命值，並**同步清空 AI 冷卻、目標及待送網路訊息**。 |
| **F1 鍵** | 開關診斷面板 | 切換右上方不良網路與診斷面板；**滑鼠游標移入面板時自動取得焦點**，鎖定戰鬥按鍵與視角旋轉，戰鬥在背景持續運行。 |
| **情境切換按鈕** | 4 種情境切換 | HUD 面板提供 `1P vs 1 AI`、`1P vs 2 AI`、`2P vs 2 AI`、`2P PvP` 一鍵切換。 |
| **匯出紀錄按鈕** | JSON 結構化日誌 | HUD 面板一鍵將戰鬥日誌匯出至 `Application.persistentDataPath`。 |

---

## 5. 表現與動作識別

WhiteRoom 採用素面白色機械外殼與幾何人形骨架，透過視鏡顏色、雙臂伸展、軀幹縮放與衝量衰減呈現互動與攻防時機：
* **握手發起／等待**：右臂前伸 0.65m 並內斜 0.30m 對齊身體中線，視鏡呈黃色；等待期間帶有呼吸微動。
* **握手碰觸／握緊**：雙手交扣，視鏡呈紫色（Grip 時窗 0.50s）。
* **握手成功／搖一下**：雙手上下同步振動（振幅 0.06m，頻率約 4Hz），視鏡呈綠色。
* **握手中途放開**：視鏡轉灰，雙手平滑返回站姿。
* **戰鬥前搖預警**：視鏡呈橘紅色，拳頭微後縮（給予對手反應時窗）。
* **戰鬥打擊判定**：視鏡呈鮮紅色，拳頭向前突進伸展 0.75m。
* **戰鬥普通防禦**：視鏡呈黃色，雙臂護於胸前。
* **戰鬥精準彈刀**：視鏡呈亮青色，成功時觸發受擊方打斷與暈眩。
* **戰鬥受擊／擊暈**：受擊閃紅並承接物理擊退，暈眩時視鏡呈紫藍色。
* **戰鬥倒地 (KO)**：生命歸零時進入 Incapacitated 姿態，視鏡熄滅。

> 註：獨立動作系統（`Yu5h1Lib/data-driven/animation`）之跨平台動作檔案接用屬於後續規劃，目前以程序化幾何人形表現層驗證。

---

## 6. 實跑驗證與證據

| 驗證項目 | 執行方式 | 結果與證據 |
|---|---|---|
| **套件與 Sample 編譯** | Unity 6000.6.0f1 Host (`Yu5h1LibTest`) | **0 錯誤**，Runtime 與 Handshake 雙 asmdef 均編譯通過。 |
| **握手案例 1：發起後取消** | PlayMode 自動化測試 (`HandshakeVerificationRunner`) | **PASS**。按住 E 伸手 0.15s，放開 E 觸發收手過渡，0.6s 後乾淨返回站立常態。 |
| **握手案例 2：AI發起、玩家回應成功** | PlayMode 自動化測試 (`HandshakeVerificationRunner`) | **PASS**。AI 原地伸手等待，玩家按住 E 走近，手部接觸觸發握緊 (0.50s)，持續按住進入搖手 (0.50s)，正式結果為 `Success`。 |
| **握手案例 3：玩家發起、AI回應成功** | PlayMode 自動化測試 (`HandshakeVerificationRunner`) | **PASS**。角色對調驗證，玩家原地伸手，AI 自動轉向走近，碰觸握緊並搖手成功，正式結果為 `Success`。 |
| **握手案例 4：握緊中途放開收手** | PlayMode 自動化測試 (`HandshakeVerificationRunner`) | **PASS**。握緊期間 AI 於 0.15s 提前放開 E，判定元件裁決為 `EarlyReleaseDuringGrip`（標明第 2 批情況），雙方直接收手返回。 |
| **握手案例 5：AI發起後取消** | PlayMode 自動化測試 (`HandshakeVerificationRunner`) | **PASS**。AI 主動伸手後放開 E，成功驗證 AI 取消收手流程。 |
| **握手案例 6：全場統一快速重設** | PlayMode 自動化測試 (`HandshakeVerificationRunner`) | **PASS**。R 鍵／ResetAll 清除會話狀態，玩家還原 (0, 0, -1.2)，AI 還原 (0, 0, 1.2)，無物理殘留。 |
| **握手結構化日誌匯出** | PlayMode HUD 匯出 | **成功**。產生 `WhiteRoom_Handshake_Logs_20261031_022558.json`，完整記錄 6 大案例判定時序。 |
| **戰鬥最小攻防循環** | PlayMode 實機操作 | P1 出招命中 AI，單次打擊成立（20 傷害），受力位移正常。 |
| **戰鬥精準防禦打斷** | PlayMode 實機操作 | P1 於 0.18s 內點按右鍵，正面承受攻擊時傷害為 0，且**攻擊者動作立即中斷並陷入 1.0s 暈眩**。 |
| **戰鬥普通防禦與警戒慢步** | PlayMode 實機操作 | 長按右鍵承受攻擊時減傷 80%（扣 4 滴），移動時保持防禦姿態慢步 (2.2 m/s)。 |
| **戰鬥跳躍落地恢復** | PlayMode 實機操作 | 按 Space 起跳，著地後 `_controller.isGrounded` 正常觸發，狀態恢復 `Idle`，可立即出招與移動。 |
| **戰鬥全場重設** | PlayMode R 鍵 / 面板按鈕 | P1 與 AI 同步還原位置、生命 100/100、清除 AI 決策計時與模擬佇列，兩入口行為一致。 |
| **戰鬥情境配置** | PlayMode HUD 切換 | 成功切換 1P vs 1 AI (2人)、1P vs 2 AI (3人)、2P vs 2 AI (4人)、2P PvP (2人)。 |
| **Windows 建置** | Unity StandaloneWindows64 | **建置成功**，產生 `Builds/WhiteRoom/WhiteRoom.exe`（667,136 bytes）。 |

---

## 7. 後續階段規劃

1. **握手第 2 批**：惡作劇收手（對方握空反應）、同時收手、進階姿勢過渡。
2. **握手第 3 批**：握手比力氣（Strength Contest 力量對抗機制）。
3. **戰鬥驗證重啟**：依據計畫時程，銜接至真實網路傳輸層整合。
