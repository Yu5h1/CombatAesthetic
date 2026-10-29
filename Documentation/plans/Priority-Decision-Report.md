# CombatAesthetic 優先任務與決策清單

## 要完成什麼

這個階段要完成的不是一次性重構整個 CombatAesthetic，而是先建立一條能被驗證的主線：

```text
CombatAesthetic
  作為動作對戰美學整合與驗證專案
  驗證哪些能力應該留在 CombatAesthetic
  驗證哪些能力應該收斂到 Yu5h1Lib / UnityExtension packages
```

初版成功標準是：

```text
先整理出最小共用技術來源，並用現有兩個專案驗證它不破壞既有操作。
```

對 UI ViewNavigator/View 這條線來說，成功結果是：

```text
CombatAesthetic 的 UI_Manager / UI_Menu
Virtual-Chat-Partner 的 MainMenu / Page

都能改用同一套 ViewNavigator / View 後，原本操作行為不退化。
```

這表示初版不是追求命名漂亮，而是要能承接已經被實際專案使用過的行為。

## 初版要做到什麼程度

初版只做到「可替代、可驗證、可繼續長期維護」。

必須做到：

- `View` 能表達一個可被切換到的畫面單位。
- `ViewNavigator` 能管理目前畫面並執行切換。
- `ViewNavigator` 能處理 Back / Esc / mobile return 類行為。
- 現有 `Page.parentPage` 與 `UI_Menu.previous` 的返回行為能被新模型覆蓋。
- 現有 `MainMenu` / `UI_Manager` 的 back handler pipeline 能被新模型覆蓋。
- 可以規劃 Unity serialized field / GUID 遷移，不讓 prefab 資料無控制地斷裂。

初版暫時不追求：

- 完整 package 拆分。
- network / rollback。
- ECS 或 DI container。
- 完整 UI framework。
- Previous / Next route 導覽。
- 將 CombatAesthetic 所有 Manager 一次改完。

## 系統關係

```text
Yu5h1Lib / UnityExtension
  common
    UX
      INavigable
      INavigator
      View
      ViewNavigator

CombatAesthetic
  UI_Manager / UI_Menu
    驗證 ViewNavigator / View 是否能覆蓋選單切換、返回、popup

Virtual-Chat-Partner
  MainMenu / Page
    驗證 ViewNavigator / View 是否能覆蓋頁面切換、parent return、Esc 行為
```

目前的方向是：通用 ViewNavigator/View 屬於 UnityExtension，不屬於 CombatAesthetic。CombatAesthetic 是驗證案例之一。

## 已決定

### View 的定義與 ViewNavigator 命名（使用者 2026-10-29）

- `View` 是導覽目的地（畫面）：可被抵達、離開與返回的單位，不等於 panel。panel 是視覺容器；常駐 HUD 是 panel 卻不是 View，沒有選單的遊戲畫面可以是 View 卻不是 panel。GameObject active 與 sibling 順序只是 Unity 的呈現方式，不屬於 View 的定義。
- 管理目前 View 並負責切換、返回的元件命名為 `ViewNavigator`（原 `Viewer`）。理由：Yu5h1Lib.UX 的 [Viewer](../../../../UX/Documentation/ModuleSpec/Viewer.md) 已定義為「承載內容的觀看區，負責視角」，同名會讓含觀看區的畫面無法分辨兩者；新名稱也對齊它實作的 `INavigator<View>`。
- 已完成改名：`UnityExtension/Packages/common/Runtime/Component/UX/ViewNavigator.cs`，沿用原 `.meta` 的 script GUID。

### View 的返回欄位：`backTarget`（使用者 2026-10-29）

