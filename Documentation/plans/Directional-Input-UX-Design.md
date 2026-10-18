# 方向鍵與移動操控 UX 設計規格書 (Directional Input & Interaction UX)

> Task ID: directional-input-ux-design  
> 狀態：設計草案，架構分層與 UX 體驗規格  
> 關聯文件：
> - 動作美學定位與控制邊界：[plan.md](../../plan.md)
> - 重構與指令源抽象指引：[Refactor-Direction-Guide.md](Refactor-Direction-Guide.md)
> - 優先任務與決策清單：[Priority-Decision-Report.md](Priority-Decision-Report.md)
> - 核心技術需求：[ARPG-Core-Technical-Requirements.md](ARPG-Core-Technical-Requirements.md)

---

## 0. 系統階層與關係

```text
硬體設備 (Hardware: Keyboard / Gamepad / Touch Screen)
  │
  ▼
[Layer 1: Input 採集層] (Yu5h1Lib.Input / Device Polling)
  ├─ 鍵盤 WASD / 方向鍵原始按壓
  ├─ 手把類比搖桿 / 十字鍵 Raw Axis
  └─ 觸控螢幕手指接觸與拖曳座標 (Touch Pointer Events)
  │
  ▼
[Layer 2: UX / Interaction 操控體驗層] (本文件核心職責)
  ├─ 虛擬搖桿 / 虛擬十字鍵操控模型 (Floating Joystick vs. Fixed D-Pad)
  ├─ 訊號整形：死區 (Deadzone)、非線性靈敏度曲線、8 向角度吸附
  ├─ 操作手勢節奏：輕推慢走 / 深推奔跑、雙擊快速衝刺 (Double-Tap Dash)
  ├─ 座標投影：螢幕空間 $\rightarrow$ 攝影機視角相對 $\rightarrow$ 世界水平面
  └─ 視覺反饋與操作舒適度：虛擬搖桿透明度、反饋波紋、鍵位自定義
  │
  ▼
[Layer 3: Command / Intent 意圖抽象層] (ICharacterCommandSource)
  └─ 語意化移動指令：MoveIntent { Direction: Vector2, Magnitude: float, IsDashing: bool }
  │
  ▼
[Layer 4: Gameplay 角色物理響應層] (Combat2D / CharacterController2D)
  ├─ 角色加速度、減速慣性與最高航速
  ├─ 轉向速率限制 (Turn Rate) 與角色面朝角度
  └─ 戰鬥約束：攻擊動作中的轉向鎖定 (Turn Lock)、受擊硬直 (Hitstun) 禁行
```

---

## 1. 架構歸屬與職責劃分：屬於 Input？Gameplay？還是 UX？

「方向鍵」在現代遊戲架構中不能粗暴地塞入單一模組，它橫跨四個獨立層次：

| 層級 | 歸屬模組 | 核心職責 | 典型處理內容 |
| --- | --- | --- | --- |
| **1. Input 採集層** | `Yu5h1Lib.Input` / 底層驅動 | 採集實體訊號 | 監聽鍵盤 KeyCode、手把類比軸、觸控點事件，輸出未經修飾的 Raw Data。 |
| **2. UX 操控體驗層** | **UX / Interaction** (本文件) | **決定操控感受與人機介面** | 虛擬搖桿判定區、死區過濾、輸入平滑、視角座標轉換、雙擊檢測、觸覺與視覺回饋。 |
| **3. Command 意圖層** | `GameplayCore` / 介面契約 | 定義語意化意圖 | 將 UX 運算結果抽象為 `MoveIntent`，隔離玩家、AI、連線遠端或錄影重播。 |
| **4. Gameplay 執行層** | `Combat2D` / `Gameplay2D` | 執行世界物理與規則 | 根據角色當前狀態（攻擊中、被擊退中、蓄力中）決定是否接受移動意圖，計算速度與位移。 |

**結論**：
* 「方向鍵的信號讀取」屬於 **Input**。
* 「玩家如何按、好不好按、容錯率、手勢識別、視角轉換與回饋」屬於 **UX / Interaction**。
* 「角色在遊戲內走多快、轉向慣性、能不能動」屬於 **Gameplay**。

