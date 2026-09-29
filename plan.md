# Combat Aesthetic — 動作對戰美學框架與 Yu5h1Lib 拆包計畫

## 0. 當前定位

CombatAesthetic 追求的不是「所有遊戲系統都放在同一包」，而是**動作、對戰、打擊感、動畫/特效/音效/UI 同步呈現的美學整合層**。

也就是說：

- CombatAesthetic 應該保留「動作對戰體驗」本身。
- 可被其他遊戲重用的系統層級能力，應拆到 Yu5h1Lib 對應 package。
- 外包 2D 遊戲目前可以先使用 CombatAesthetic 作為整合包，但長期應讓它依賴更乾淨的 Yu5h1Lib packages。

CombatAesthetic 最後比較像：

- action-combat gameplay preset
- combat feel orchestration
- 角色戰鬥整合 component
- animation / FX / audio / hit reaction 的高階串接
- samples、authoring presets、整合測試場景

而不是：

- 通用 UI package
- 通用 2D physics package
- 通用 input package
- 通用 AI package
- 通用 network package
- 通用 editor utility package

## 1. Yu5h1Lib Package 拆分原則

### 留在 CombatAesthetic 的內容

這些內容直接服務「動作對戰美學」，可以先留在 CombatAesthetic 或未來拆成 `Combat*` package：

- hit / hurt / knockback / hitstun / cancel window
- action command interpretation
- combat-oriented character controller composition
- combat animation bridge
- skill timing and cast windows
- hitbox/hurtbox presentation bridge
- combat camera feel
- combat UI feedback orchestration
- action feel samples and presets

### 往 Yu5h1Lib 拆出的內容

這些是系統層級或應用層級的通用能力，應針對相應類別新增 package 或併入既有 package：

| 目標 Package | 應拆內容 |
| --- | --- |
| `GameplayCore` | AttributeStat、EnergyInfo、AttributeType、AffectType、DefeatedReason、通用 gameplay model |
| `Gameplay2D` | ITransform2D、2D movement/detector abstraction、通用 2D gameplay component |
| `2D` | Collider2D/Physics2D extension、Line2D、2D editor helper、PSD/sprite 工具 |
| `Animation` | Animator extension、SMB base/helper、IK、PlayableDirector/Timeline adapter |
| `InputSystem` | Player input adapter、command source bridge |
| `AI` | Route、Patrol、Autopilot 的通用路線/決策基礎 |
| `UI` | UIControl、stat view、prompt/menu/dialog 的通用 UI 能力 |
| `common` | Pool、generic event、resource helper、通用 Mono utility |
| `Network` | command channel、local loopback、lag simulator、snapshot/serialization contract |

## 2. 建議 Package 形狀

短期不要一次拆太細。先建立穩定依賴方向：

```text
Yu5h1Lib Core/common
  -> GameplayCore
  -> Gameplay2D
  -> CombatCore
  -> Combat2D
  -> CombatAnimation
```

之後再拆 adapter：

```text
CombatInput
CombatAI
CombatUI
CombatFX
CombatNetworkAdapter
```

核心規則：

- Input、AI、Network 都只是 command source。
- UI、Animation、FX、Audio 都只是 presentation output。
- Combat core 不直接依賴 Unity UI、Animator、InputSystem、具體網路傳輸。
- 2D package 不依賴 GameManager、SceneController、CharacterController2D。
- CombatAesthetic 可以依賴多個 package 做整合，但低層 package 不反向依賴 CombatAesthetic。

## 3. 目前 CombatAesthetic 的主要解耦點

### CharacterController2D

目前混合了 movement、attribute damage、host input、detector、scanner、caster、teleport、PlayerPrefs、invincibility visual、GameManager pause。

拆法：

- movement/detector 抽到 `Gameplay2D` 或 `2D`
- attribute model 抽到 `GameplayCore`
- host input 改成 command source interface
- scanner/caster/teleportable 改成 optional feature 或 adapter
- 視覺閃爍與 UI/Audio 不留在 controller core

### AnimatorCharacterController2D