- `backTarget` 是作者指定的「從這個 View 執行 Back 要去哪裡」，對應 `ViewNavigator.Back()` 的第二步。它不表達層級，也不表達時間歷史。
- 不用 `parent`：CombatAesthetic 的 `playerMenu` 與 `LevelSceneMenu` 互為返回目標（`UI_Manager.cs` 161–162 行），沒有層級可言。不用 `previous`：`UI_Menu.previous` 只在第一次切換時自動寫入（`UI_Manager.cs` 250 行），之後就固定，名稱與行為不符。不用 `returnTarget`：返回的唯一入口是 `Back()`，`Return()` 已移除。
- View 不自動記錄上一個 View。如果需要按時間順序的歷史，那是 ViewNavigator 的切換策略，不寫進 `backTarget`；這與 Android 把 Up（宣告的父層）和 Back（系統持有的返回堆疊）分開的做法一致。遷移 CombatAesthetic 時，原本自動寫入的 `previous` 要改成明確指定的 `backTarget`。
- 尚未實作：`View.cs` 還沒有這個欄位。

### Back 的位置、註冊方式與執行順序（依現況採用，2026-10-29）

以下三項沒有另外出題：現有實作或平台原生行為已經給出依據。

- **`Back()` 留在 ViewNavigator，不進 `INavigator`**：現有 `ViewNavigator.cs` 已是如此。`INavigator<T>` 是 DotNet 層的通用介面，只有 `Current`、`CanNavigate`、`MoveTo`；Back 依賴 back handler、`backTarget` 與 base view，屬於 UI 導覽策略，不是每個 navigator 都有。
- **back handler 以 C# event 註冊，型別名為 `BackHandler`**：現有 `ViewNavigator.backHandlers` 是 event，自訂 add 會略過重複的 delegate；VCP `MainMenu.cancelActions` 也是 event。兩個舊專案都沒有在 Inspector 序列化的 handler，所以不需要 UnityEvent 版本。CombatAesthetic 的 `AddCancelAction(Type, …)` 以型別為鍵移除，遷移時改成保存 delegate 再 `-=`。
- **執行順序採 LIFO（最後加入的先處理）**：兩個舊專案都是新增順序，現有 ViewNavigator 已是 LIFO，彼此衝突。依據平台原生行為採 LIFO：Android 的 `OnBackPressedDispatcher` 由最後加入的 callback 先處理，也符合「最後開啟的先關閉」。遷移驗證時要確認同時有效的 handler 不因順序反轉而改變結果：CombatAesthetic 的 `UI_DialogBase`、`StoryHandler`、`CameraController`，以及 VCP 的 `GuideManager.TrySkip`、`GetLetterRequestData.SkipLetter`。
- **移除 `Return()` 別名，只留 `Back()`**（使用者 2026-10-29）：兩個名稱做同一件事只多一層間接；VCP 按鈕綁的是舊 `Page.Return`，換成 View／ViewNavigator 後綁定目標本來就不同，別名無助於遷移。已從 `ViewNavigator.cs` 移除。

### Switch 與 Popup 都是初版必要功能（依成功標準採用，2026-10-29）

- 成功標準要求 CombatAesthetic 的選單行為不退化，而 KFC 的 prefab 以 UnityEvent 綁定 `UI_Menu.Popup`：`StartSceneMenu` 4 處、`LevelSceneMenu`（開 `SaveMenu`）、`PlayerAttribute_UI`（開 `MapDetail`）、`5秒後出現選字` 各 1 處。因此 Popup 必須在初版。
- Switch（關閉目前再開下一個）即現有 `MoveTo` 搭配 `hidePrevious`，`ReturnToPrevious` 也走這條路，已具備。
- 舊 Popup 的實際行為（`UI_Manager.ChangeMenu`／`Engage`）：不關閉目前 menu，但把 `currentMenu` 改成 popup；popup 首次開啟時自動把底下的 menu 記成 `previous`，Back 時關閉 popup 並回到它。

### Popup 成為 Current，底層由 ViewNavigator 記住（使用者 2026-10-29）

