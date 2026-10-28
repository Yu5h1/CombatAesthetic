# CombatAesthetic UPM 拆包討論計畫

本文記錄 Combat-Aesthetics 的演進方向。已確認先在同一 repo 隔離新戰鬥基礎，由 WhiteRoom 驗證。後文的套件名稱與分層仍是候選，不代表必須立即建立多個 package。

## 已確認的演進與遷移方向

```text
Combat-Aesthetics repo
├─ 既有 2D 元件與相容入口 ← 既有使用端
└─ 新戰鬥基礎 → 經驗證的 Yu5h1Lib 共用能力
   └─ WhiteRoom：驗證最小 3D 戰鬥與連線案例
```

本工作範圍是新戰鬥基礎與 WhiteRoom。先界定新舊依賴與狀態責任，再發展最小連線案例；新戰鬥基礎不依賴舊 GameManager 或 2D 角色控制器。保留仍被使用的舊入口與資產識別，避免開發新基礎時破壞既有使用端。

KFC 的逐步遷移仍是必要工作，但由使用者在另一個 session 處理。本計畫不制定其遷移批次、驗收條件或玩法變更，也不以 KFC 遷移完成作為 WhiteRoom 的前置條件。原先在此展開的遷移流程已因工作範圍更正而移除。

## 目前判斷

CombatAesthetic 現在比較像一個 2D 動作遊戲原型框架，而不是單純的 Combat package。裡面混有角色控制、2D 偵測/碰撞、Animator StateMachineBehaviour、技能資料、FX cast、UI stat、場景/音訊/存檔、AI patrol、Input host、Editor inspector 與工具視窗。

因此不建議一開始直接按現有資料夾拆成 package。現有資料夾是功能聚落，但不是乾淨依賴邊界。比較穩的方式是先建立依賴方向，逐步把低層抽出去，最後讓 CombatAesthetic 變成整合包或 sample/gameplay package。

## 建議 Package 分層

### 1. 已有基底 package

先承認這些已經存在，CombatAesthetic 拆出去時應盡量依賴它們，而不是複製：

| Package | 用途 | CombatAesthetic 對應內容 |
| --- | --- | --- |
| `com.yu5h1.library` / Core | Yu5h1Lib 基礎型別、BaseMonoBehaviour、Singleton、Extension、Adapter | 只依賴，不搬入 Combat |
| `com.yu5h1.common` | 通用 Unity component/data/adapter | Pool、事件、簡單資料、通用 Mono 工具可評估往這裡收 |
| `com.yu5h1.2d` | 2D 基礎工具 | Collider2D、Physics2D、Line2D、Sprite/PSD 工具應優先往這裡 |
| `com.yu5h1.inputsystem` | Input System bridge | 新 input host 不應留在 Combat Core |
| `com.yu5h1.ui` | UI controls / MVVM adapter | `UI_Stat*`、Dialog、Prompt、Menu 可往這裡或獨立 gameplay UI |
| `com.yu5h1.animation` | Animator/Timeline/IK utilities | Animator enhancer、SMB 基礎、PlayableDirector、IK |
| `com.yu5h1.ai` | Route、Patrol、Autopilot 基礎 | AI 不應直接反向依賴 Combat controller |

### 2. 新增建議 package

| Package | 責任 | 依賴 |
| --- | --- | --- |
| `com.yu5h1.gameplay` 或 `com.yu5h1.gameplay.core` | 遊戲通用資料：AttributeStat、EnergyInfo、AttributeType、DefeatedReason、IGameManager-like contract | Core/common |
| `com.yu5h1.gameplay.2d` | 2D gameplay 基礎：ITransform2D、Rigidbody2DBehaviour-like movement base、2D detector abstraction | Core/common/2D |
| `com.yu5h1.combat.core` | 不依賴 Unity 表現層的戰鬥模型：命令、狀態、hit/hurt intent、attribute calculation、snapshot contract | Core 或純 C# |
| `com.yu5h1.combat.2d` | Unity 2D 戰鬥整合：CharacterController2D、ColliderDetector2D、HurtBox、Scanner、Caster bridge | gameplay.core/gameplay.2d/combat.core/2D |
| `com.yu5h1.combat.animation` | Animator controller bridge、CharacterSMB、ActionSMB、AnimParamSMB、AnimatorCharacterController2D | combat.2d/animation |
| `com.yu5h1.combat.fx` | Cast、Fx_Sender/Receiver、sprite renderer receiver、particle tracer | combat.2d/common/pool |
| `com.yu5h1.combat.ai` | HostData2D 的 AI 實作、Autopilot、Patrol combat adapter | combat.2d/AI |
| `com.yu5h1.combat.input` | PlayerHost 與 Unity Input/InputSystem adapter | combat.2d/inputsystem |
| `com.yu5h1.combat.ui` | Attribute UI、stat bar/counter/animation、combat prompt | combat.2d/UI |
| `com.yu5h1.network` | Transport/channel abstraction、local loopback、lag simulator、future rollback sync | combat.core 或獨立純 C# |