---

## 2. 為什麼方向鍵需要嚴謹的 UX 設計？

在 ARPG 與動作對戰遊戲（CombatAesthetic）中，方向操控的品質決定了整體的操控敏捷度（Responsiveness）與走位微操感（Micro-spacing）：
1. **多端操控體驗不一致**：鍵盤是離散的二進制（0 或 1，瞬間達到最大值）；手把是連續的類比訊號；手機螢幕是無物理邊界的反饋盲區。若無 UX 層統一調校，不同設備的手感將嚴重割裂。
2. **走位精確度與誤觸**：斜向走位（如右前 45 度）若無合理的扇形角度分配與防抖動濾波，容易造成角色在直走與斜走之間頻繁抖動抽搐。
3. **戰鬥節奏協同**：格鬥與動作遊戲需要明確的「走-跑切換」與「走位-轉向-出招鎖定」，若方向輸入未與戰鬥狀態機嚴密對齊，會產生出招滑步或轉向延遲的漂浮感。

---

## 3. 多端操控模型詳細 UX 規範

### 3.1 觸控螢幕操控 (Mobile Touch UX)
行動端提供兩種主要 UX 方案，建議在遊戲設定中提供切換：

#### 方案 A：動態浮動搖桿 (Floating / Dynamic Joystick) — **推薦預設**
* **觸發機制**：左半螢幕任意點下時，以落點為搖桿中心（Center Pivot），手指滑動帶動把頭（Knob）。
* **活動範圍**：
  * **內死區 (Inner Deadzone)**：中心半徑 $10\%$ 內忽略微小滑移，防止手指靜態微顫。
  * **步行區 (Walk Zone)**：半徑 $10\% \sim 70\%$，輸出線性增長推力，角色以行走姿態移動。
  * **奔跑區 (Run Zone)**：半徑 $70\% \sim 100\%$，輸出最大推力，角色進入跑步姿態。
  * **動態跟隨 (Follow Distance)**：當手指拉扯超過 $120\%$ 半徑時，搖桿中心平滑向手指方向牽引，避免手指滑到螢幕邊緣後失去回中操作彈性。

#### 方案 B：虛擬固定十字鍵 (Virtual Fixed D-Pad)
* **適用場景**：偏好經典街機/格鬥手感，需要精確 8 向走位與取消輸入的玩家。
* **扇形角度劃分**：
  * 正方向（上、下、左、右）各分配 $60^\circ$ 扇形區。
  * 斜向方向（左上、右上、左下、右下）各分配 $30^\circ$ 扇形區（防止一般直行時誤觸斜向）。
* **視覺反饋**：按下對應方向箭頭時，給予瞬時按壓高亮與擴散光圈，提供視覺確認感。

### 3.2 手把類比搖桿 (Gamepad Joystick UX)
* **死區校正 (Axial vs. Radial Deadzone)**：
  * 採用**圓形死區 (Radial Deadzone)** 而非軸向死區（十字死區），保證 $360^\circ$ 旋轉時向量長度的均勻過渡。
  * 預設死區閾值：內死區 $15\%$，外邊界死區 $95\%$（防止硬體磨損導致推到底無法達到滿值）。
* **響應曲線 (Response Curve)**：
  * 採用微凸的冪次曲線（如 $y = x^{1.2}$），在微小推力時提供高精度的走位微調，大幅推力時快速切換至極速奔跑。

### 3.3 鍵盤輸入 (Keyboard WASD / Arrow Keys UX)
* **輸入平滑 (Digital-to-Analog Emulation)**：
  * 鍵盤按下為瞬時數值 $1.0$。若直接送入物理層，角色起步可能顯得唐突。
  * UX 層提供可微調的上升時間（Rise Time $\approx 0.05\text{s}$）與釋放時間（Fall Time $\approx 0.03\text{s}$），營造身體肌肉加速感。
* **斜向歸一化 (Diagonal Normalization)**：
  * 同時按下 W 與 D 時，向量為 $(1, 1)$，未經處理長度為 $\sqrt{2} \approx 1.414$。
  * 必須執行歸一化（Normalize）夾緊至最大模長 $1.0$，防止玩家利用斜走獲得額外移動速度優勢。

