# ARPG 核心玩法技術需求規格書 (換裝、昇華、培養與收集)

> 狀態：技術需求分析與架構規劃草案  
> 關聯文件：
> - 專案定位與動作美學原則：[plan.md](../../plan.md)
> - 成長與昇華玩法設計：[Progression-and-Ascension-Design.md](Progression-and-Ascension-Design.md)
> - 架構解耦與重構指引：[Refactor-Direction-Guide.md](Refactor-Direction-Guide.md)
> - 網路同步與連線評估：[Multiplayer-Action-Network-Interaction-Technology-Discussion.md](Multiplayer-Action-Network-Interaction-Technology-Discussion.md)
> - 當前狀態與未決事項：[handoff.md](../../handoff.md)

---

## 0. 系統階層與關係

```text
CombatAesthetic 動作對戰核心 (Action Combat & Feel)
│
├─ 換裝與外觀系統 (Modular Equipment & Appearance)
│  ├─ 骨骼共享與網格掛載 (Skinned Mesh & Bone Binding)
│  ├─ 武器掛點與動態判定框 (Socket Attachment & Hitbox Sync)
│  ├─ 外觀材質與物理動態 (Custom Shaders, Cloth & Dynamic Bones)
│  └─ 外觀與數值解耦 (Cosmetics vs. Equipment Stats)
│
├─ 人物昇華系統 (Ascension & Transformation)
│  ├─ 突破條件與契機判定 (Breakthrough Gates & Internalization)
│  ├─ 動作模組與連段重載 (Move-Set Overrides & Combo Mutation)
│  ├─ 形態切換與機制質變 (Form Shifts & Passive Ability Trait)
│  └─ 昇華演出與特效回饋 (Ascension Aura & Presentation Feedback)
│
├─ 角色培養系統 (Progression & Stat Cultivation)
│  ├─ 經歷累積與轉化 (Battle Potential → Rest/Meditation Internalization)
│  ├─ 屬性計算與修正器 (Attribute System, Base/Percent/Modifier Pipeline)
│  ├─ 裝備/技能強化與詞條 (Refinement, Socketing & Affix Generation)
│  └─ 資源與消耗管理 (Materials, Currency, Fatigue/Vitality Rules)
│
├─ 收集與圖鑑系統 (Collection & Codex)
│  ├─ 圖鑑資料結構 (Weapon/Monster/Costume/Lore Registry)
│  ├─ 解鎖進度追蹤 (Bitmask Flags & Condition Evaluators)
│  ├─ 全域圖鑑加成 (Account/Character Milestone Stat Injections)
│  └─ 掉落與拾取表現 (Loot Tables, Physical Drops & Magnetic Attract)
│
└─ 底層架構支援 (Foundational Infrastructure)
   ├─ 資料驅動配置 (Data-Driven Configuration: SO / JSON / FlatBuffers)
   ├─ 存檔與結構遷移 (Save System, Encryption & Schema Migration)
   ├─ 事件驅動通訊 (Decoupled Event Broker: Combat, State, View)
   └─ 效能與記憶體管理 (Addressables, Object Pool, Zero-GC Snapshot)
```

---

## 1. 需求背景與核心目標

在具備高水準打擊感、動作反饋與流暢操作（CombatAesthetic 核心美學）的 ARPG 遊戲中，長期的玩家驅動力來自於**外觀自定義（換裝）**、**角色能力質變（昇華）**、**持續數值成長（培養）**與**探索反饋（收集）**。

這四大系統不能只是單純的 UI 介面，它們與戰鬥判定、角色控制器、渲染批次、物理動態、存檔管理和網路同步產生高度的技術交織。

本文件旨在為結合這四項能力的 ARPG，梳理出完整且工程可落地的**技術需求規格**與**系統實作藍圖**。

---

## 2. 換裝能力 (Modular Equipment & Appearance) 技術需求

### 2.1 骨骼共享與網格組合 (Skinned Mesh Combination / Sharing)
* **需求目標**：角色不同身體部位（頭、胸、腿、手、足、披風等）能自由更換裝備，動作與骨骼必須完全對齊，無破皮與接縫問題。
* **技術手段**：
  1. **共用骨架（Shared Skeleton Hierarchy）**：所有裝備部位在 DCC 軟體中綁定同一套標準骨架（Standard Rig）。運行時將裝備 Prefab 的 `SkinnedMeshRenderer.bones` 映射指向宿主骨骼 Root。
  2. **動態網格烘焙（Runtime Mesh Combine / Baking）**：若在同螢幕多人同屏或低效能裝置，為避免過多 SubMesh 與 Draw Call 爆炸，需提供網格合併工具，將多個 `SkinnedMeshRenderer` 合併為單一 Mesh，並透過 UV 貼圖打包（Texture Atlas）合併 Material。
  3. **部位裁切遮罩（Body Part Blanking / Masking）**：當穿上長靴或全罩長袍時，透過骨骼權重裁切、Alpha Mask 貼圖或隱藏角色內部基礎網格，避免穿模（Z-Fighting / Clipping）。

