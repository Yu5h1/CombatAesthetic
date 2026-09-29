# CombatAesthetic 重構方向指引

本文目的不是列術語，而是建立一個能逐步拆舊代碼的判斷框架。CombatAesthetic 的核心應是「動作對戰美學」，不是所有系統的集合。因此重構時要先問：這段代碼是在表達戰鬥美學，還是在提供通用系統能力？

## 先從哪裡開始

建議先從 `AttributeBehaviour`、`HostData2D`、`CharacterController2D` 三條線開始，而不是直接拆 package 或導入大型架構。

順序如下：

1. 先清楚化資料模型：Attribute / Energy / Cost。
2. 再清楚化指令來源：玩家、AI、Network、Replay 都只是 command source。
3. 再清楚化角色執行器：CharacterController2D 只負責消費 command 並推進狀態。
4. 最後清楚化表現輸出：Animator、FX、Audio、UI、Camera 都只訂閱事件或狀態。

這四步會自然導向 package 拆分。反過來如果先搬檔，耦合只會換位置。

## 第一刀：Attribute 不是 UI，也不是 Character

目前問題：

- `AttributeStat` 是純資料，但放在 CombatAesthetic。
- `AttributeBehaviour` 同時負責數值、恢復、扣血、UI refresh、死亡判定。
- `AttributeBehaviour` 持有 `UI_Attribute`，導致 core model 依賴 presentation。

目標方向：

```text
GameplayCore
  AttributeStat
  AttributeType
  AffectType
  EnergyInfo
  AttributeChangedEvent data

Unity Gameplay Adapter
  AttributeBehaviour

UI Package / CombatUI
  AttributeView / UI_Attribute
```

重構判斷：

- 純資料與規則可先抽到 `GameplayCore`。
- Unity lifecycle、SerializeField、Coroutine 留在 Unity adapter。
- UI 不應由 Attribute 主動刷新，而是訂閱 AttributeChanged。

這一步最值得先做，因為它會讓 Skill、UI、Character 都少一個共同耦合點。

## 第二刀：HostData2D 是命令來源，不是角色本體

目前問題：

- `HostData2D.HostBehaviour2D` 直接持有 `CharacterController2D Body`。
- `PlayerHost` 直接讀 `GameManager.InputModule.input`。
- `Autopilot` 直接知道 `AnimatorCharacterController2D`、`Scanner2D`、`Patrol`、`AudioManager`。

目標方向：

```text
ICharacterCommandSource
  GetMovement()
  GetActionState()
  GetSkillSwitch()
  GetAimIntent()

PlayerCommandSource
AICommandSource
NetworkCommandSource
ReplayCommandSource
```

`CharacterController2D` 不需要知道指令來自鍵盤、AI、伺服器還是錄影回放。這是多人與單機共用的入口。

這裡可以使用你已經理解的「同目的同行為，不同實作」觀念。玩家、AI、Network 都是同一目的：提供角色指令；只是實作不同。

## 第三刀：CharacterController2D 先變瘦，不急著變純

目前 `CharacterController2D` 是大耦合中心。它現在同時做：

- movement
- gravity / slope / jump
- input polling
- attribute damage
- detector / scanner / caster / teleport component wiring
- invincibility visual flicker
- GameManager pause
- PlayerPrefs tuning

短期目標不是立刻變成完美純 C#，而是先把它變成「角色執行器」：

```text
CharacterController2D
  consumes command
  updates movement
  emits events
  queries required adapters/interfaces
```

應該移出去的責任：

- `PlayerPrefs` tuning -> setting asset
- invincibility visual flicker -> presentation component
- UI/audio/cursor -> event subscriber
- input polling -> command source
- attribute calculation -> gameplay model/service
- scanner/caster/teleportable hard reference -> optional component adapter

## 第四刀：AnimatorCharacterController2D 是表現橋，不是核心

目前它控制了：

- Animator root motion
- StateMachineBehaviour
- Skill selection
- FX cast
- Audio play
- UI/cursor
- hit reaction

重構方向：

```text
Combat2D
  Character state and combat events

CombatAnimation
  Animator bridge
  SMB bridge
  root motion interpretation

CombatFX / Audio / UI
  subscribe combat events
```

例如 `CastFXByIndex`、`PlayAudioClip`、`attribute.ui.Hide` 這類直接呼叫，都表示表現層互相纏住。未來應改成：

```text
CombatEvent.SkillCast
CombatEvent.HitConfirmed
CombatEvent.CharacterDefeated
CombatEvent.InvincibilityStarted
```

然後 Animator/FX/Audio/UI 各自訂閱。

