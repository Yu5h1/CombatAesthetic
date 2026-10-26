# WhiteRoom：3D PvE 白色測試場 Sample

> 套件名稱：`com.yu5h1.combataesthetic`  
> 基準宿主：Unity `6000.3.9f1` URP  
> 規範文件：[WhiteRoom 實作計畫](Implementation-Plan.md) ｜ [既有能力對照](Existing-Capabilities.md) ｜ [階段 0 準備產出](Preparation/README.md)

---

## 1. 簡介與定位

WhiteRoom 是 CombatAesthetic 套件的官方 3D 徒手 PvE 測試場範例（Sample）。其核心使命為：
1. **驗證第三人稱動作對戰美學**：以可累積、可重用的方式展示角色八方向移動、跳躍、原地/方向出招、普通與精準防禦、受擊反應與受力因果。
2. **驗證跨平台動作製作管線**：作為獨立動作系統（`Yu5h1Lib/data-driven/animation`）跨平台資料在 Unity 宿主環境的消費與重播驗證端。
3. **提供乾淨的情境重設機制**：允許測試者（人類或 AI）在白色空間中反覆驗證對戰規則，在不累積受擊殘留與速度快照的前提下快速重置。

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
  * 參照：`Yu5h1Lib.CombatAesthetic`、`Yu5h1Lib.Core`、`Yu5h1Lib.Common`、`Yu5h1Lib.Animation`、`UnityEngine.InputSystem`。

---

## 3. 宿主相容性與必要依賴

### 3.1 宿主環境要求
* **Unity 版本**：`6000.3.9f1`（LTS 基準版本）
* **Render Pipeline**：Universal Render Pipeline (URP)
* **輸入後端**：Unity New Input System (`com.unity.inputsystem`)

### 3.2 必要套件相依
宿主專案需具備以下套件參照（由 `package.json` 宣告）：
* `com.yu5h1.library`（Core 基礎型別與轉向幾何）
* `com.yu5h1.common`（通用事件、輸入採集抽象）
* `com.yu5h1.animation`（`GroundIKTarget` 足部貼地、`BalanceHipController` 重心平衡）
* `com.yu5h1.ui`（主題切換與除錯資訊呈現）
* `com.unity.inputsystem`（1.7.0+）

---

## 4. 操作與控制指南

| 操作輸入 | 動作行為 | 可觀察標準與反饋 |
|---|---|---|
| **W / A / S / D** | 視角八方向移動 | 角色以鏡頭水平面為基準轉向輸入方向並平滑前進；支援面向畫面近處出招。 |
| **Space** | 垂直起跳與滯空 | 按下起跳，具備明確起跳前搖、滯空下落與著地反應；支援落地足部貼地。 |
| **滑鼠左鍵 (Click)** | 原地／方向出招 | 未輸入方向為原地直拳/刺拳；輸入方向時朝指定方位出招。具備攻擊命中判定窗與前後搖。 |
| **滑鼠右鍵 (Hold)** | 警戒與普通防禦 | 角色進入警戒姿態，面向範圍內自動格擋來襲攻擊；背後不防禦。 |
| **滑鼠右鍵 (Tap)** | 精準防禦 (Parry) | 在受擊判定生效瞬間短暫按下放開，成功時中斷敵方連續攻擊並觸發卸力反饋。 |
| **R 鍵 (Reset)** | 快速重設情境 | 立即清除所有當前速度、殘留外力衝量、受擊僵直與命中紀錄，恢復起始基準。 |

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
3. **PvE 循環驗證**：進入 PlayMode，依序操作八方向移動、跳躍、原地/方向出招、防禦與重設，比對各招式之打擊感、防禦反饋與物理動量延續。