### 2.2 武器掛點與動態 Hitbox 綁定 (Socket Attachment & Hitbox Sync)
* **需求目標**：換裝不同武器（單手劍、長槍、巨斧）時，武器模型正確掛載於手部/背部 Socket，且戰鬥判定框與打擊點跟隨武器變動。
* **技術手段**：
  1. **Socket Transform Anchor**：在角色骨架特定節點（`RightHand_Socket`, `Back_Socket`）預留掛載點。
  2. **動態 Hitbox 注入**：武器裝備資料包含對應的 `HitboxProfile`（長度、半徑、打擊判定盒座標）。當更換武器時，戰鬥控制器動態更新該武器的傷害判定範圍與特效發射點。
  3. **動態質心與慣性微調（可選）**：重型武器與輕型武器可為動作系統提供重量偏移係數，影響受擊反衝與起手蓄力時間。

### 2.3 外觀與數值解耦 (Cosmetics vs. Equipment Stats)
* **需求目標**：支援「裝備屬性」與「幻化外觀（Transmog / Wardrobe）」完全獨立。
* **技術手段**：
  * 資料層拆分為 `EquippedGearData`（提供戰鬥屬性計算）與 `AppearanceProfile`（提供渲染與模型實體加載）。

### 2.4 效能與動態物理 (Dynamic Bone & Draw Call Control)
* **技術手段**：
  * **布料與垂墜物理**：使用 `Dynamic Bone` 或 Unity `Cloth` / `Animation Rigging`，換裝時動態建立碰撞球體（Collider Proxy）與頭髮/裙襬的物理關聯。
  * **GPU Instancing 與 Material Property Block**：同款服飾的不同染色方案，透過 Shader 支援色盤替換（HSV Shift 或 Mask Tint），避免新增大量獨立 Material 實例。

---

## 3. 人物昇華能力 (Character Ascension) 技術需求

### 3.1 昇華判定與契機檢驗 (Ascension Gate & Verification)
* **需求目標**：符合 [Progression-and-Ascension-Design.md](Progression-and-Ascension-Design.md) 的設計，昇華不只是單純點擊按鈕升級，而是具備「累積達標 + 關鍵契機（挑戰/突破/領悟）」的質變。
* **技術手段**：
  1. **`IAscensionConditionEvaluator`**：定義突破條件評估器，包含多維度檢查：
     - 成長潛力或歷練累積值達標。
     - 特定副本通關或擊敗指定 Boss。
     - 特定招式使用次數或受擊/極限閃避次數。
  2. **狀態機鎖定與突破流程**：在非戰鬥安全區（或戰鬥中的特定無敵領悟時間窗）觸發，觸發時進入不可中斷的昇華狀態。

### 3.2 動作模組動態重載 (Move-Set & Combo Mutation)
* **需求目標**：昇華後角色戰鬥模式發生質變，例如：解鎖全新連段（Combo Branch）、蓄力衍生招式、甚至切換架勢/形態。
* **技術手段**：
  1. **`AnimatorOverrideController` 動態置換**：保留同一個 StateMachine 架構，運行時將基礎攻擊的 Animation Clip 動態替換為進階攻擊動畫。
  2. **動作指令解析器分支重定向（Action Interpreter Routing）**：
     - 基礎形態：`Attack1 -> Attack2 -> Attack3`。
     - 昇華形態：`Attack1 -> (Hold) -> SpecialChargedSlash -> HeavyFinisher`。
  3. **Cancel Window 與 Frame Data 變更**：昇華後可能開放「受擊閃避取消（Hitstun Cancel）」或更早的「後搖取消窗口（Action Cancel Window）」。

### 3.3 形態切換與機制質變 (Form Shifts & Mechanism Mutators)
* **技術手段**：
  1. **被動特性管道（Trait / Mutator Pipeline）**：角色戰鬥邏輯掛載 `List<ICombatMutator>`，昇華可注入如「攻擊附加霸體判定」、「完美格擋反彈飛行道具」等獨立規則物件。
  2. **模型與 Shader 形態演變**：
     - 昇華特徵（如發光紋路、法相、背後神環、翅膀）透過 Sub-Mesh 的啟用或 Dissolve 著色器動態展開。
     - 專用 VFX 與環境色調微調（Post-Processing 瞬時衝擊感）。

---

## 4. 培養機制 (Progression & Cultivation) 技術需求