## Yu5h1Lib 的注入概念怎麼放進來

你現在描述的注入比較接近 Yu5h1Lib 的 Adapter/Registration 模式：

- 同一個目的或行為介面。
- 多個不同 Unity component 實作。
- RuntimeInitializeOnLoad 時註冊 concrete type -> adapter/interface。
- 使用端只找「能處理這個目標的 adapter」，不硬寫具體類別。

這套很適合用在 CombatAesthetic 的「周邊適配」：

| 用途 | 可以注入/Adapter 化 |
| --- | --- |
| Attribute view | `IAttributeViewAdapter` 對應不同 UI 實作 |
| Command source | `ICharacterCommandSource` 對應 Player/AI/Network |
| Hit target | `IHitReceiver` 對應 Character、Breakable、Projectile |
| Hurt source | `IDamageSource` 對應 hitbox、trap、environment |
| Presentation | `ICombatPresentation` 對應 Animator、FX、Audio、Camera |
| Value binding | `IValuePortAdapter<T>` 對應 UI/MVVM binding |

但不建議一開始把所有東西都注入化。先用在「同目的、多實作、需要被統一操作」的位置。

## 社群術語該怎麼看

### DI / Dependency Injection

現在不需要先導入完整 DI container。你需要的是「依賴反轉」：

- 高層不直接 new 低層。
- 高層不直接找具體 UI/Audio/Input。
- 高層依賴 interface 或 adapter。

Yu5h1Lib 的 AdapterFactory 已經能解一部分問題。先把 interface 邊界設計好，比導入 Zenject/VContainer 之類 container 更重要。

### ScriptableObject Architecture

SO 很適合：

- authoring data
- skill config
- input/AI profile
- tuning setting
- event channel asset
- reusable preset

SO 不適合承擔所有 runtime state。像 health current、rollback state、network snapshot 這類會快速變動或需要複製的資料，不應全部塞進 SO。

### ECS

目前不建議先轉 ECS。原因：

- 你的問題主要是責任邊界與 package 邊界，不是效能資料布局。
- 2D 外包遊戲先需要可維護、可調手感。
- ECS 會放大 authoring 與除錯成本。

但 ECS 的思考有用：資料與行為分離、系統批次處理、避免 MonoBehaviour 大物件。可以吸收思想，不必導入框架。

### Event / Message / Signal

這個需要關注。CombatAesthetic 很適合事件化，因為戰鬥美學本質上是「核心事件驅動表現」：

- hit confirmed -> hit stop / FX / sound / camera shake
- guard break -> UI / animation / slow motion
- skill cast -> cast VFX / resource cost / cooldown
- defeated -> animation / input lock / scene logic

事件能把 Character、Animator、FX、UI、Audio 拆開。

## 最小重構路線

### Step 1：畫責任圖

先對每個大類標記：

- Model：純資料/規則
- Command：輸入/AI/網路命令
- Controller：消費命令、推進狀態
- Presentation：動畫/FX/UI/音效/鏡頭
- Tooling：Editor 或 authoring 工具

### Step 2：先建立 interface，不搬檔

先設計小 interface：

- `IAttributeSource`
- `IAttributeReceiver`
- `ICharacterCommandSource`
- `IHitReceiver`
- `ICombatEventSink`

先讓舊類別實作它們。這是低風險重構。

### Step 3：讓直接呼叫變成事件

優先處理這種味道：

- controller 直接呼叫 UI
- controller 直接呼叫 AudioManager
- animation controller 直接改 cursor
- AttributeBehaviour 直接刷新 UI

先改成 event，再讓原本 UI/Audio 用 subscriber 接回去。行為不變，但依賴方向會變乾淨。

### Step 4：抽出純資料

資料先搬最安全：

- AttributeStat
- EnergyInfo
- combat event data
- command data

這些是 package 化的第一批。

### Step 5：再拆 package

當舊類別已經依賴 interface/event，而不是互相直接持有，搬 package 才會順。

## 我建議現在的第一個具體工作

先不要碰 network，不要碰 ECS，不要導入 DI container。

第一個具體工作應該是：

> 替 `AttributeBehaviour` 與 `CharacterController2D` 畫出責任切割圖，並新增最小 interface / event 草案。

原因：

- Attribute 是 UI、Skill、Character 的共同耦合點。
- CharacterController2D 是 input、physics、combat、presentation 的共同耦合點。
- 這兩個釐清後，後面的 package 拆分會自然很多。

完成這一步後，再進入 command source：

> 把 `HostData2D` 的概念整理成 `ICharacterCommandSource`。

這會讓 Player、AI、Network 的路線同時清楚。
