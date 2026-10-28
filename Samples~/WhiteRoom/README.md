# WhiteRoom：3D PvE 白色測試場 Sample

> 套件名稱：`com.yu5h1.combataesthetic`  
> 驗證宿主：Unity `6000.6.0f1` URP（`W:/UnityProject/Yu5h1LibTest` 實測 0 編譯錯誤、PlayMode 攻防循環驗證通過、Windows x64 建置成功）  
> 規範文件：[WhiteRoom 實作計畫](Implementation-Plan.md) ｜ [既有能力對照](Existing-Capabilities.md) ｜ [WhiteRoom 靜態與動態驗收紀錄](../../Documentation/WhiteRoom-Acceptance.md)

---

## 1. 簡介與定位

WhiteRoom 是 CombatAesthetic 套件的官方 3D 徒手 PvE / PvP 最小測試場範例（Sample）。其核心使命為：
1. **驗證第三人稱動作對戰美學**：以可累積、可重用的方式展示角色八方向移動、跳躍落地恢復、原地/方向出招、警戒慢步、普通防禦、精準彈刀反制、受擊反應與受力因果。
2. **共用戰鬥判定流程（CombatEntity）**：本機玩家、AI 傀儡與遠端連線角色共用同一套 `CombatEntity` 與 `CombatActionIntent` 合法性規則，分清輸入意圖、戰鬥判定與畫面呈現。
3. **可配置情境與全場乾淨重設**：支援 `1P vs 1 AI`、`1P vs 2 AI`、`2P vs 2 AI` 與 `2P PvP` 多種情境，並透過 R 鍵或面板統一重置生命、位置、AI 目標/冷卻與網路佇列。
4. **診斷面板與焦點隔離**：操作診斷面板時自動凍結戰鬥指令與視角旋轉，戰鬥在背景持續即時進行；提供結構化 JSON 日誌匯出。

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
├─ Scenes/                    WhiteRoom.unity（URP 測試場景）
├─ Runtime/                   獨立 asmdef 執行程式
│  ├─ CombatActionIntent.cs   抽象操作意圖與輸入來源介面
│  ├─ CombatEntity.cs         共用戰鬥實體狀態機與受擊/中斷判定核心
│  ├─ WhiteRoomCharacter.cs   本機玩家控制器（P1 鍵鼠 / P2 鍵盤）
│  ├─ WhiteRoomDummy.cs       自主 AI 傀儡（逼近、週期進攻、依預警防禦/彈刀）
│  ├─ WhiteRoomVisuals.cs     程序化機械人形與姿態/時機顏色反饋
│  ├─ WhiteRoomCamera.cs      第三人稱軌道鏡頭（支援 UI 焦點隔離）
│  ├─ WhiteRoomHUD.cs         IMGUI 狀態、情境切換、診斷與 JSON 紀錄匯出
│  ├─ WhiteRoomNetwork.cs     模擬不良網路通道（延遲/抖動/丟包佇列）
│  └─ WhiteRoomSceneBuilder.cs 場景環境與情境角色生成器
└─ Tests/                     戰鬥規則與重設測試
```

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

首階段 WhiteRoom 採用素面白色機械外殼與幾何人形骨架，透過視鏡顏色、雙臂伸展、軀幹縮放與衝量衰減呈現攻防時機：
* **前搖預警**：視鏡呈橘紅色，拳頭微後縮（給予對手反應時窗）。
* **打擊判定**：視鏡呈鮮紅色，拳頭向前突進伸展 0.75m。
* **普通防禦**：視鏡呈黃色，雙臂護於胸前。
* **精準彈刀**：視鏡呈亮青色，成功時觸發受擊方打斷與暈眩。
* **受擊／擊暈**：受擊閃紅並承接物理擊退，暈眩時視鏡呈紫藍色。
* **倒地 (KO)**：生命歸零時進入 Incapacitated 姿態，視鏡熄滅。

> 註：獨立動作系統（`Yu5h1Lib/data-driven/animation`）之跨平台動作檔案接用屬於後續規劃，首階段 Runtime 尚未包含 Clip/FBX 外部載入器。

---

## 6. 實跑驗證與證據

| 驗證項目 | 執行方式 | 結果與證據 |
|---|---|---|
| **套件編譯** | Unity 6000.6.0f1 Host (`Yu5h1LibTest`) | **0 錯誤**，已清除所有過時 API 警告。 |
| **最小攻防循環** | PlayMode 實機操作 | P1 出招命中 AI，單次打擊成立（20 傷害），受力位移正常。 |
| **精準防禦打斷** | PlayMode 實機操作 | P1 於 0.18s 內點按右鍵，正面承受攻擊時傷害為 0，且**攻擊者動作立即中斷並陷入 1.0s 暈眩**。 |
| **普通防禦與警戒慢步** | PlayMode 實機操作 | 長按右鍵承受攻擊時減傷 80%（扣 4 滴），移動時保持防禦姿態慢步 (2.2 m/s)。 |
| **跳躍落地恢復** | PlayMode 實機操作 | 按 Space 起跳，著地後 `_controller.isGrounded` 正常觸發，狀態恢復 `Idle`，可立即出招與移動。 |
| **全場重設** | PlayMode R 鍵 / 面板按鈕 | P1 與 AI 同步還原位置、生命 100/100、清除 AI 決策計時與模擬佇列，兩入口行為一致。 |
| **情境配置** | PlayMode HUD 切換 | 成功切換 1P vs 1 AI (2人)、1P vs 2 AI (3人)、2P vs 2 AI (4人)、2P PvP (2人)。 |
| **結構化紀錄匯出** | PlayMode HUD 按鈕 | 成功於 `Application.persistentDataPath` 產生 JSON 檔案（12,334 bytes）。 |
| **Windows 建置** | Unity StandaloneWindows64 | **建置成功**，產生 `Builds/WhiteRoom/WhiteRoom.exe`（667,136 bytes）。 |

---

## 7. 後續真連線階段 (批次 C) 評估與建議

依據 WhiteRoom 實作計畫，下一批將進行真實網路傳輸整合。請參閱 [連線方案評估報告](../../Documentation/plans/Networking-Architecture-Evaluation.md) 與計畫尚未決定項目。
