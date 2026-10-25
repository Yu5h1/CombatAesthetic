# WhiteRoom：宿主依賴靜態盤點與相容性分析

> 執行項目：`white-room-stage0` › `host-dependency-inventory`  
> 基準日期：2026-10-25  
> 盤點方式：靜態程式碼、組件定義與套件 Manifest 唯讀盤點（未啟動 Unity、未還原依賴、未修改現有套件與專案）

---

## 1. 盤點目標與環境基準

依 [WhiteRoom 實作計畫](../Implementation-Plan.md#實作階段) 階段 0 要求，WhiteRoom 測試場將在一個「乾淨的 3D URP 專案」作為首個驗證宿主。宿主固定使用本機已安裝的 **Unity 6000.3.9f1**（具體版本號 `6000.3.9f1 (7a9955a4f2fa)`）。

本報告針對 `com.yu5h1.combataesthetic` 套件資訊、組件定義（asmdef）、Yu5h1Lib 內部套件及 Core DLL 來源進行完整靜態核對，列出在 Unity 6000.3.9f1 URP 宿主環境中安裝此套件所需的必要依賴、版本對應、架構缺口與未驗證事項。

---

## 2. CombatAesthetic 套件定義與組件參考盤點

### 2.1 套件資訊 (`package.json`)

* **檔案路徑**：`Unity/Combat-Aesthetics/package.json`
* **套件名稱**：`com.yu5h1.combataesthetic`
* **版本**：`1.0.0`
* **宣稱 Unity 版本**：`2024.1`
* **宣告依賴項 (`dependencies`)**：
  ```json
  {
    "com.unity.feature.2d": "1.0.0",
    "com.unity.inputsystem": "1.7.0",
    "com.unity.textmeshpro": "3.0.6",
    "com.yu5h1.library": "1.0.1",
    "com.yu5h1.common": "1.0.0",
    "com.yu5h1.2d": "0.1.0"
  }
  ```

---

### 2.2 執行階段組件定義 (`Yu5h1Lib.CombatAesthetic.asmdef`)

* **檔案路徑**：`Unity/Combat-Aesthetics/Runtime/Yu5h1Lib.CombatAesthetic.asmdef`
* **組件名稱**：`Yu5h1Lib.CombatAesthetic`
* **組件 GUID**：`5bebe61b49d4f3b4ab967767b260ea4c`
* **組件屬性**：`autoReferenced: true`, `overrideReferences: false`, `noEngineReferences: false`
* **參考 GUID 解析對照表**：

| asmdef 內記載之 GUID | 解析目標組件名稱 | 來源套件／位置 | 說明與狀態 |
|---|---|---|---|
| `41524c21c95e5fe4dbc5b48bd21995a4` | `Unity.2D.Animation.Runtime` | `com.unity.2d.animation` | Unity 官方 2D 動畫 Runtime |
| `88f14d179e7c91c4eac28302d69f32e1` | `Unity.2D.IK.Runtime` | `com.unity.2d.animation` (IK) | Unity 官方 2D IK Runtime |
| `75469ad4d38634e559750d17036d5f7c` | `Unity.InputSystem` | `com.unity.inputsystem` | Unity 官方新 Input System |
| `6055be8ebefd69e48b49212b09b47b2f` | `Unity.TextMeshPro` | `com.unity.ugui` (Unity 6) / `com.unity.textmeshpro` | 文字排版組件 |
| `15bc4668ef1785848a9c85a0b340b9da` | `Yu5h1Lib.UI` | `com.yu5h1.ui` (`Unity/UnityExtension/Packages/UI`) | **缺口**：未在 `package.json` 宣告依賴 |
| `0a2ad6279ef01794b8610804d06308fa` | `Yu5h1Lib.Common` | `com.yu5h1.common` (`Unity/UnityExtension/Packages/common`) | 已在 `package.json` 宣告 |
| `e097909683439544eb9da6a0b8f26f54` | `Yu5h1Lib.2D` | `com.yu5h1.2d` (`Unity/UnityExtension/Packages/2D`) | 已在 `package.json` 宣告 |
| `f12f63ae204b605429c6f84beccb3dd3` | `Yu5h1Lib.Animation` | `com.yu5h1.animation` (`Unity/UnityExtension/Packages/Animation`) | **缺口**：未在 `package.json` 宣告依賴 |

---

### 2.3 編輯器組件定義 (`Yu5h1Lib.CombatAesthetic.Editor.asmdef`)

* **檔案路徑**：`Unity/Combat-Aesthetics/Editor/Yu5h1Lib.CombatAesthetic.Editor.asmdef`
* **組件名稱**：`Yu5h1Lib.CombatAesthetic.Editor`
* **平台限制**：`Editor`
* **參考 GUID 解析對照表**：

| asmdef 內記載之 GUID | 解析目標組件名稱 | 來源套件／位置 | 說明 |
|---|---|---|---|
| `5bebe61b49d4f3b4ab967767b260ea4c` | `Yu5h1Lib.CombatAesthetic` | `Unity/Combat-Aesthetics/Runtime` | 參考自家的 Runtime 組件 |
| `0a2ad6279ef01794b8610804d06308fa` | `Yu5h1Lib.Common` | `com.yu5h1.common` | 常用共用工具 |
| `15bc4668ef1785848a9c85a0b340b9da` | `Yu5h1Lib.UI` | `com.yu5h1.ui` | UI 編輯輔助 |
| `6055be8ebefd69e48b49212b09b47b2f` | `Unity.TextMeshPro` | `com.unity.ugui` | TMP 相關欄位 Drawer |

---

## 3. Yu5h1Lib 內部套件與 Core DLL 來源追溯

### 3.1 `Yu5h1Lib.Core`（預編譯 DLL）建置管線

* **專案檔**：`Unity/Core/Runtime/Yu5h1Lib.Runtime.csproj` 與 `Unity/Core/Editor/Yu5h1Lib.Editor.csproj`
* **目標架構**：`netstandard2.1;net48`（編譯常數包含 `UNITY`）
* **產出複製機制**：
  專案檔包含 PostBuild 目標：
  ```xml
  <Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="'$(TargetFramework)'=='netstandard2.1'">
    <Exec Command="xcopy /y /d &quot;$(TargetDir)$(ProjectName).dll&quot; &quot;$(MSBuildProjectDirectory)\..\..\UnityExtension\Packages\Core\Runtime&quot;" />
  </Target>
  ```
* **發布載體**：
  編譯後的 DLL 部署於 `Unity/UnityExtension/Packages/Core/Runtime/Yu5h1Lib.Runtime.dll`（99KB / 214KB）。
* **Unity 匯入設定 (`.meta`)**：
  `Yu5h1Lib.Runtime.dll.meta` 設定為 `isExplicitlyReferenced: 0`, `isOverridable: 1`，適用於 Editor、Win、Win64、Android、Linux64、OSXUniversal、WebGL。因為 `Yu5h1Lib.CombatAesthetic.asmdef` 的 `overrideReferences: false`，此 DLL 會被 Unity 自動注入為全域可用參考。
* **關鍵能力驗證**：
  `Existing-Capabilities.md` 所列之 `CameraController.LookAt3rdPerson` 確實在 `Unity/Core/Runtime/Source/Control/CameraController.cs` 原始碼內（命名空間 `Yu5h1Lib.Runtime`），並已編入 `Yu5h1Lib.Runtime.dll`。

---

### 3.2 相關 Yu5h1Lib 本地套件位置與關係

| 套件識別碼 | 本地相對路徑 | 宣告版本 | 依賴項 | 備註 |
|---|---|---|---|---|
| `com.yu5h1.library` | `Unity/UnityExtension/Packages/Core` | `1.0.1` | 無 | 包含 `Yu5h1Lib.Runtime.dll` 與 `Yu5h1Lib.Editor.dll` |
| `com.yu5h1.common` | `Unity/UnityExtension/Packages/common` | `1.0.0` | `com.yu5h1.library` | `Yu5h1Lib.Common.asmdef` 原始碼 |
| `com.yu5h1.2d` | `Unity/UnityExtension/Packages/2D` | `0.1.0` | `com.yu5h1.library`, `com.yu5h1.common` | `Yu5h1Lib.2D.asmdef` 原始碼 |
| `com.yu5h1.ui` | `Unity/UnityExtension/Packages/UI` | `1.0.0` | `com.unity.ugui`, `com.yu5h1.common` | `Yu5h1Lib.UI.asmdef` 原始碼 |
| `com.yu5h1.animation` | `Unity/UnityExtension/Packages/Animation` | `0.0.0` | `com.yu5h1.common` | `Yu5h1Lib.Animation.asmdef`，引用 `Unity.Mathematics`、`Unity.Splines` |
| `com.yu5h1.inputsystem` | `Unity/UnityExtension/Packages/InputSystem` | `0.1.0` | `com.yu5h1.common` | 新 Input System 後端轉接層 |
| `com.yu5h1.urpextension` | `Unity/UnityExtension/Packages/URP` | `1.0.0` | `com.yu5h1.library`, `com.unity.render-pipelines.universal` | URP 擴充輔助 |

---

## 4. 現有專案基準比對（BonghuoVR 6000.3.9f1 vs KFC 6000.6.0f1）

本機已有的兩組 Unity 專案提供了重要的版本參考：

1. **`W:\UnityProject\BonghuoVR`（原生 6000.3.9f1 URP 專案）**：
   * Unity 核心：`6000.3.9f1 (7a9955a4f2fa)`
   * Universal RP：`com.unity.render-pipelines.universal: 17.3.0`
   * Input System：`com.unity.inputsystem: 1.18.0`
   * uGUI：`com.unity.ugui: 2.0.0`（在此版本中 TMP 已整併入 uGUI）
   * Splines：`com.unity.splines: 2.8.4`
   * Navigation：`com.unity.ai.navigation: 2.0.10`

2. **`W:\UnityProject\KFC`（引用 Combat-Aesthetics 的 6000.6.0f1 專案）**：
   * Universal RP：`17.6.0`
   * 2D Feature：`com.unity.feature.2d: 2.0.2`
   * 本地套件引用方式：全部透過 `file:C:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/...` 釘選於 `manifest.json`。

---

## 5. 依賴缺口與相容性風險分析

### 缺口 1：`package.json` 缺少必要套件宣告（嚴重）
* **現狀**：`Yu5h1Lib.CombatAesthetic.asmdef` 明確包含對 `Yu5h1Lib.UI` 與 `Yu5h1Lib.Animation` 的 GUID 參考，但 `package.json` 的 `dependencies` **完全沒有宣告** `com.yu5h1.ui` 與 `com.yu5h1.animation`。
* **影響**：在一個全新建立的乾淨宿主中，若宿主 `manifest.json` 僅宣告 `"com.yu5h1.combataesthetic": "file:..."`，Unity Package Manager 不會自動解析 UI 與 Animation 套件，導致 CombatAesthetic 在匯入時立即報編譯錯誤（找不到 asmdef 參考）。
* **處置建議**：在宿主 `manifest.json` 中手動補足這兩項 local package 參照，或待獲准修改套件定義時補齊 `package.json`。

---

### 缺口 2：Unity 6 (6000.3.9f1) 套件版本規範不符
* **現狀**：
  * `package.json` 宣告 `"com.unity.textmeshpro": "3.0.6"`。在 Unity 6 中，TextMeshPro 獨立套件已除役，統一整併至 `com.unity.ugui`（`2.0.0` 以上版本）。
  * `package.json` 宣告 `"com.unity.feature.2d": "1.0.0"`。在 Unity 6 中，2D Feature 包已升級為 `2.0.x`。
* **影響**：宿主專案需使用 Unity 6 對應的 package 設定，否則 UPM 解析可能引發衝突或警告。

---

### 缺口 3：Runtime 程式碼存在未防護之 `using UnityEditor;`（阻礙 Windows 建置）
* **現狀**：
  * `Runtime/Extension/ParticleSystemEx.cs` 第 3 行：`using UnityEditor;`
  * `Runtime/UI/MiniMap.cs` 第 5 行：`using UnityEditor;`
  兩者在檔案開頭引用了 `UnityEditor`，且**完全沒有 `#if UNITY_EDITOR` 條件編譯防護**（兩者皆為未使用的 using）。
* **影響**：在 Editor 環境中因 `autoReferenced` 尚可編譯；但驗收案例要求產出「Windows 執行版建置」，在執行 Player Standalone Build 時，編譯 `Yu5h1Lib.CombatAesthetic` 一定會因找不到 `UnityEditor` 命名空間而中斷建置（錯誤 `CS0246`）。

---

### 缺口 4：未宣告之第三方與可選命名空間洩漏
* **現狀**：
  * `Editor/RecordsWindow.cs` 第 8 行存在 `using DG.DOTweenEditor.UI;`，但套件未依賴 DOTween。若乾淨宿主未安裝 DOTween，打開該 Editor 視窗或編譯 Editor asmdef 時會報錯。
  * `Runtime/Event/TriggerEvent2D.cs`、`Runtime/Physics/FragmentsHandler2D.cs`、`Editor/AttributeValuePropertyDrawer.cs` 皆有 `using Unity.VisualScripting;`，但套件未宣告 Visual Scripting 依賴。
* **影響**：在無 DOTween 或最小安裝的宿主環境存在編譯隱患。

---

### 缺口 5：3D URP 宿主仍須承載 2D 依賴包袱
* **現狀**：雖然 WhiteRoom 是 3D PvE 白色測試場，但因為目前 Sample 位於 `com.yu5h1.combataesthetic` 套件內部，且該套件根目錄的 `Yu5h1Lib.CombatAesthetic.asmdef` 仍包含大量 2D 程式與 2D 組件參考（`com.unity.feature.2d`, `com.yu5h1.2d`）。
* **影響**：乾淨的 3D URP 宿主不能只安裝 3D 與 URP 套件，**必須一併安裝 2D Feature 及 `com.yu5h1.2d`**，否則整包套件無法通過編譯。

---

## 6. 建議的乾淨 3D URP 宿主最小 `Packages/manifest.json` 配置

當未來建立專用的 Unity `6000.3.9f1` 3D URP 宿主時，宿主的 `Packages/manifest.json` 必須包含下列依賴才能確保 `CombatAesthetic` 套件正常載入與編譯：

```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "17.3.0",
    "com.unity.inputsystem": "1.18.0",
    "com.unity.ugui": "2.0.0",
    "com.unity.feature.2d": "2.0.2",
    "com.unity.splines": "2.8.4",
    "com.unity.mathematics": "1.3.2",
    "com.unity.modules.animation": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unity.modules.ui": "1.0.0",

    "com.yu5h1.library": "file:C:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/Core",
    "com.yu5h1.common": "file:C:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/common",
    "com.yu5h1.2d": "file:C:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/2D",
    "com.yu5h1.ui": "file:C:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/UI",
    "com.yu5h1.animation": "file:C:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/UnityExtension/Packages/Animation",
    "com.yu5h1.combataesthetic": "file:C:/Users/Yu5h1/Dev/VSProjects/Yu5h1Lib/Unity/Combat-Aesthetics"
  }
}
```

---

## 7. 未驗證項目清單（保持客觀與嚴謹）

1. **Unity 實際編譯結果未驗證**：依指引「不啟動 Unity、不安裝依賴、不建立宿主」，本報告所有推導均來自靜態組件定義與程式碼比對，尚未在實際 Unity 6000.3.9f1 實例中執行 batchmode 編譯。
2. **Unity Package Manager 自動依賴鏈行為未驗證**：在宣告 `file:` 本地套件路徑下，UPM 是否能完全遞迴解析多層 local package 之間的 `package.json` 依賴，仍待在宿主專案中實測確認。
3. **其餘腳本是否包含潛在反射或未宣告型別未驗證**：除上述 `using` 陳述外，尚未對所有程式碼進行 IL 反射掃描以確認有無執行階段反射載入的不可見相依性。