### 4.1 戰鬥經歷累積與內化架構 (Accumulation & Internalization)
* **需求目標**：實現「戰鬥累積潛力 → 休息/睡眠內化為能力 → 冥想自主定向突破」的核心循環。
* **技術手段**：
  1. **雙層經歷資料模型**：
     - `PendingExperience`（未內化潛力）：戰鬥擊殺、完成探索時獲得，死亡可能有懲罰機制。
     - `InternalizedExperience`（已轉化修為）：透過營地休息、特定安全區冥想轉化為真實能力點數，具備防重複發放保證。
  2. **抗作弊與空耗防範演算法**：疲勞系統依據「有效挑戰強度」評估權重，若無有效戰鬥或訓練輸入，反覆進行休息操作不會獲得任何數值轉換。

### 4.2 模組化屬性計算引擎 (Gameplay Attribute System)
* **需求目標**：支撐生命、體力、攻擊力、暴擊、防禦、抗性、蓄力速度等複雜數值，並能精確處理裝備、Buff、昇華加成的加乘順序。
* **技術手段**：
  1. **三層計算管道**：
     $$\text{FinalStat} = (\text{Base} + \sum \text{FlatAdd}) \times (1 + \sum \text{PercentAdd}) \times \prod (1 + \text{IndependentMultiplier})$$
  2. **屬性變更事件解耦**：
     - `AttributeStat` 保持為純資料，脫離 Unity UI 依賴（落實 [Refactor-Direction-Guide.md](Refactor-Direction-Guide.md) 指引）。
     - 提供 `OnAttributeChanged(AttributeType, oldValue, newValue)` 事件，由 UI、戰鬥控制器、狀態機訂閱，禁止屬性核心持有 UI 物件。

### 4.3 裝備與詞條養成 (Affix & Refinement System)
* **技術手段**：
  1. **隨機詞條引擎（Affix Generator）**：支援前綴（Prefix）、後綴（Suffix）、固有詞條與套裝效果（Set Bonus）。
  2. **裝備強化與精煉階級**：提供穩定的消耗公式與等級上限管理。
  3. **鑲嵌與靈石系統**：裝備插槽（Sockets）可動態裝配提供額外 Passive 特性的寶石/核心。

---

## 5. 收集機制 (Collection, Compendium & Codex) 技術需求

### 5.1 典籍與圖鑑資料庫 (Codex Registry)
* **需求目標**：涵蓋怪物圖鑑（Bestiary）、武器收藏（Weapon Armory）、時裝衣櫃（Wardrobe）、成就碎片與世界觀文檔。
* **技術手段**：
  1. **唯讀靜態配置庫（Static Catalog / ScriptableObject Registry）**：每項可收集實體具備唯一識別碼 `EntryId`、類別、稀有度、立繪、3D 預覽模型路徑與解鎖描述。
  2. **輕量化玩家收集狀態庫（Player Collection State）**：
     - 對於單純開關型的解鎖，採用 `BitArray` 或 `Bitmask HashSet` 壓縮儲存（10,000 個圖鑑項目僅需約 1.25 KB 記憶體）。
     - 對於階梯型收集（如擊殺數量 1/10/100 解鎖不同故事），使用 `Dictionary<int, int>` 記錄計數。

### 5.2 收集條件判定引擎 (Condition Evaluator)
* **技術手段**：
  * **監聽全域遊戲事件（Game Events Broker）**：
    - `OnEnemyDefeated(enemyId)` -> 遞增怪物圖鑑計數。
    - `OnItemAcquired(itemId)` -> 標記道具圖鑑為已取得。
    - `OnAreaDiscovered(zoneId)` -> 探索率計算。

### 5.3 圖鑑里程碑與全域屬性加成 (Collection Milestone Buffs)
* **需求目標**：當收集度達到特定百分比（或收集完整特定套裝/派系）時，提供全帳號或角色的常駐屬性加成。
* **技術手段**：
  * **動態 Modifier 注入**：圖鑑系統作為獨立的屬性來源（`IAttributeModifierProvider`），將解鎖加成直接註冊進屬性計算管道，不需要修改角色本體存檔。

### 5.4 掉落與拾取表現 (Loot Tables & Physical Pickup)
* **技術手段**：
  1. **加權隨機掉落表（Weighted Loot Table）**：支援保底、階層掉落與動態掉落率修正。
  2. **物理拋物線與磁吸吸附（Loot Dynamics）**：掉落物產出時帶有初始向外衝量，落地後在玩家進入拾取半徑時以動態插值（Lerp / Spring）平滑吸向角色，拾取完成發出音效並寫入暫存背包。

---

## 6. 底層關鍵工程挑戰與架構支撐

