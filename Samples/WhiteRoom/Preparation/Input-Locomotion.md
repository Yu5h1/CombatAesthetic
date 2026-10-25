# WhiteRoom：輸入與移動重用盤點與 3D 適配分析

> 執行項目：`white-room-stage0` › `input-locomotion-inventory`  
> 基準日期：2026-10-25  
> 盤點方式：靜態原始碼、介面契約與 UX 設計規格唯讀盤點（未實作、未修改程式、未啟動 Unity）

---

## 1. 盤點目標與對照需求

依 [WhiteRoom 實作計畫](../Implementation-Plan.md#第一個交付的目標) 與 [既有能力對照](../Existing-Capabilities.md)，第一個 3D PvE 可操作交付在「移動與跳躍」上的可觀察要求為：
1. **WASD 以鏡頭畫面為基準進行八方向移動**：角色隨輸入向量平滑轉向輸入方向並前進。
2. **跳躍與落地 (Space)**：能按下空白鍵起跳，具備明確起跳、滯空與著地反應。
3. **原地攻擊與方向攻擊**：未輸入方向時能原地出招；輸入方向時能指定出手方向（朝側方或近處目標出招）。
4. **受力因果與動量保留**：動作切換不能任意清除外力或原有擊飛速度，動畫速度與物理外力需合理解算。

本文件針對 `InputHandler`、輸入後端、`ILocomotor`、`NPCLocomotor`、鏡頭幾何及既有 2D 控制器進行唯讀盤點，梳理來源、可用部分、3D 限制與架構缺口。

---

## 2. 輸入採集層 (Input Collection Layer)

### 2.1 既有組件與原始碼來源

* **`InputHandler`**：[`Unity/UnityExtension/Packages/common/Runtime/Input/InputHandler.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/common/Runtime/Input/InputHandler.cs)
  * 定義抽象介面 `InputHandler.IBackend`，靜態封裝按鍵、滑鼠按鈕、滾輪、軸（Axis）與觸控（Touch）。
  * 支援 `GetKeyDown`、`GetKey`、`GetKeyUp`、`GetMouseButtonDown`、`GetAxis`、`GetButtonDown`。
* **`InputSystemBackend`**：[`Unity/UnityExtension/Packages/InputSystem/Runtime/Backend/InputSystemBackend.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/InputSystem/Runtime/Backend/InputSystemBackend.cs)
  * 實作 `IBackend`，透過 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 自動註冊為 `CurrentBackend`。
  * 內部對接 Unity New Input System (`UnityEngine.InputSystem`) 的 `Keyboard.current`、`Mouse.current`、`Touchscreen.current` 與 `InputSystem.actions`。

### 2.2 現狀、可用部分與缺口

| 項目 | 現有能力與可用部分 | 3D / WhiteRoom 限制與缺口 |
|---|---|---|
| **鍵鼠信號採集** | `InputHandler` 已統一鍵盤、滑鼠與 Axis 查詢，可直接在 3D 專案中取得 WASD（`Horizontal`/`Vertical`）與 Space（`Jump`）。 | **套件耦合脫節**：CombatAesthetic 現有的 [`PlayerHost.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/Combat-Aesthetics/Runtime/Input/PlayerHost.cs#L19) 未使用 `InputHandler`，而是直接依賴 `GameManager.InputModule.input`（`BaseInput`），導致無 `GameManager` 即無法採集輸入。 |
| **右鍵防禦信號** | `InputHandler.GetMouseButtonDown(1)` 可直接偵測滑鼠右鍵按下／放開。 | 既有 `PlayerHost` 將右鍵定義為切換技能（`ShiftIndexOfAction`），需要依 WhiteRoom UX 改為警戒與防禦操作。 |
| **視角座標投影** | 無。`InputHandler` 僅輸出 Raw Axis（$[-1, 1]$），不負責視角轉換。 | 缺乏將螢幕 2D 軸向量投影至 3D 攝影機水平視角平面的計算模組。 |

---

## 3. 移動抽象與執行層 (Locomotion Layer)

### 3.1 既有組件與原始碼來源

* **`ILocomotor` 契約**：[`Unity/UnityExtension/Packages/Animation/Runtime/Core/ILocomotor.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/Animation/Runtime/Core/ILocomotor.cs)
  ```csharp
  public interface ILocomotor
  {
      Vector3 Velocity { get; }
      void Drive(Vector3 velocity);
  }
  ```
  * 設計約定：`Drive(velocity)` 為世界座標期望速度（方向為前進方向，長度為目標速率 m/s，零為停止）；`Velocity` 為當前幀實際位移速度，用於 Animator Blend Tree、足部 IK 與音效。
* **`NPCLocomotor` 元件**：[`Unity/UnityExtension/Packages/Animation/Runtime/Component/NPCLocomotor.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/Animation/Runtime/Component/NPCLocomotor.cs)
  * 實作 `ILocomotor`。每幀邏輯：
    1. 將指令投影至水平面（`horiz.y = 0f; float speed = horiz.magnitude;`）。
    2. 朝目標方向旋轉（`Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * dt)`）。
    3. 沿水平朝向平移（`transform.position += fwd * (speed * dt)`）。
    4. 計算幀速度並將速率寫入 Animator 參數（預設 `"Speed"`）。
    5. `Awake` 時強制關閉根骨骼位移（`animator.applyRootMotion = false`）。

### 3.2 `NPCLocomotor` 的可用性與 3D 戰鬥限制

* **可用部分**：
  * 「`Drive(Vector3)` 接收目標速度向量」的職責分離設計非常優秀，完美隔絕玩家、AI 與網路指令來源。
  * 「水平轉向速率限幅（`Quaternion.RotateTowards`）」滿足 WhiteRoom「角色隨輸入方向轉向前進」的需求。
* **3D 戰鬥限制與缺口**：
  1. **無碰撞與實體物理**：採用純 `transform.position` 平移，無法處理 3D Collider 障礙物碰撞、邊界阻擋或斜坡貼地。
  2. **完全忽略垂直維度**：程式碼寫死 `horiz.y = 0f`，完全不支援重力（Gravity）、跳躍衝量（Jump Impulses）、下落與落地判定。
  3. **不支援外力與動量累加**：未提供 `AddForce`、擊退（Knockback）或慣性衰減，無法滿足受擊位移與受力因果。
  4. **強制關閉 Root Motion**：在 Awake 寫死 `applyRootMotion = false`，若戰鬥攻擊動作帶有 Root Motion 將完全失效。

---

## 4. 鏡頭與移動接合盤點 (Camera & Movement Coupling)

### 4.1 核心庫 3D 鏡頭幾何能力

* **來源**：[`Unity/Core/Runtime/Source/Control/CameraController.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/Core/Runtime/Source/Control/CameraController.cs#L14)
* **能力**：`CameraController.LookAt3rdPerson`
  ```csharp
  public void LookAt3rdPerson(Vector2 delta, Transform viewer, Vector3 pivot, ref Vector2 angle, MinMax AngleRangeX, MinMax AngleRangeY, float distance)
  ```
  * 接收滑鼠/軸輸入偏移 `delta`，更新 Pitch/Yaw 角度並限制於 `AngleRange`。
  * 計算繞 pivot 的球形旋轉座標：`viewer.position = pivot + Quaternion.Euler(angle.y, angle.x, 0) * new Vector3(0, 0, -distance);`
  * 呼叫 `camera.transform.LookAt(pivot)` 維持視線中心。

### 4.2 既有 CombatAesthetic 鏡頭與命名衝突

* **現狀**：CombatAesthetic 內亦有一個 [`Runtime/Core/CameraController.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/Combat-Aesthetics/Runtime/Core/CameraController.cs)，但它是**2D 正交攝影機跟隨器**：
  * 繼承 `SingletonBehaviour<CameraController>`。
  * 處理 2D 排序圖層（`SortingLayerSprites`）與正交黑邊（`FitfadeBoardWithOrthographic`）。
  * 缺乏 3D 自由旋轉軌道與第三人稱環繞能力。
* **命名衝突**：CombatAesthetic 內全域類別 `CameraController` 遮蔽了 `Yu5h1Lib.Runtime.CameraController`，若未指定命名空間將造成混淆。

### 4.3 鏡頭基準八方向向量映射公式（待實作接合點）

在 3D 第三人稱視角下，將玩家鍵盤 WASD（輸入向量 $(x, z)$）轉換為世界空間前進向量的標準幾何轉換公式為：

$$\vec{F}_{\text{flat}} = \frac{\vec{F}_{\text{cam}} - (\vec{F}_{\text{cam}} \cdot \vec{U}) \vec{U}}{\|\vec{F}_{\text{cam}} - (\vec{F}_{\text{cam}} \cdot \vec{U}) \vec{U}\|}, \quad \vec{R}_{\text{flat}} = \frac{\vec{R}_{\text{cam}} - (\vec{R}_{\text{cam}} \cdot \vec{U}) \vec{U}}{\|\vec{R}_{\text{cam}} - (\vec{R}_{\text{cam}} \cdot \vec{U}) \vec{U}\|}$$

$$\vec{V}_{\text{world}} = \text{Normalize}(x \cdot \vec{R}_{\text{flat}} + z \cdot \vec{F}_{\text{flat}}) \times \text{Speed}$$

目前 CombatAesthetic 內無任何模組提供此投影計算，需在 WhiteRoom 移動控制器或轉接層中補齊。

---

## 5. 既有 2D 角色控制器對照 (`CharacterController2D`)

### 5.1 既有設計中值得沿用的戰鬥概念

1. **操作可控性限制 (`Controllable`)**：
   * 角色是否能接受移動/跳躍輸入，由狀態機動作區間與意識狀態（`Conscious > 10`）共同決定。硬直、倒地或被擊飛時自然屏蔽輸入。
2. **動畫速度與物理外力疊加 (`AnimatorCharacterController2D.ProcessMovement`)**：
   * 透過 `rootMotionWeight` 與 `VelocityWeight` 混合動畫根骨骼速度與外力速度。
   * 空中與地面具備不同的控制靈敏度倍率（`AirborneMultiplier`）。
3. **衝量注入 (`AddForce`)**：
   * 受到外力時，衝量直接累加至當前速度，若衝量向上則主動脫離地面判定（`detector.LeaveGround()`），避免外力被地面吸附抵消。

### 5.2 必須揚棄的 2D 專屬實作

* **物理核心**：綁定 `Rigidbody2D`、`Physics2D` 與 `Collider2D`。
* **二元面向切換**：2D 轉向僅靠 `forwardSign > 0 ? 0 : 180` 翻轉 X 軸或 Y 軸 Euler 角度；3D 必須改用連續角度的四元數旋轉（Quaternion Yaw）。
* **地形與斜坡**：`CheckSlop` 與 2D 射線檢測，無法適應 3D NavMesh 或 3D Mesh Collider。

---

## 6. 盤點總結與 WhiteRoom 接用架構矩陣

| WhiteRoom 需求 | 評估既有來源 | 採用建議 | 缺口與待實作項目 |
|---|---|---|---|
| **鍵鼠/手把輸入** | `Yu5h1Lib.InputHandler` + `InputSystemBackend` | **直接重用** | 建立 `PlayerInputSource` 將 `InputHandler` 轉接為移動/跳躍命令，不透過舊 `PlayerHost`/`GameManager`。 |
| **移動控制契約** | `Yu5h1Lib.ILocomotor` | **直接沿用契約** | `ILocomotor` 契約完全通用，直接作為 WhiteRoom 角色移動的統一入口。 |
| **3D 移動執行元件** | `Yu5h1Lib.NPCLocomotor` | **參考轉向邏輯，擴充/重構 3D 戰鬥版本** | `NPCLocomotor` 缺少 3D 碰撞、跳躍、重力與外力。需以 Unity `CharacterController` 實作具備重力、跳躍與衝量快取的 `WhiteRoomLocomotor : ILocomotor`。 |
| **第三人稱視角鏡頭** | `Yu5h1Lib.Runtime.CameraController.LookAt3rdPerson` | **直接重用運算法則** | 建立獨立於 2D `CameraController` 的 3D 軌道跟隨元件，提供相機水平前向向供輸入投影。 |
| **鏡頭相對 8 方向** | [Directional-Input-UX-Design.md](../../Documentation/plans/Directional-Input-UX-Design.md) | **落實規範算法** | 依視角平面公式將 WASD 投影為水平世界方向，傳入 `ILocomotor.Drive(V)`。 |
| **跳躍與落地** | `CharacterController2D.TriggerJump` 設計思維 | **移植行為邏輯** | 在 3D 移動元件中維護垂直速度 $V_y$、重力加速度 $g$ 與接地狀態，跳躍給予初速，著地觸發事件。 |
| **外力保留與擊退** | `CharacterController2D.AddForce` 設計思維 | **移植行為邏輯** | 實作外力衰減（Decay）緩衝區，外力未完全抵消前切換攻擊動作不應強制重設速度。 |