- Popup 開啟時不關閉底下的 View，但成為 `Current`，與舊 `UI_Manager.Engage` 一致；Back 與 handler 都作用在最上層。
- popup 疊在哪個 View 上由 ViewNavigator 記住，不寫進 View：同一個 popup 可從不同 View 開啟（KFC 的 `SaveMenu` 由 `StartSceneMenu` 與 `LevelSceneMenu` 開啟），作者指定的 `backTarget` 無法表達；這屬於按時間順序的歷史，依〈View 的返回欄位〉歸 ViewNavigator。
- popup 的返回走同一個 Back 流程，`ViewNavigator.Back()` 改為：

```text
ViewNavigator.Back()
  1. back handlers（LIFO）
  2. 目前是 popup：關閉它，回到它疊在上面的 View
  3. 目前 View 的 backTarget
  4. base view
  5. 都沒有就不處理
```

- 舊 `previous` 只在 popup 首次開啟時寫入；若 UI 物件跨場景保留，之後從另一個 menu 開同一個 popup，Back 會回到第一次的 menu。新模型不會有這個問題。這是讀程式碼的推論，未在執行中確認。
- 尚未實作：現有 `ViewNavigator.cs` 沒有 popup 與底層紀錄，`Back()` 只有第 1、4 步。

### 依現況採用的 API 與事件（2026-10-29）

以下各項沒有另外出題，依據寫在各項。

- **Popup 是獨立方法 `Popup(View)`，不是 `MoveTo` 的參數**：KFC 的 7 處 Popup 是 UnityEvent 在 Inspector 綁定，UnityEvent 只能呼叫最多一個參數的方法；舊 `UI_Menu` 也是分成 `SwitchMenu`／`Popup`。
- **沒有返回目標時**：依〈Popup 成為 Current〉的 Back 流程，退到 base view；目前就是 base view 時不處理。
- **View 只保留 `Arrived`／`Left`，不放 `focusedChanged`**（決策 8）：KFC 所有 `UI_Menu.focusedChanged` 的綁定都是空的；舊 `shown`／`hide` 只有 2 處綁定（`LevelSceneMenu`→`LoadMapImageFromActivateScene`、`SaveMenu`→`ClearSelection`），由 `Arrived`／`Left` 承接。現有 `View.cs` 已同時有 UnityEvent 與 C# event。ViewNavigator 的 current changed 事件沒有現有使用者，初版不加。
- **`CanNavigate` 維持在 `INavigator`**（決策 9）：現有介面已是如此，判斷能否前往是任何 navigator 都需要的通用能力。
- **淘汰 `DisallowPreviouse`**（決策 14）：它的用途是阻止自動記錄 `previous`；新模型的 View 不自動記錄任何東西，這個欄位沒有作用。KFC 只有 `StartSceneMenu` 設為 true。

### base view 不被隱藏，淘汰 `DisallowDismiss`（使用者 2026-10-29）

- ViewNavigator 以 `MoveTo` 或 `Popup` 離開 base view 時不隱藏它，base view 留作背景；Back 停在 base view 時不處理。
- 依據：KFC 只有 `StartSceneMenu` 設 `DisallowDismiss`，效果正是讓每個場景的根選單在切換時留在底下、Back 也關不掉，與 base view 的角色相同。
- 尚未實作：現有 `ViewNavigator.MoveTo` 在 `hidePrevious` 為 true 時一律隱藏上一個 View，包括 base view。

### ViewNavigator 的 `locked`，初版不做 `CanLeave`／`CanHide`（使用者 2026-10-29）

- ViewNavigator 加 bool 屬性 `locked`；為 true 時 `MoveTo`、`Popup`、`Back` 都不動作。鎖是導覽的狀態，放在 ViewNavigator，不放在 View。
- 依據：VCP 的 `AllowChangePageOrReturn` 在場景中以 UnityEvent 設定（ChatScene 7 處、vcp_AudioClient 3 處，可辨認的來源是 `ChatRequest` 的 callback），請求進行中鎖住換頁與返回。單一 bool 屬性才能讓 UnityEvent 直接設定，承接這些綁定。
- 初版不做 `CanLeave`／`CanHide`：兩個舊專案都沒有「由 View 自己決定能否離開」的使用者；唯一相近的 `DisallowDismiss` 已由 base view 取代。
- 已知限制：兩個請求同時進行時，先結束的會提早解鎖，與舊行為相同；初版不改成計數。
- 尚未實作。

