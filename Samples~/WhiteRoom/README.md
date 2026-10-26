# WhiteRoom：3D PvE 白色測試場 Sample

> 套件名稱：`com.yu5h1.combataesthetic`  
> 基準宿主：Unity `6000.3.9f1` URP / `6000.6.0f1`（Yu5h1LibTest 宿主實機驗證通過）  
> 規範文件：[WhiteRoom 實作計畫](Implementation-Plan.md) ｜ [既有能力對照](Existing-Capabilities.md) ｜ [階段 0 準備產出](Preparation/README.md)

---

## 1. 簡介與定位

WhiteRoom 是 CombatAesthetic 套件的官方 3D 徒手 PvE / PvP 測試場範例（Sample）。其核心使命為：
1. **驗證第三人稱動作對戰美學**：以可累積、可重用的方式展示角色八方向移動、跳躍、原地/方向出招、普通與精準防禦、受擊反應與受力因果。
2. **驗證跨平台動作製作管線**：作為獨立動作系統（`Yu5h1Lib/data-driven/animation`）跨平台資料在 Unity 宿主環境的消費與重播驗證端。
3. **提供乾淨的情境重設機制**：允許測試者（人類或 AI）在白色空間中反覆驗證對戰規則，在不累積受擊殘留與速度快照的前提下快速重置。
4. **驗證共同戰鬥流程與不良網路同步**：本機玩家、AI 傀儡與遠端連線客戶端共用同一套 `CombatEntity` 操作合法性與時序規則，並透過內建的延遲/抖動/丟包模擬器檢驗時序判定。

---

## 2. 目錄架構與 Assembly 邊界

WhiteRoom 遵循獨立 Assembly 邊界原則，嚴格隔離宿主專案腳本（不依賴 `Assembly-CSharp`），避免將範例邏輯反向污染套件核心 Runtime：

```text
Samples~/WhiteRoom/
├─ README.md                  本文件：匯入、操作、重設與驗收入口
├─ Implementation-Plan.md     完整實作計畫與階段規劃
├─ Existing-Capabilities.md   既有能力與模組重用分析
├─ Preparation/               階段 0 唯讀盤點報告與測試案例
├─ Scenes/                    WhiteRoom 3D URP 測試場景
├─ Prefabs/                   簡化人形機器人、AI 與測試情境物件
├─ Art/                       素面白色機械外殼、材質、動作資料與授權清冊
├─ Runtime/                   Sample 組合與情境控制腳本（獨立 asmdef）
├─ Settings/                  情境配置、角色能力與主題設定
└─ Tests/                     戰鬥規則、判定區間與重設自動化檢查
```

* **Runtime Assembly**：`Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Runtime`
  * 參照：`Yu5h1Lib.CombatAesthetic`、`Yu5h1Lib.Common`、`Yu5h1Lib.UI`、`Unity.InputSystem`。

---

## 3. 宿主相容性與必要依賴

### 3.1 宿主環境要求
* **Unity 版本**：`6000.3.9f1`（計畫基準） / `6000.6.0f1`（Yu5h1LibTest 宿主實機驗證通過，0 編譯錯誤）
* **Render Pipeline**：Universal Render Pipeline (URP)
* **輸入後端**：Unity New Input System (`com.unity.inputsystem`)，透過 `Yu5h1Lib.InputHandler` 統一調度

### 3.2 必要套件相依
宿主專案需具備以下套件參照（由 `package.json` 宣告）：
* `com.yu5h1.library`（Core 基礎型別與轉向幾何）
* `com.yu5h1.common`（通用事件、`InputHandler` 輸入採集抽象）
* `com.yu5h1.inputsystem`（New Input System 後端橋接）
* `com.yu5h1.animation`（動畫相關擴充）
* `com.yu5h1.ui`（主題切換與除錯資訊呈現）
* `com.unity.inputsystem`（1.7.0+）

---

## 4. 操作與控制指南

| 操作輸入 | 動作行為 | 可觀察標準與反饋 |
|---|---|---|
| **W / A / S / D** | 視角八方向移動 | 角色以鏡頭水平面為基準轉向輸入方向並平滑前進；支援面向畫面近處出招。 |
| **Space** | 垂直起跳與滯空 | 按下起跳，具備明確起跳、滯空下落與著地貼地；受擊/硬直期間無法起跳。 |
| **Left Ctrl (Hold)** | 蹲下姿態維持 | 按住維持蹲姿（碰撞高度與移動速度減半）；放開自動平滑恢復站立。 |
| **C 鍵 (Toggle)** | 蹲下姿態切換 | 點按切換蹲下／站立狀態。 |
| **滑鼠左鍵 (Click)** | 原地／方向出招 | 未輸入方向為原地出招；輸入方向時朝指定方位出招。依序歷經：前搖蓄力 (Startup 0.20s，視鏡橘紅預警) ➔ 有效打擊 (Active 0.15s，拳部伸展 0.75m，單次命中保證) ➔ 收招後搖 (Recovery 0.25s)。 |
| **滑鼠右鍵 (Hold)** | 警戒與普通防禦 | 角色進入防禦姿態（雙臂交叉、視鏡呈黃色），正面 120 度錐形範圍內減傷 80%、減少擊退 70%；背後不防禦。 |
| **滑鼠右鍵 (Tap)** | 精準防禦 (Parry) | 進入防禦姿態 0.18 秒內受到正面攻擊判定時觸發。免疫傷害與擊退，並**實質打斷攻擊者的出招時序**，令攻擊者陷入 1.0 秒暈眩癱瘓 (Stunned，視鏡呈紫藍色)。 |
| **R 鍵 (Reset)** | 全場統一重設 | 立即重設全場所有角色（玩家、AI 傀儡）至起始位置、速度歸零、恢復 100 點生命值，清除受擊殘留與出招紀錄。 |
| **2P 同機控制** | 雙人攻防模擬 | HUD 可啟用 2P 模式（方向鍵移動，小鍵盤 1 攻擊，小鍵盤 2 防禦/彈刀），供同機雙端互相攻防與測試。 |
| **不良網路模擬** | 延遲與丟包測試 | 右上角面板可設定 0~300ms 延遲、0~50ms 抖動與 0~25% 丟包率，測試非理想連線下的攻防時序。 |

---

## 5. 跨平台動作資料接用

WhiteRoom 支援讀取獨立動作系統保存的非二進位（去 `.anim` / 去 FBX）動作姿態數據：
1. 資料由外部編輯器或 Web Viewer 產生並儲存為跨平台資料。
2. WhiteRoom 執行階段透過 `RuntimePoseController` 與 `HumanPoseHandler` 將動作映射至固定人形骨架。
3. 動作播放期間依據 `CharacterSMB` 權重動態計算切線位移，受擊或取消時保留原有慣性衝量（受力因果）。

---

## 6. 驗證與驗收流程

1. **靜態相容性**：確認套件未引用宿主特定代碼，Standalone Player 編譯無警告。
2. **場景載入**：開啟 `Samples~/WhiteRoom/Scenes/WhiteRoom.unity`，確認素面白色測試場與機械角色模型正確呈現。
3. **PvE / PvP 循環驗證**：進入 PlayMode，依序操作八方向移動、跳躍、原地/方向出招、防禦、精準彈刀反制與全場重設，比對各招式之打擊感、防禦反饋與物理動量延續。
4. **不良網路驗證**：拉動 HUD 的 Latency 與 Packet Loss 滑桿，檢驗延遲與丟包情境下的攻防時序與狀態一致性。
