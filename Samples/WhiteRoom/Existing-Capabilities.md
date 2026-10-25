# WhiteRoom：既有能力與重用依據

2026-10-25 唯讀程式盤點。供 [實作計畫](Implementation-Plan.md#模組化累積與重用) 的階段 0 使用，記錄已找到的來源、用途及接用前要驗證的邊界；以下尚未經 Unity 宿主執行驗證。

## 累積位置與關係

```text
Yu5h1Lib 已累積的共用技術
├─ 輸入、移動介面、動畫協調、接地與姿態、鏡頭、theme
└─ CombatAesthetic 已累積的戰鬥設計與程式
   ├─ 操作來源、技能啟動限制、可操作區間、動畫與速度權重
   └─ WhiteRoom Sample：依既有能力組合並驗證 3D PvE 體驗
      └─ 乾淨 URP 宿主：引用套件、執行與建置
```

Yu5h1Lib 是本計畫的技術基礎。AI 的工作是理解、接用與補足這些累積；新宿主用來辨明實際依賴。既有 CombatAesthetic 的 2D 耦合，需要逐項處理，不能據此把整個 Yu5h1Lib 視為不適用 3D。

## 具體來源與接用方向

下列 UnityExtension 與 Core 路徑相對於 Yu5h1Lib 工作區。連結指向盤點時的原始碼；跨儲存庫來源不會隨 Sample 匯入，後續須以套件版本／Core DLL 對應來源固定依賴。UnityExtension 盤點基準為 `5e5d4ae14e391e32389d4cf2a1d2c14d189be2f4`，本表所讀路徑當時無未提交修改；CombatAesthetic 程式基準為 `b7a0d5d340983253a87dbb16fa88cd9fb217fb99`。

| WhiteRoom 需要 | 已存在的程式與行為 | 接用方向與驗證邊界 |
|---|---|---|
| PC 鍵鼠輸入 | [InputHandler](../../../UnityExtension/Packages/common/Runtime/Input/InputHandler.cs) 統一按下／持續／放開、滑鼠與軸查詢；[InputSystemBackend](../../../UnityExtension/Packages/InputSystem/Runtime/Backend/InputSystemBackend.cs) 已有新 Input System 後端及啟動註冊。 | 優先接用既有輸入入口與後端。核對宿主輸入設定及 action 名稱；不能把基本按鍵查詢當成已具備雙擊、緩衝及精準防禦判定。 |
| 玩家／AI 驅動移動 | [ILocomotor](../../../UnityExtension/Packages/Animation/Runtime/Core/ILocomotor.cs) 已定義世界座標 `Vector3` 速度命令與實際速度；[NPCLocomotor](../../../UnityExtension/Packages/Animation/Runtime/Component/NPCLocomotor.cs) 接命令、水平轉向／位移並更新動畫速度。 | 先評估沿用既有移動契約，再決定 3D 執行元件。現有 NPC 元件以 Transform 移動、關閉 root motion，未提供跳躍、重力與碰撞解算，不能直接視為完整戰鬥控制器。 |
| 腳與地面配合 | [GroundIKTarget](../../../UnityExtension/Packages/Animation/Runtime/Component/GroundIKTarget.cs) 以 3D Raycast 取得接地點、法線及 Collider，調整目標位置／旋轉並可平滑。 | 優先作足部接地候選。需驗證骨架、IK 接線及求值順序；此元件提供目標資料，不等於已完成全身 IK 或落地受身。 |
| 身體與受力姿態 | [BalanceHipController](../../../UnityExtension/Packages/Animation/Runtime/Component/BalanceHipController.cs) 依期望上方向、權重、角度上限與平滑修正髖部旋轉。 | 優先評估姿態修正接用。需確認飛行、受擊與倒地時的權重，避免修正與主要反應互相抵消；不能當成完整平衡、ragdoll 或衝擊模型。 |
| 第三人稱鏡頭 | [CameraController.LookAt3rdPerson](../../../Core/Runtime/Source/Control/CameraController.cs) 已有角度更新／限幅、繞 pivot 放置與朝向 pivot 的幾何處理。 | 先查 Core 公開 API 與出貨 DLL 是否對應，再評估接用。鏡頭避障、多方威脅構圖、滑鼠模式及角色朝向仍需體驗驗證；本輪不修改 Core。 |
| 動畫狀態協調 | [StateBehaviour](../../../UnityExtension/Packages/Animation/Runtime/Component/StateBehaviour.cs) 發送進入／離開事件；[AnimationController](../../../UnityExtension/Packages/Animation/Runtime/Component/AnimationController.cs) 管理目前狀態與 layer 啟用。 | 優先評估既有協調方式及與戰鬥狀態的關係。它們沒有因此自動具備取消、不可控制、接續三階段或輸入緩衝。 |
| 自有動畫的姿態與關鍵影格製作 | [AnimationToolWindow](../../../UnityExtension/Packages/Animation/Editor/Tool/AnimationToolWindow.cs) 可讀取有效 Humanoid Avatar 的當前姿態，將 muscle 與可選的 body transform 寫入指定時間的 AnimationClip 曲線，支援 Undo／保存；拒絕直接寫入模型內嵌 clip。此路徑查核時無未提交修改，最近檔案提交為 `28ebeda`。 | 此工具依賴 Unity Editor 與 Clip，只是既有姿態／曲線處理的證據，不符合新方向的跨平台來源與 runtime 製作入口要求。評估可保留的概念及可接用部分，不將整個工具當成必要管線；不能視為完整的 agent 自動製作流程。本輪不開啟工具或寫入動畫。 |
| 動作的可操作區間 | [CharacterSMB](../../Runtime/StateMachineBehaviour/CharacterSMB.cs) 已按 normalized time 門檻與可控制旗標決定 `Controllable`，並保存 root motion／剛體速度權重。 | 以此為既有設計依據，確認 3D 可沿用的資料與行為。單一恢復門檻尚未涵蓋全部 UX 區間，且基底與角色仍有耦合；不另寫一套相同概念再回頭合併。 |
| 出招合法性、成本與玩家／AI 共用規則 | [HostData2D](../../Runtime/Character/HostData2D.cs) 已分離操作來源；[SkillBehaviour](../../Runtime/Character/SkillBehaviour.cs) 檢查啟用、準備、可控制及技能限制並扣成本；[Autopilot](../../Runtime/AI/Autopilot.cs) 也有受可控制狀態限制的設計。 | 保留這些設計意圖，逐項確認可接用／應調整的歸屬。現有來源直接持有 2D 角色、使用 `Vector2`，AI 另依賴 Scanner2D／GameManager，須列出具體接合點後再做 3D 適配。 |
| 動畫與外力共同影響移動 | [AnimatorCharacterController2D.ProcessMovement](../../Runtime/Character/AnimatorCharacterController2D.cs) 已處理動畫速度與動作權重；[CharacterController2D](../../Runtime/Character/CharacterController2D.cs) 已有施力與意識影響控制的行為。 | 先對照原本的設計與 UX，再規劃 3D 受力整合。現有程式不是「出招必定保留原擊飛」的驗證證據；需要以空中受擊出招等案例檢驗，不能直接複製或把舊程式全部丟棄。 |
| AI 接近目標 | [NavMeshPathfinding](../../../UnityExtension/Packages/AI/Runtime/Component/NavMeshPathfinding.cs) 已透過 Unity NavMesh 計算路徑，區分完整／部分／無效路徑，發出路徑及目標可達性事件。 | 可評估為接近目標的既有工具；仍需接到角色移動與合法出招規則，並驗證宿主導航資料。路徑工具不等於戰鬥決策已完成。 |
| 亮暗 theme 與隊伍色 | [Theme](../../../UnityExtension/Packages/common/Runtime/Theme/Theme.cs) 已有具型別的參數查詢、schema 及 binding；[ThemeOptionSet](../../../UnityExtension/Packages/common/Runtime/Theme/ThemeOptionSet.cs) 可在選取時套用綁定。 | 優先評估既有 theme 資料與套用機制。3D 外殼／關節材質適配、隊伍色與 theme 的組合仍需核對，不能只看到 UI／Sprite 綁定就假定已涵蓋 URP 模型。 |

這是首交需求的具體起點，不是全部 Yu5h1Lib 的清單，也不是以上元件已在同一場景相容的結論。尚未盤點到的能力先標為需要查找，不能直接宣布不存在或新增替代實作。

## 階段 0 如何形成可實作依據

先以本表追查實際依賴與公開用法，為每項需求選定來源、版本及接線方式，再在獲准實作／驗證時補執行證據。優先驗證輸入、移動與動畫區間能否共同運作，再延伸接觸、防禦、受力及 AI；不要先做一份平行控制器才回頭找共用技術。

有差距時，先指出哪一段既有行為不滿足哪個 UX 案例，再選最窄的擁有者擴充。共用能力留在原擁有套件，戰鬥規則留在 CombatAesthetic，WhiteRoom 保存情境組合與示例資產；涉及其他專案時由需求文件提出，盤點不直接改來源。

階段 0 的依據必須能回答：這個操作用了什麼既有能力、保留了哪個原有設計、缺少什麼，以及未來要在哪裡驗證。只加入 Yu5h1Lib 引用卻重寫相同行為，不符合模組化累積的目標。