### `visibles` 進初版 View（使用者 2026-10-29）

- View 持有 `visibles`（此畫面另外包含的物件）與 `AddVisible`／`RemoveVisible`。ViewNavigator 切換時比對新舊兩組：兩邊都有的保持開啟，只在舊 View 的關閉，新 View 的全部開啟。Popup 不隱藏底層，底層的 `visibles` 也保持開啟。
- 依據：VCP 三個場景的 Page（ChatScene 14、vcp_AudioClient 14、stereoscopic 3D 12）全部設有 `visibles`，同一物件常被多個 Page 共用，成功標準要求不退化。放在 View 符合〈View 的定義〉：畫面可以由自己的 panel 加上共用物件組成。做成獨立元件仍需 ViewNavigator 比對新舊兩組，只多一層間接。
- 尚未實作。

### 共用元件位置與 DotNet 邊界（依現況採用，2026-10-29）

- **正式位置是 `UnityExtension/Packages/common/Runtime/Component/UX`**（決策 17）：`View.cs`、`ViewNavigator.cs` 已在此，屬 `Yu5h1Lib.Common` assembly。
- **DotNet core 只保留 `INavigable`／`INavigator`**（決策 18）：`DotNet/Source/UX` 目前只有這兩個介面。View 的顯示、`visibles`、base view 與 `locked` 都依賴 GameObject 或 UI 導覽策略，屬 Unity 實作端，不進平台中立的 DotNet 層。

### 遷移保留舊 class，不改 script GUID（使用者 2026-10-29）

- **`Page : View`**：script GUID 不變，VCP 的 `Page` 型別參照（9 處）與 ChatScene 約 30 個綁在 `Page`／`MainMenu` 的 UnityEvent 不斷。舊方法（`ChangePage`、`Return` 等）暫留在 `Page`，只轉給 ViewNavigator。
- **舊欄位值由 VCP 端一次性遷移**：`parentPage`→`backTarget`、`loaded`→`Arrived`、`unlooaded`→`Left`；`visibles` 同名沿用。共用 `View` 不放 `[FormerlySerializedAs]` 等專案舊名。
- **`MainMenu` 保留並持有 `ViewNavigator`**：它另有相機、使用者資料、虛擬鍵盤、XR 等責任（411 行），不能整個換成 ViewNavigator。back handler 改接 `backHandlers`，`AllowChangePageOrReturn` 改接 `locked`。
- 轉接方法在驗證不退化後，另以項目把 UnityEvent 改綁到 View／ViewNavigator 再移除，不在首次遷移內。
- CombatAesthetic 第二輪依同一原則：`UI_Menu : View`，`UI_Manager` 持有 ViewNavigator。

### VCP 先做遷移驗證（使用者 2026-10-29）

- 首次遷移驗證採 Virtual-Chat-Partner 的 `MainMenu`／`Page` 流程。它涵蓋 `backTarget`、`locked`、`visibles` 與 LIFO back handler，且沒有 Popup，範圍較小；也避開由使用者另行處理的 KFC。
- VCP 目前有其他任務在改 `MainMenu.cs`、`ChatRequest.cs`，遷移項目以 `waits_for` 等那些項目完成後才開始。CombatAesthetic（KFC）為第二輪。

## 優先任務

### 0. 確認遊戲核心的物理美學

目的：先定義 CombatAesthetic 的戰鬥核心不能破壞世界物理感。遊戲可以誇張、可以高速、可以抽象，但不能把世界規則做成任意瞬間定位或完全複製。