目前混合 Animator root motion、SMB、Skill、FX cast、AudioManager、UI/cursor。

拆法：

- Animator bridge 放 `CombatAnimation`
- Skill authoring 可先留在 Combat，之後分 `CombatCore` + Unity authoring adapter
- FX/audio/UI 改事件化，不讓 animation controller 直接操作所有系統

### AttributeBehaviour

目前 attribute model、MonoBehaviour lifecycle、UI refresh 綁在一起。

拆法：

- `AttributeStat` 等純資料先抽出
- `AttributeBehaviour` 成為 Unity adapter
- UI 改訂閱 attribute changed event，不由 attribute 直接持有 UI

### HostData2D / PlayerHost / Autopilot

這是很好的 command source 起點，但目前 `HostData2D` 直接知道 `CharacterController2D`。

拆法：

- 抽 `ICharacterCommandSource`
- PlayerHost 放 input adapter
- Autopilot 放 AI adapter
- Network/replay 未來也走同一 command source contract

## 4. 分階段計畫

### Phase -1：先清晰化重構方向

在拆 package 前，先把舊代碼分成四種角色：

- Model：純資料與規則，例如 Attribute、Energy、Cost、Command data。
- Command：玩家、AI、Network、Replay 提供的操作意圖。
- Controller：消費 command 並推進角色/戰鬥狀態。
- Presentation：Animator、FX、Audio、UI、Camera。

目前最該先處理的不是 ECS 或 DI container，而是：

- `AttributeBehaviour`：拆開資料、狀態變更事件、UI refresh。
- `HostData2D`：整理成 command source 抽象。
- `CharacterController2D`：縮小成角色執行器。
- `AnimatorCharacterController2D`：定位為動畫/表現橋。

Yu5h1Lib 現有的 AdapterFactory / RuntimeInitializeOnLoad 注入模式可以用在「同目的、多實作」的位置，例如 command source、attribute view、hit receiver、presentation adapter。不要為了套術語而把所有東西都注入化。

### Phase 0：盤點與標記

- 建立 class-to-package mapping。
- 標記 generic utility、gameplay model、combat-specific integration。
- 不搬檔、不改 namespace。

### Phase 1：抽 GameplayCore

- 先移出純資料與 enum。
- 讓 UI、Character、Skill 都依賴同一份 gameplay model。

### Phase 2：抽 Gameplay2D / 2D

- 移出通用 2D extension、line/collider/physics helper。
- 避免帶入 GameManager、SceneController、CharacterController2D。

### Phase 3：建立 command source 邊界

- 把玩家、AI、未來 network/replay 都收斂到 command source。
- Combat controller 只消費 command，不知道來源。

### Phase 4：拆 Combat2D / CombatAnimation

- `CharacterController2D` 成為 Combat2D 整合層。
- `AnimatorCharacterController2D` 與 SMB 進 CombatAnimation。
- UI/FX/Audio 改成 presentation event。

### Phase 5：拆 adapter packages

- `CombatInput`
- `CombatAI`
- `CombatUI`
- `CombatFX`
- `Network`

## 5. 原始長期願景：單機/連線一體化

以下保留原本 Combat Aesthetics 的長期方向，用於提醒拆包後的 CombatCore/Network 邊界。

## 1. 核心願景與世界觀 (System Architecture Overview)
本框架旨在處理**「時空連續體中，多個獨立物件的行為干涉與狀態同步系統」**。
為同時服務「單機 PVE（完全離線）」與「高打擊感連線 PVP」，框架將「網路傳輸」視為純粹的通訊通道，而非遊戲邏輯。核心引擎眼裡永遠只有一種運作方式：
> **「客戶端負責發送操作指令，伺服器負責計算絕對真相。」**

### 商業與開發優勢
* **前後端代碼高度復用：** 核心計算邏輯採用純 C#（Pure C# Class Library）編寫，不依賴特定遊戲引擎 API。可直接作為 DLL 供 .NET 後端伺服器與 Unity 前端共用。
* **內建開發除錯黑科技：** 在本機單機測試時，可透過記憶體佇列（Memory Queue）建立「本機虛擬伺服器（Local Loopback Server）」。只要在通道中加入 `Task.Delay()`，即可在不架設任何實體網路的環境下，單機模擬並除錯各種高延遲（Ping 值）下的物理判定。