---

## 4. 座標系投影 UX (Coordinate Mapping)

在 2.5D、俯視角（Top-Down）或 3D 動作遊戲中，方向鍵的意義取決於視角關係：

```text
玩家輸入 (螢幕平面 Vector2: x=水平, y=垂直)
        │
        ▼ 依據當前遊戲視角模式進行轉換
┌────────────────────────┬────────────────────────┐
│ 模式 A: 攝影機相對模式 │ 模式 B: 世界平面對齊模 │
│ (Camera-Relative)      │ (World-Aligned / 2D)   │
└────────────────────────┴────────────────────────┘
        │                                │
        ▼                                ▼
投影至攝影機 Forward/Right 平面       直接映射至世界 X/Y 或 X/Z
角色「向上」永遠等於「往畫面深處跑」   角色「向上」等於世界座標 +Y / +Z
```

* **推薦規則**：
  * 若視角為固定斜 45 度（Isometric），方向鍵應支援「**軸向旋轉 $45^\circ$ 補償**」，讓按「上」時直觀朝畫面左上或右上斜切前進，符合螢幕透視直覺。
  * 在 CombatAesthetic 2D 橫向捲軸模式下，垂直方向（W/S 或上/下）解讀為跳躍、下蹲、上下爬梯或目標瞄準偏移，水平方向專注左右橫移。

---

## 5. 戰鬥狀態對方向輸入的約束與鎖定 (Combat State Interaction)

在 ARPG 與動作對戰中，方向輸入絕不能隨意無條件執行：

1. **攻擊前搖與轉向鎖定 (Startup Turn Lock)**：
   * 在揮刀/施法的起手階段（Startup Frames），角色轉向角速度被嚴格限制（如限制轉向率 $\le 90^\circ/\text{s}$ 或完全鎖定），避免動作抽搐變形。
2. **後搖取消窗口 (Recovery Cancel Window)**：
   * 在招式硬直即將結束的「取消窗口」，輸入方向鍵可觸發「以移動取消後搖」或「閃避翻滾取消」，提升操作絲滑感。
3. **受擊硬直 (Hitstun Lockout)**：
   * 角色處於受擊狀態時，完全屏蔽移動指令，僅保留受擊擊退物理動能。
4. **雙擊衝刺判定 (Double-Tap Dash Cadence)**：
   * 在短時間內（例如 $200\text{ms}$ 內）連續兩次敲擊同方向，將 `MoveIntent.IsDashing` 設為 true，觸發墊步衝刺。

---

## 6. 介面與資料契約草案 (API & Contract Draft)

```csharp
namespace Yu5h1Lib.Combat.Input
{
    using UnityEngine;

    /// <summary>
    /// 語意化移動操作意圖 (從 UX 層輸出至 Gameplay 層)
    /// </summary>
    public struct MoveIntent
    {
        public Vector2 Direction;    // 歸一化方向向量 (長度 0~1)
        public float Magnitude;      // 推力強度 (0=靜止, 0.1~0.6=慢走, 0.7~1.0=奔跑)
        public bool IsDashing;       // 是否觸發衝刺/快跑手勢
        public bool IsCameraRelative;// 是否需要相機空間投影
    }

    /// <summary>
    /// 方向輸入源介面 (可由鍵盤、手把、動態搖桿或虛擬十字鍵實作)
    /// </summary>
    public interface IDirectionalInputSource
    {
        MoveIntent PollMoveIntent();
        void SetActive(bool active);
    }
}
```

---

## 7. 驗收標準 (Acceptance Criteria)

1. **多設備等價性**：鍵盤、手把與觸控虛擬搖桿推至最大時，角色達到的終端速度一致。
2. **死區與防抖**：手把微小靜置偏角不導致角色微走；手機手指停留時角色不頻繁轉向。
3. **斜向均勻性**：斜向走位速度不得大於正向速度（向量長度 $\le 1.0$）。
4. **戰鬥鎖定生效**：角色在重攻擊動作與受擊硬直中，輸入方向不會發生瞬間瞬轉或反常位移。