目前決定方向：

- 不能完全克隆一個角色或物件，因為現實中即使看似一樣，材質、來源、狀態也不完全相同。
- 不能瞬間移動到遠距離位置，因為物體可以非常快，但不應無過程地跨越距離。
- 戰鬥核心要保留「來源、過程、材質、慣性、距離」這類能形成物理美感的概念。
- 視覺表現可以省略細節，但核心規則不應假設物件能無成本地複製或瞬移。

需要討論的問題：

- 玩家如何出生？
- 出生是從世界外進入、由場地生成、由既有物質組成，還是某種召喚/投放？
- 出生過程是否需要時間、位置限制、保護期、可被干擾、或有視覺/物理跡象？
- 如果多人線上同步，出生由 server 決定，還是由房間規則決定？

這個決策會影響角色生成、重生、傳送、召喚、分身、投射物、網路同步與防作弊，所以先記錄，不急著實作。

### 1. 收斂 ViewNavigator / View 的真實需求

目的：先讓共用 UI 導覽元件能取代兩套已用過的專案實作。

要分析的舊行為：

- `UI_Manager.currentMenu`
- `UI_Manager.ChangeMenu`
- `UI_Manager.OnCancelPressed`
- `UI_Menu.previous`
- `UI_Menu.ReturnToPrevious`
- `Page.currentPage`
- `Page.parentPage`
- `Page.Return`
- `Page.ChangePage`
- `MainMenu.HandleBackAction`
- `MainMenu.cancelActions`

初步保留方向：

- `View` 是可被導覽抵達的畫面單位。
- `ViewNavigator` 是持有 current 並負責切換的管理者。
- Back 行為應優先由 handler pipeline 處理，再回到目前 View 的返回目標，再 fallback。
- `previous` / `parent` 類資料有價值，但最終名稱和位置需要由使用案例驗證。

### 2. 決定 Back 模型

目的：把 Escape / mobile return / cancel 行為整理成長期可維護的策略。

已知共同點：

- CombatAesthetic 用 `cancelActions` 優先處理特殊取消，例如 dialog skip、camera stop。
- Virtual-Chat-Partner 用 `_cancelActions`、關閉指定物件、清 focus、Page return。

已決定的流程見〈Popup 成為 Current，底層由 ViewNavigator 記住〉。