---

## 2. 核心架構分層 (System Layers)

系統嚴格遵循「邏輯」與「表現」的絕對分離：

1. **確定性狀態計算機 (Model / Core State)：**
   純 C# 的數學運算。負責接收指令、處理 Hitbox 碰撞、速度向量、力學傳遞以及「受擊硬直時間」。
2. **通訊介面適配器 (Network Channel / Adapter)：**
   * **單機模式（Local）：** 透過記憶體佇列（0 延遲）直接將指令送入本機虛擬伺服器。
   * **連線模式（Network）：** 換上網路實作（如 UDP / WebSockets），將指令序列化為位元流送至雲端。
3. **渲染表現層 (View / Unity Implementation)：**
   Unity 腳本只負責訂閱狀態數據，依據結果播放對應的 Animator 動畫、特效與音效。

---

## 3. 當前關鍵工程挑戰與解決方案 (Core Bottlenecks)

要調校出達 95 分、極致流暢的近戰動作手感，黑盒子必須克服以下三個地獄級技術點：

### 挑戰 A：100% 的「確定性（Determinism）」與浮點數地獄
* **問題：** C# 的 `float` 在不同的行動裝置 CPU 架構上，算出來的小數點後微小位數可能存在硬體優化誤差。在 Rollback（回滾）重算多幀後，微小誤差會被放大，導致前後端世界線撕裂（Desync）。
* **方案：** 黑盒子的空間座標與物理計算不能使用浮點數，必須全面採用**定點數數學庫（Fixed-Point Math）**或純整數（Integer）進行位移與碰撞模擬。

### 挑戰 B：狀態快照（Snapshot）引起的記憶體海嘯
* **問題：** 遊戲一秒跑 60 幀，回滾機制要求系統必須默默在後台備份過去 1~2 秒內每一幀的所有物體狀態。若頻繁 `new` 物件存檔，會引發嚴重的垃圾回收（GC Spike），導致手機劇烈卡頓。
* **方案：** 必須設計嚴格的**記憶體池（Object Pooling）**。事先宣告固定大小的 Byte 陣列，利用結構體（Struct）直接在記憶體位址上進行高速的位元複製（Buffer 拷貝），確保運作過程「零 GC 宣告」。

### 挑戰 C：視覺欺騙與動畫平滑彌補 (Visual Interpolation)
* **問題：** 現代動作連線採用 Rollback 技術，本機前端 0 延遲播放動畫。當 0.05 秒後收到伺服器真相發現「猜錯了」，黑盒子的數據會在 1 毫秒內完成重算。如果直接覆蓋畫面，物體會產生突兀的「瞬間移動（Teleporting）」。
* **方案：** 前端渲染層必須具備**動態內插追趕演算法**。當發現黑盒子真實座標與目前 3D 模型位置不符時，模型不能瞬移，而是在接下來的 2~3 幀內以肉眼難察覺的高速「滑行追趕」至正確位置，欺騙人類視覺以達到絲滑感。

---

## 4. 階段性開發里程碑 (Milestones)

* [ ] **Milestone 1：** 撰寫純 C# 類別庫，定義不含 Unity API 的角色狀態機與 `IActionPayload`、`INetworkChannel` 介面。
* [ ] **Milestone 2：** 實作「本機虛擬伺服器」與「記憶體通訊隊列」，達成同螢幕雙搖桿操縱兩個方塊人對打的純數學驗證。
* [ ] **Milestone 3：** 實作「受擊硬直（Hitstun）、動能擊退（Knockback）與碰撞框（Hitbox）判定」。
* [ ] **Milestone 4：** 寫入 `LagSimulatorChannel` 故意滯留封包，開始在單機環境下調校 Rollback（回滾）與視覺平滑補償演算法。