### 6.1 動作戰鬥手感與確定性 (Combat Determinism & Feel)
* **關鍵技術**：
  1. **Hitstop（頓挫感/卡肉）**：命中瞬間暫停攻擊者與受擊者的動畫數個 Frame（如 0.05~0.15 秒），同時觸發微小位移震動，營造力道。
  2. **Camera Shake & Directional Knockback**：擊退速度基於打擊方向向量與受擊者韌性計算，相機採平滑衰減振動。
  3. **定點數與回滾考量（若涉及多人對戰）**：依照 [plan.md](../../plan.md) 第 3 節，物理與移動模擬須考慮邏輯與表現分離，使單機與連線能共用核心計算。

### 6.2 存檔、結構遷移與防竄改 (Save System & Schema Migration)
* **關鍵技術**：
  1. **資料版本化（Schema Versioning）**：版本更新時，舊存檔結構需透過遷移器（Migration Pipeline）平滑升級，不導致玩家收集品遺失。
  2. **非對稱加密與本機校驗碼**：離線單機存檔採 AES 加密與雜湊校驗（HMAC），防止本地記憶體直接竄改培養數值。
  3. **增量快照（Dirty-Flag Delta Snapshot）**：自動存檔僅序列化發生變更的區塊（如僅更新體力與當前經驗），降低 I/O 卡頓。

### 6.3 資產生命週期管理 (Addressables & Memory Management)
* **關鍵技術**：
  1. **外觀與武器動態加載（Addressables System）**：換裝系統具備大量模型、貼圖與材質，嚴禁全數預加載至記憶體中。必須依據使用情況動態加載，卸載時確實呼叫 `Addressables.Release`。
  2. **物件池（Object Pooling）**：掉落物、打擊特效（VFX）、浮動傷害字體（Damage Text）必須採用零 GC 的物件池管理。

---

## 7. 與 CombatAesthetic 現有代碼之銜接建議

根據 [Refactor-Direction-Guide.md](Refactor-Direction-Guide.md) 與現有代碼盤點：

| 既有類別 | 現狀問題 | ARPG 擴充改造方向 |
| --- | --- | --- |
| `AttributeStat` / `AttributeBehaviour` | 數值與 UI 強烈耦合，缺乏多層加成運算 | 抽為純 C# `GameplayCore.Attribute`，支援加乘分層計算，UI 改為訂閱事件。 |
| `CharacterController2D` | 混合了位移、受擊、輸入與多種組件 | 縮減為純指令執行器，外觀與換裝交由外層掛載組件負責。 |
| `AnimatorCharacterController2D` | 動畫控制器直接呼叫 FX、Audio 與 UI | 引入 `CombatEvent` 事件機制；昇華與武器換裝時動態注入 `AnimatorOverrideController`。 |
| `HostData2D` | 僅支援本地單一輸入源 | 抽象為 `ICharacterCommandSource`，便於未來接入 AI、連線或劇情腳本控制。 |
| *(新需求)* `AppearanceController` | 專案尚無換裝控制器 | 負責管理裝備槽位、骨骼映射、武器掛載點與外觀更新。 |
| *(新需求)* `AscensionController` | 專案尚無昇華管理組件 | 負責判定突破條件、管理形態切換、通知動作模組重載與昇華特效。 |
| *(新需求)* `CodexManager` | 專案尚無收集圖鑑系統 | 負責全域事件監聽、位元旗標記錄、圖鑑加成數值向屬性系統的注入。 |

---

## 8. 驗收標準與後續步驟

### 8.1 階段驗收標準
1. **換裝驗收**：角色在更換 3 套不同部位裝備與 2 種不同尺寸武器時，動作播放正常、無破皮穿模，武器 Hitbox 正確跟隨攻擊動作判定。
2. **昇華驗收**：滿足特定歷練條件並觸發突破後，角色招式連段與被動特性產生實質改變，視覺外觀有明確質變回饋。
3. **培養驗收**：實戰累積潛力值在休息/冥想後能穩定轉化為屬性成長，加成計算依公式正確結算，且同一份收益不被重複發放。
4. **收集驗收**：解鎖新怪物/裝備時，圖鑑正確點亮並在達成里程碑時向角色注入全域加成，存檔與讀檔後狀態完全一致。

### 8.2 後續行動建議
1. 依據 [Refactor-Direction-Guide.md](Refactor-Direction-Guide.md)，優先完成 `AttributeBehaviour` 的資料與 UI 解耦。
2. 開發 `AppearanceController` 原型，驗證 Unity 骨骼共享與武器 Socket 機制。
3. 依據 [Progression-and-Ascension-Design.md](Progression-and-Ascension-Design.md) 實作初版「實戰累積 → 休息轉化」之培養測試案例。