已決定：handler 名稱、註冊方式、執行順序，見 [Back 的位置、註冊方式與執行順序](#back-的位置註冊方式與執行順序依現況採用2026-10-29)。

已決定：不保留 `Return()`，見 [Back 的位置、註冊方式與執行順序](#back-的位置註冊方式與執行順序依現況採用2026-10-29)。

### 3. 決定 View 的返回資料

目的：保留 `Page.parentPage` 和 `UI_Menu.previous` 的有效經驗，但避免照抄錯誤命名。

候選名稱：

- `previous`
- `parent`
- `returnTarget`
- `backTarget`

目前觀察：

- `Page.parentPage` 比較像層級返回。
- `UI_Menu.previous` 比較像切換流程中的上一個 menu。
- 兩者都不是 route，也不必導出 `Previous/Next`。

已決定：欄位名稱、是否自動記錄，見 [View 的返回欄位](#view-的返回欄位backtarget使用者-2026-10-29)。

已決定：沒有返回目標時退到 base view，見〈依現況採用的 API 與事件〉。

### 4. 決定 Switch / Popup 模型

目的：保留 `UI_Manager.ChangeMenu(current, next, dismiss)` 中真正有價值的差異。

舊行為：

```text
SwitchMenu = 切換頁面，關閉目前
Popup      = 疊出新頁面，不關目前
```

已決定：Switch／Popup 都必要；Popup 會改變 `Current`、關閉後回到底層、走同一個 Back 流程，見〈Popup 成為 Current，底層由 ViewNavigator 記住〉；API 分成 `MoveTo(View)`／`Popup(View)`，見〈依現況採用的 API 與事件〉。


### 5. 決定顯示生命週期事件

目的：保留舊 `loaded/unlooaded`、`shown/hide` 的實際用途，但避免過度事件化。

已經較穩的事件：

- arrived
- left

懷疑項目：

- `focusedChanged` 不應直接放在 View，因為 focused/current 是 ViewNavigator 管理狀態。
- loaded/unloaded、shown/hide 與 arrived/left 是否只是同一件事的不同名稱。

已決定：使用 `Arrived`／`Left`，UnityEvent 與 C# event 並存，初版不加 current changed 事件，見〈依現況採用的 API 與事件〉。

### 6. 決定 shared visible objects 是否進核心

目的：判斷 `Page.visibles` 是否是核心 View 能力。

舊用途：

```text
切換 page 時，某些 GameObject 被多個 page 共用，不應被目前 page 關閉。
```

目前判斷：

- 這個功能有實際價值。
- 但它可能不是最小 `View` 核心，而是額外 component 或可選策略。

已決定：進初版 View，見〈`visibles` 進初版 View〉。

### 7. 決定舊專案序列化遷移策略

目的：避免共用化時讓 prefab 和 scene 的 serialized data 斷裂。

必須決定：

- 新欄位是否刻意沿用舊 field name。
- 是否使用 `[FormerlySerializedAs]`。
- 是否保留舊 class 作為 wrapper / adapter 過渡。
- 是否要改 script GUID，讓舊 component 直接對齊新 component。
- CombatAesthetic 和 Virtual-Chat-Partner 哪一邊先遷移。

建議原則：

- 先完成共用元件。
- 再選一個專案做低風險驗證。
- 最後才處理 GUID / fieldName 對齊。

## 懷疑項目

### `DisallowDismiss`

懷疑原因：

- 它像是 popup / modal 場景的臨時補丁。
- 它把「能不能關閉」寫成 View 的靜態欄位，但原因可能是 loading、資料未儲存、base view、modal 必須完成等不同情境。

可能替代：

- `CanLeave`
- `CanHide`
- back handler
- viewer-level policy

### `DisallowPreviouse`

懷疑原因：

- 它的實際用途是阻止自動設定 previous。
- 這比較像 ViewNavigator 的切換策略，不像 View 的本質。

可能替代：

- `recordPrevious`
- `rememberPrevious`
- move option
- popup/switch mode 的一部分

### `focusedChanged`

懷疑原因：

- focused/current 是 ViewNavigator 狀態。
- View 不應假設自己一定只有一個 ViewNavigator 管理。

可能替代：

- ViewNavigator 的 `CurrentChanged`。
- 由 ViewNavigator 在切換時通知舊 View / 新 View。

### `AllowChangePageOrReturn`

懷疑原因：

- 它是全域鎖，能解問題，但可能會把多個原因混成一個 bool。

可能替代：

- `CanNavigate`
- `CanLeave`
- back handler
- viewer lock / transition state

### `ObjectsOfDeactivateOnCancel`

懷疑原因：

- 功能有用，但名稱與責任偏專案化。
- 它比較像 floating objects 或 overlay close policy，不一定屬於 ViewNavigator 核心。

可能替代：

- back handler 註冊。
- floating view。
- overlay viewer。

## 決策清單

請之後逐項回覆：

0. CombatAesthetic 的遊戲核心是否確定採用「符合物理美學」作為基礎原則？
0.1 玩家出生方式要如何解釋：世界外進入、場地生成、物質組成、召喚/投放，還是其他模型？
0.2 是否明確禁止無過程瞬間移動與完全克隆？
1. ~~初版 ViewNavigator/View 是否只以 `UI_Manager/UI_Menu` 與 `MainMenu/Page` 行為不退化作為成功標準？~~ 已決定：是（使用者 2026-10-29），見〈要完成什麼〉。
2. ~~`View` 的返回欄位要叫 `previous`、`parent`、`returnTarget`，還是其他名稱？~~ 已決定：`backTarget`。
3. ~~`Back()` 是否保留在 `ViewNavigator`，但不進 `INavigator`？~~ 依現況採用：是。
4. ~~back handler 註冊方式要用 event，還是明確方法？~~ 依現況採用：event。
5. ~~back handler 執行順序要用新增順序，還是 LIFO？~~ 依平台原生行為採用：LIFO。
6. ~~`Switch` / `Popup` 是否是初版必要功能？~~ 依成功標準採用：兩者都必要，見〈Switch 與 Popup 都是初版必要功能〉。
7. ~~Popup 是否會改變 `ViewNavigator.Current`？~~ 已決定：會，見〈Popup 成為 Current，底層由 ViewNavigator 記住〉。
8. ~~`View` 是否只保留 `Arrived` / `Left`，不放 `focusedChanged`？~~ 依現況採用：是。
9. ~~`CanNavigate` 是否維持在 `INavigator`？~~ 依現況採用：是。
10. ~~是否需要 `CanLeave` / `CanHide`，或先由 back handler 解決？~~ 已決定：初版不做。
11. ~~`visibles` 是否進初版 `View`，或延後成可選 component？~~ 已決定：進初版 View。
12. ~~`AllowChangePageOrReturn` 的功能是否要轉成 viewer lock / transition state？~~ 已決定：轉成 ViewNavigator 的 `locked`。
13. ~~`DisallowDismiss` 是否淘汰，不進共用核心？~~ 已決定：淘汰，由 base view 不被隱藏取代。
14. ~~`DisallowPreviouse` 是否淘汰，改由 ViewNavigator move option 控制？~~ 依現況採用：淘汰，不需替代。
15. ~~CombatAesthetic 和 Virtual-Chat-Partner 哪一個先做遷移驗證？~~ 已決定：VCP 先。
16. ~~遷移時是否要保留舊 class 名稱作 wrapper，還是直接改 script GUID 對齊新 component？~~ 已決定：保留舊 class（繼承 View／持有 ViewNavigator），不改 GUID。
17. ~~新共用元件正式位置是否確認為 `UnityExtension/Packages/common/Runtime/Component/UX`？~~ 依現況採用：是。
18. ~~DotNet core 是否只保留 `INavigable` / `INavigator`，不放 Unity View 行為？~~ 依現況採用：是。

## 建議驗證順序

### 驗證 1：Virtual-Chat-Partner Page 流程

理由：`Page.parentPage` 與 `MainMenu.HandleBackAction` 行為清楚，適合驗證 Back 基本模型。

驗證項目：

- FirstPage 啟動。
- ChangePage。
- Parent return。
- Escape return。
- focus 存在時先清 focus。
- floating object 先關閉。

### 驗證 2：CombatAesthetic UI_Menu 流程

理由：`SwitchMenu` / `Popup` / `previous` / cancel action 更接近遊戲選單情境。

驗證項目：

- StartSceneMenu 啟動。
- LevelSceneMenu / PlayerAttribute_UI 互相 previous。
- Popup 不關目前 menu。
- Switch 關目前 menu。
- Dialog skip 優先於 menu return。

### 驗證 3：序列化與 prefab 遷移

理由：功能可用不代表可安全替換舊專案。

驗證項目：

- 舊 prefab 欄位能對齊。
- scene reference 不丟失。
- UnityEvent 綁定不無聲失效。
- GUID / fieldName 遷移策略可重複。

## 暫時不處理

- Route / Previous / Next 線性導覽。
- Tree / hierarchy 視覺化。
- Network / rollback。
- CharacterController2D 大型拆分。
- AttributeBehaviour 拆分。
- package move。

這些都重要，但不應該擠進 ViewNavigator/View 初版。ViewNavigator/View 初版先證明一件事：

```text
兩套已存在 UI flow 可以收束到同一個長期維護來源。
```
