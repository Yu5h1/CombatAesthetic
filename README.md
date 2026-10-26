# Combat Aesthetics

第三人稱 ARPG 戰鬥體驗的 Unity 整合套件。以移動、攻擊、防禦、受擊與協作，讓玩家判讀四面八方的威脅，透過操作與戰友配合應對情境；動作表現保留重量與受力的因果。

## 專案分工

```text
Yu5h1Lib
├─ UX：跨平台戰鬥操作體驗、行為規則與動畫格式規範
├─ Character-Animation：agent／人類友善的動畫系統與製作流程整合
├─ Web：Web 共用套件、Viewer 與製作／預覽入口
└─ Unity/Combat-Aesthetics：Unity 戰鬥整合
   └─ Samples~/WhiteRoom：角色、動畫、戰鬥與連線的測試場
```

UX 說明玩家要經歷什麼、操作和結果應如何對應，作為各平台共同依據。此套件的 [WhiteRoom 實作計畫](Samples~/WhiteRoom/Implementation-Plan.md) 說明 Unity 端如何分階段整合與驗證；動畫系統、Web 共用能力與格式規範按各自歸屬接用。

工作區中的戰鬥體驗來源為 `UX/Documentation/CombatAesthetic-Experience-Discovery.md`；動畫系統方向為 `Character-Animation/plan.md`，Web 共用觀看邊界為 `Web/Documentation/ViewerInteractionPlan.md`。動畫格式仍在評估，正式規範由 UX 維護。

## WhiteRoom 發展方向

- 第一個可操作交付聚焦 PC 鍵鼠、第三人稱 3D 徒手 PvE：八方向移動、跳躍、原地／方向攻擊、普通／精準防禦、受擊恢復與快速重設。
- 白色測試場搭配簡化機械人形，優先看清朝向、動作、接觸與後果；首個 Unity 驗證基線為 `6000.3.9f1`／URP。
- 動畫製作先以 Web 驗證單一人形出拳的姿勢／時序修改、預覽、保存與重載，agent 與人類都能接續修改成果。
- 連線先從本機雙端進到區域網路兩台電腦，各控制一個角色共同攻防；後續擴至網際網路，再評估雲端與玩家服務。PvP、PvE 與協作持續發展。

原始碼保留既有 2D 能力，3D WhiteRoom 來源位於 `Samples~/WhiteRoom`，並已登錄為套件 Sample。上述方向的實際完成程度與相容性，以 [Report](report.json) 及各項驗證證據為準；本次文件整理未執行 Unity 或連線測試。

## 文件入口

- [WhiteRoom 實作與驗證計畫](Samples~/WhiteRoom/Implementation-Plan.md)
- [既有能力與重用證據](Samples~/WhiteRoom/Existing-Capabilities.md)
- [套件整合與模組化方向](plan.md)
- [續接入口](handoff.md)

Unity 套件識別維持 `com.yu5h1.combataesthetic`。
