# WhiteRoom：執行期動作系統盤點與解耦分析

> 執行項目：`white-room-stage0` › `runtime-motion-inventory`  
> 基準日期：2026-10-25  
> 盤點方式：靜態原始碼、介面契約與 UX 設計規格唯讀盤點（未實作、未修改程式、未啟動 Unity）

---

## 1. 盤點目標與對照需求

依 [WhiteRoom 實作計畫](../Implementation-Plan.md#第一個交付的目標) 與 [既有能力對照](../Existing-Capabilities.md)，WhiteRoom 在動作製作與驗證管線上要求：
1. **動作重用與動態調整**：支援在執行期預覽、微調人形骨架姿勢與時間點，提供 Live Preview 與重播能力。
2. **足部貼地與身體動態平衡**：角色在起伏地形或移動過程中，足部與髖部能依地形自然貼合與穩定重心。
3. **戰鬥控制窗與狀態銜接**：具備明確的可取消窗（Cancel Window）、輸入緩衝窗（Buffer Window）與攻擊命中判定窗。
4. **受力因果與動量延續**：動作切換、受擊或中斷時，原有速度與受力衝量不得突兀被歸零，需具備物理動量保留機制。

本文件針對 `AnimationToolWindow`、`GroundIKTarget`、`BalanceHipController`、相關 StateMachineBehaviour 及物理運動銜接進行唯讀盤點，區分 Runtime API 與 Editor 依賴，並分析執行期動作即時調整與動量延續可行性。

---

## 2. 執行期與編輯器依賴分離 (Runtime vs Editor Separation)

### 2.1 `AnimationToolWindow` 架構分析

* **原始碼來源**：[`Unity/UnityExtension/Packages/Animation/Editor/AnimationToolWindow.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/Animation/Editor/AnimationToolWindow.cs)
* **核心機制**：該工具提供動畫骨架姿勢取樣、筋肉數值（Muscles）滑桿調整、關鍵影格烘焙與儲存功能。

### 2.2 原生 Runtime 能力：`HumanPoseHandler` 與 `HumanPose`

經過靜態原始碼與 Unity API 契約分析，`AnimationToolWindow` 的核心姿態操作使用的是 Unity 原生**執行期** API：
* **`UnityEngine.HumanPoseHandler`**：
  * 命名空間為 `UnityEngine`，屬於執行期引擎核心（`UnityEngine.CoreModule` / `UnityEngine.AnimationModule`），**完全可在 PlayMode 或 Standalone Runtime 執行**。
  * 建構子：`new HumanPoseHandler(Avatar avatar, Transform root)`。只要提供具備 Humanoid Avatar 的 Animator 與 Root Transform 即可在執行期實例化。
  * 核心方法：
    * `GetHumanPose(ref HumanPose humanPose)`：讀取當前骨架姿態。
    * `SetHumanPose(ref HumanPose humanPose)`：將數值寫入骨架姿態。
* **`UnityEngine.HumanPose`**：
  * 包含 `Vector3 bodyPosition`、`Quaternion bodyRotation` 以及 `float[] muscles`（95 條人形筋肉的標準化角度數值 $[-1.0, 1.0]$）。
  * 筋肉結構天生具備跨模型 Retargeting 能力，無需關心個別骨骼 Transform 的本機座標軸方向與層級結構差異。

### 2.3 嚴格 Editor 依賴項

`AnimationToolWindow` 中無法在執行期使用的部分主要集中在**資源序列化**與**Unity 編輯器介面**：
1. **`UnityEditor.AnimationUtility`**：
   * 用於取樣現有 `.anim` Clip（`AnimationUtility.SampleAnimationClip`）、讀取或設置動畫曲線（`AnimationUtility.SetEditorCurve`）。
   * `EditorCurveBinding` 亦屬於 `UnityEditor` 命名空間。
2. **`UnityEditor.AssetDatabase`**：
   * 用於將調整後的數據儲存回專案資源資產檔（`AssetDatabase.CreateAsset`、`AssetDatabase.SaveAssets`）。
3. **`UnityEditor.EditorWindow` & `GUILayout`**：
   * 基於 Unity Editor IMGUI 的視窗繪製與事件循環。

### 2.4 執行期解耦與 Live Preview 路徑

| 能力向度 | Editor 現有做法 | WhiteRoom 執行期解耦架構建議 |
|---|---|---|
| **姿態採集與微調** | `HumanPoseHandler.GetHumanPose` / `SetHumanPose` | **直接重用**：在 Runtime 建立 `RuntimePoseController`，操作 `HumanPoseHandler` 讀寫 `HumanPose.muscles`。 |
| **動態預覽 (Live Preview)** | 依賴 Editor 影格推進與 Update | **直接重用**：於 `MonoBehaviour.Update` 或 `LateUpdate` 中調用 `SetHumanPose` 即時預覽變更。 |
| **曲線取樣與撥放** | `AnimationUtility.SampleAnimationClip` | **PlayableGraph API**：執行期使用 `UnityEngine.Playables`（如 `AnimationClipPlayable` 或 `AnimationPosePlayable`）進行時間軸精確推進與採樣。 |
| **數據存檔與交換** | 寫入 `.anim` 檔案 | **自訂輕量格式**：以 JSON / ScriptableObject 記錄 Keyframe 姿態（時間點 + muscles 陣列），避免在 Runtime 強行生成二進位 `.anim` 資產。 |

---

## 3. 姿態與足部平衡系統 (Posture & IK)

### 3.1 `GroundIKTarget`：足部地形貼合

* **原始碼來源**：[`Unity/UnityExtension/Packages/Animation/Runtime/Component/GroundIKTarget.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/Animation/Runtime/Component/GroundIKTarget.cs)
* **執行期特性**：
  * 純 Runtime 組件，無任何 Editor 命名空間引用。
  * 每幀自足部目標點（Left Foot / Right Foot）向下發射射線（`Physics.Raycast`），檢測地面碰撞點與法線（Normal）。
  * 計算足部相對於角色 Root 的高度偏移量（`groundOffset`）以及隨地形傾斜的角度旋轉補償（`Quaternion.FromToRotation(Vector3.up, hit.normal)`）。
* **與 WhiteRoom 結合價值**：
  * 提供 3D 地形行走、階梯與斜坡上的足部自然接觸，解決 2D 控制器過渡到 3D 空間時最顯著的「雙腳懸空」或「穿透斜坡」視覺瑕疵。

### 3.2 `BalanceHipController`：重心與骨盆動態平衡

* **原始碼來源**：[`Unity/UnityExtension/Packages/Animation/Runtime/Component/BalanceHipController.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/Animation/Runtime/Component/BalanceHipController.cs)
* **執行期特性**：
  * 純 Runtime 組件。
  * 根據雙足接地點的高度差與目標位置，動態調降或傾斜骨盆（Hips Bone Transform 或 `HumanPose.bodyPosition`）。
  * 避免雙腳因為 IK 拉伸極限而產生抽搐（IK Pop）或膝蓋過度挺直，維持自然的人體下蹲重心平衡。

### 3.3 其他相關輔助解算器

* **`AimSolver`**（頭部／胸腔視線注視）：支援在執行期將角色視線或武器朝向動態對準目標。
* **`IKController`**：統一在 `OnAnimatorIK(int layerIndex)` 中協調足部 IK 權重、手部握持點與全身解算。

---

## 4. 狀態協調與控制窗機制 (State Coordination & Controllable Windows)

### 4.1 核心狀態機行為組件 (StateMachineBehaviours)

* **`CharacterSMB`**：[`Unity/Combat-Aesthetics/Runtime/Animation/CharacterSMB.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/Combat-Aesthetics/Runtime/Animation/CharacterSMB.cs)
  * 掛載於 Animator Controller 的各動作 State 上。
  * 職責：
    1. **時間窗控制**：定義動作前搖（Startup）、命中判定窗（Active Frames）、後搖（Recovery）與可取消窗（Cancel Window）。
    2. **控制權標記**：透過布林或正規化時間區間（Normalized Time $[start, end]$）動態開放 `isControllable`、`canCombo`。
    3. **速度權重調製**：提供 `VelocityWeight` 或 `rootMotionWeight` 參數曲線，調節動作播放期間位移速度的衰減與放大。
* **`ReactionSMB`**：[`Unity/Combat-Aesthetics/Runtime/Animation/ReactionSMB.cs`](file:///c:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/Combat-Aesthetics/Runtime/Animation/ReactionSMB.cs)
  * 控制受擊（Hit Reaction）、受創僵直（Flinch / Stun）與硬直恢復狀態。
  * 在受擊期間關閉常規輸入響應，僅允許特定受身（Tech / Break Recovery）或完全等待硬直結束。

### 4.2 控制窗在 WhiteRoom 3D PvE 戰鬥的關鍵作用

1. **連段輸入緩衝 (Input Buffer)**：
   * 玩家在攻擊動作後搖開始前按下輕擊／重擊，`CharacterSMB` 記錄連段意圖並在 `canCombo` 視窗開啟的第一幀順暢進入下一段攻擊。
2. **防禦取消與移動取消 (Guard / Dash Cancel)**：
   * 在特定技能命中後或特定幀數之後，玩家按下右鍵（防禦）或 Space（閃避/跳躍）能提早中斷後搖，提升操作靈敏度與回饋感。

---

## 5. 物理因果與動量延續分析 (Physical Causality & Momentum Preservation)

### 5.1 現狀衝突：動畫位移 vs 物理衝量

在目前的程式碼架構中，存在以下阻礙「受力因果與動量保留」的衝突：
1. **速度覆寫問題**：
   * `NPCLocomotor` 每一幀直接透過 `transform.position += fwd * (speed * dt)` 強制平移，完全沒有外力或加速度暫存器。
   * 若角色在受到大推力擊退（Knockback）時切換為常規移動，擊退速度會瞬間被輸入速度完全抹除。
2. **Root Motion 突兀截斷**：
   * 帶有突進（Lunge）的攻擊動作若依靠 Root Motion 或特定前衝速度推進，一旦被敵方打斷或玩家強制取消，速度若直接歸零會產生反物理的「撞牆急煞感」。

### 5.2 跨狀態動量延續方案 (Momentum Preservation Pipeline)

為滿足「受力因果不可斷裂」的戰鬥美學要求，WhiteRoom 在後續移動與戰鬥狀態解算中應遵守以下原則：
1. **分離意圖速度與物理慣性速度**：
   $$v_{total} = v_{drive} + v_{impulse} + v_{residual}$$
   * $v_{drive}$：由玩家輸入或 AI 意圖驅動之速度（受 `CharacterSMB.VelocityWeight` 調節）。
   * $v_{impulse}$：由爆炸、受擊或外力衝擊產生之瞬時向量，依據阻力係數平滑指數衰減（Exponential Decay）。
   * $v_{residual}$：動作被中斷或取消時，將該瞬間的動畫切線速度（Residual Tangent Velocity）保留為慣性位移，逐漸過渡為零或轉移至物理剛體。
2. **嚴格禁止狀態切換時直接歸零速度**：
   * 任何 State 進入或退出回呼，不可直接執行 `velocity = Vector3.zero`，除非該狀態明確代表「被定身」或「撞擊靜止障礙物」。

---

## 6. 套件邊界與獨立性評估 (Package Boundaries)

* **`com.yu5h1.animation` 與 CombatAesthetic 的關係**：
  * `GroundIKTarget` 與 `BalanceHipController` 屬於 `com.yu5h1.animation` 套件。
  * 如 [宿主依賴盤點](Host-Dependencies.md) 所指出，CombatAesthetic 的 `package.json` 漏列了對 `com.yu5h1.animation` 的依賴，但程式碼已在 asmdef 中引用其 GUID。
* **WhiteRoom 的引用建議**：
  * WhiteRoom 作為 CombatAesthetic 的 Samples 範例，應維持單向依賴：
    * WhiteRoom $\rightarrow$ CombatAesthetic $\rightarrow$ `com.yu5h1.animation` & `com.yu5h1.core`。
  * 避免將任何 Editor 專用工具邏輯（如 `AnimationToolWindow` 的資產寫入）移入 Runtime，確保 Standalone Player 建置乾淨。

---

## 7. 結論與 Stage 1 實作準備清單

1. **`HumanPoseHandler` 經確認為完全合法的 Runtime API**，WhiteRoom 執行期姿勢調整可以直接以其為核心架構，無需依賴任何 Editor 套件。
2. **`GroundIKTarget` 與 `BalanceHipController` 具備立即可用性**，能無縫掛載於 3D 角色解決足部貼地與重心自然下沉。
3. **動量延續是 3D 控制器必須補齊的核心邏輯**，需要在 Stage 1 實作一個將動畫切線速度與物理外力疊加的控制器，而非沿用純平移的 `NPCLocomotor`。