以下四個邊界保留作候選分析；實際邊界由 WhiteRoom 的需求與既有能力盤點決定，不直接照表建立 package：

1. `gameplay.core`
2. `gameplay.2d`
3. `combat.2d`
4. `combat.animation`

`combat.fx`、`combat.ai`、`combat.input`、`combat.ui` 可以先留在整合包內，等前四個穩了再拆。

## 目前代碼耦合中心

### CharacterController2D

位置：`Runtime/Character/CharacterController2D.cs`

目前同時負責：

- 2D movement / gravity / slope / jump
- Attribute health damage
- HostData2D input/AI command
- ColliderDetector2D ground detection
- Scanner2D / Caster / Teleportable component wiring
- PlayerPrefs gameplay tuning
- invincibility visual flicker
- GameManager paused state

拆分方向：

- movement base 放 `gameplay.2d`
- attribute interaction 改成 interface，例如 `IAttributeTarget`
- host input 改成 command source，例如 `ICharacterCommandSource`
- scanner/caster/teleportable 不要由 base controller 強綁，可改成 optional adapter 或 feature component
- PlayerPrefs tuning 改成 setting asset，不放 controller static property

### AnimatorCharacterController2D

位置：`Runtime/Character/AnimatorCharacterController2D.cs`

目前把動畫、技能、音效、FX、UI、GameManager cursor 都串在一起。它比較適合變成 `combat.animation` 的高階整合 component，而不是 combat core。

拆分方向：

- Animator root motion / SMB bridge 留在 `combat.animation`
- SkillData/SkillBehaviour 先留在 combat runtime，之後抽成 `combat.core` + Unity authoring layer
- `AudioManager.Play`、`GameManager.instance.cursors`、`attribute.ui.Hide` 這類直接呼叫應改成 event/hook
- `CastHitBox`、`CastFxOnTransform` 應移到 FX/Combat presentation service

### AttributeBehaviour 與 UI

位置：

- `Runtime/Core/GameDataStructure.cs`
- `Runtime/Character/AttributeBehaviour.cs`
- `Runtime/UI/UI_Stat*.cs`

目前 attribute model、MonoBehaviour lifecycle、UI refresh 綁在一起。這會阻礙多人與 rollback，因為 server/core 不應知道 Unity UI。

拆分方向：

- `AttributeStat`、`EnergyInfo`、enum 放 `gameplay.core`
- `AttributeBehaviour` 放 Unity gameplay adapter
- UI 改訂閱 attribute changed event，不由 AttributeBehaviour 持有 `UI_Attribute`

### HostData2D / PlayerHost / Autopilot

位置：

- `Runtime/Character/HostData2D.cs`
- `Runtime/Input/PlayerHost.cs`
- `Runtime/AI/Autopilot.cs`

這組其實是「控制來源抽象」，是好的拆分入口。但現在 `HostData2D.HostBehaviour2D` 直接拿 `CharacterController2D Body`，導致 AI/Input 都依賴具體 controller。

拆分方向：

- 保留 `HostData2D` 概念，但改名或抽 interface：`ICharacterCommandSource`
- movement/action/skill switch 形成 command DTO
- PlayerHost 放 input package adapter
- Autopilot 放 AI adapter
- Combat controller 只吃 command，不知道命令來自玩家、AI、網路或 replay

### 2D Physics / Event

位置：

- `Runtime/Physics/*`
- `Runtime/Event/*`

`BaseColliderDetector2D`、`ColliderDetector2D`、`Scanner2D`、`TriggerEvent2D`、`CollisionEvent2D` 是 2D framework 材料，不一定屬於 Combat。`EventMask2D` 目前依賴 `GameManager.IsQuit`、`SceneController.IsUnloading`，這讓它難以放進純 2D package。

拆分方向：

- 低層 collider cast、line cast、bounds/physics extension 放 `2D`
- gameplay-specific event mask 留在 gameplay/combat
- 移除 2D event 對 GameManager/SceneController 的直接依賴，改用 lifecycle service 或 optional guard

### Editor

位置：`Editor/*`

Editor 目前一包 inspector 全部引用 runtime 類別。拆包後 Editor 應跟 runtime package 對齊：

- 2D inspector/editor extension 跟 `com.yu5h1.2d`
- combat controller inspector 跟 `combat.2d`
- animation/SMB inspector 跟 `combat.animation`
- UI inspector 跟 `combat.ui`
- game cache/records window 先留在 integration package，因為它比較像專案工具，不是框架 core

## 建議拆分順序

### Phase 0：只整理依賴圖

- 將 asmdef GUID reference 換成可讀名稱前，先建立 dependency map。
- 標記每個 runtime 類別的 package 候選。
- 找出所有跨資料夾引用，尤其是 `GameManager`、`AudioManager`、`AttributeBehaviour`、`CharacterController2D`、`AnimatorCharacterController2D`。
- 不搬檔，不改 namespace。

### Phase 1：抽 gameplay data

優先搬低風險純資料：

- `AttributeStat`
- `EnergyInfo`
- `AttributeType`
- `AttributePropertyType`
- `AffectType`
- `DefeatedReason`

目的：讓 UI、Character、Skill 都能依賴同一個低層 gameplay model。

### Phase 2：抽 2D foundation

候選：

- `ITransform2D`
- `Line` / `LineEx`
- Collider2D / Physics2D extension
- `BaseColliderDetector2D` 的低層 cast 能力

注意：任何依賴 `CharacterController2D`、`GameManager`、`AttributeBehaviour` 的檔案不要在這階段搬。

### Phase 3：建立 command source 邊界

先不碰 network，先把本機玩家與 AI 都變成 command source：

- movement axis
- jump/action pressed/held/released
- skill index shift
- optional aim/target intent

目標是讓未來 network/replay/rollback 可以走同一條入口。

### Phase 4：拆 combat.2d 與 combat.animation

`CharacterController2D` 先成為 `combat.2d` 的主整合 component。`AnimatorCharacterController2D`、SMB、Skill animation bridge 進 `combat.animation`。

這階段要把直接呼叫 UI/Audio/GameManager cursor 的部分轉成事件，否則拆包後依賴會反向。

### Phase 5：拆 input / AI / UI / FX

等 command source 和 presentation event 穩定後再拆：

- `combat.input`: PlayerHost
- `combat.ai`: Autopilot + combat patrol adapter
- `combat.ui`: Attribute UI
- `combat.fx`: Caster/Fx sender/receiver

### Phase 6：Network package

Network 不應一開始就依賴 Unity component。建議先做純 C# interface：

- `ICommandChannel`
- `ISnapshotStore`
- `ILagSimulator`
- `ILocalLoopbackChannel`
- serialization contract

CombatAesthetic 的現有 controller 可以晚點接 adapter。先讓 network 成為「命令與快照通道」，不要讓它知道 Animator、Collider、UI。

## 最終依賴方向

理想方向如下：

```text
Core / common
  -> gameplay.core
    -> gameplay.2d
      -> combat.core
        -> combat.2d
          -> combat.animation
          -> combat.fx
          -> combat.ai
          -> combat.input
          -> combat.ui
          -> combat.network.adapter
```

嚴格規則：

- UI 不被 core 持有。
- Input/AI/Network 都只是 command source。
- Animation/FX/Audio 都只是 presentation output。
- Combat core 不依賴 UnityEngine.Animator、UnityEngine.UI、UnityEngine.InputSystem。
- 2D foundation 不依賴 GameManager、SceneController、CharacterController2D。

## 對現有 UnityExtension Packages 的對應建議

| 現有路徑 | 建議角色 |
| --- | --- |
| `Packages/Core` | 不放 gameplay 規則，只放真正通用 core |
| `Packages/common` | Pool、通用 Mono utility、通用 event/data adapter |
| `Packages/2D` | 2D collider/physics/graphic/tooling |
| `Packages/InputSystem` | 新 input bridge，PlayerHost 的依賴方向應往這裡 |
| `Packages/Animation` | Animator utilities、SMB base、IK、PlayableDirector |
| `Packages/UI` | UIControl、stat UI base、prompt/dialog 若可通用 |
| `Packages/AI` | Route/Patrol 通用層，不放 Combat-specific target decision |
| `Packages/Timeline` | Timeline/Playable 擴充 |
| `Packages/URP` | Rendering pipeline specific，Combat 不直接依賴 |
| `Packages/Plugins` | 第三方 plugin bridge，不放自家 gameplay |
| `Packages/UPM` | package template/tooling，不放 runtime |

新增 package 應放在同一個 `UnityExtension/Packages` 底下，命名保持清楚：

- `Gameplay`
- `Gameplay2D`
- `CombatCore`
- `Combat2D`
- `CombatAnimation`
- `CombatInput`
- `CombatAI`
- `CombatUI`
- `CombatFX`
- `Network`

## 暫時不要做的事

- 不要一開始就把 CombatAesthetic 整包拆成很多 UPM，會只是把耦合搬到 package manifest。
- 不要先寫 rollback/network，因為 command source 和 state snapshot 邊界還不穩。
- 不要把 `CharacterController2D` 當成 pure core，它現在是 Unity 2D component integration。
- 不要讓 UI、Audio、Cursor、Editor window 反向成為 combat base dependency。
- 不要在第一階段大改 namespace，避免 prefab/script reference 大量斷裂。

## 下一步討論題

1. `AttributeType` 這類 gameplay enum 要不要做成可擴充資料表，而不是 enum？
2. `HostData2D` 要保留 ScriptableObject authoring，還是切成 runtime command interface + ScriptableObject adapter？
3. Combat core 是否真的要純 C#，支援 server DLL？若是，`Vector2`/`Time`/`Physics2D` 都要被隔離。
4. `CombatAesthetic` 最後要變成 umbrella package、sample package，還是完全拆空只留文件？
