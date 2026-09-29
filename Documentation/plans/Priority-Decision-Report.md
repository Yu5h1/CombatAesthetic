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

對 UI Viewer/View 這條線來說，成功結果是：

```text
CombatAesthetic 的 UI_Manager / UI_Menu
Virtual-Chat-Partner 的 MainMenu / Page

都能改用同一套 Viewer / View 後，原本操作行為不退化。
```

這表示初版不是追求命名漂亮，而是要能承接已經被實際專案使用過的行為。

## 初版要做到什麼程度

初版只做到「可替代、可驗證、可繼續長期維護」。

必須做到：

- `View` 能表達一個可被切換到的畫面單位。
- `Viewer` 能管理目前畫面並執行切換。
- `Viewer` 能處理 Back / Esc / mobile return 類行為。
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
      Viewer

CombatAesthetic
  UI_Manager / UI_Menu
    驗證 Viewer / View 是否能覆蓋選單切換、返回、popup

Virtual-Chat-Partner
  MainMenu / Page
    驗證 Viewer / View 是否能覆蓋頁面切換、parent return、Esc 行為
```

目前的方向是：通用 Viewer/View 屬於 UnityExtension，不屬於 CombatAesthetic。CombatAesthetic 是驗證案例之一。

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

### 1. 收斂 Viewer / View 的真實需求

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
- `Viewer` 是持有 current 並負責切換的管理者。
- Back 行為應優先由 handler pipeline 處理，再回到目前 View 的返回目標，再 fallback。
- `previous` / `parent` 類資料有價值，但最終名稱和位置需要由使用案例驗證。

### 2. 決定 Back 模型

目的：把 Escape / mobile return / cancel 行為整理成長期可維護的策略。

已知共同點：

- CombatAesthetic 用 `cancelActions` 優先處理特殊取消，例如 dialog skip、camera stop。
- Virtual-Chat-Partner 用 `_cancelActions`、關閉指定物件、清 focus、Page return。

初版建議：

```text
Viewer.Back()
  1. 先跑 back handlers
  2. 再嘗試 current 的 return target
  3. 再嘗試 base view
  4. 最後不處理
```

待決策：

- handler 名稱用 `BackHandler` 是否確定。
- 註冊方式要用 event，還是 `AddBackHandler` / `RemoveBackHandler`。
- handler 執行順序要沿用新增順序，還是改成 LIFO。
- `Return()` 是否保留作為 `Back()` alias。

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

待決策：

- 共用 View 要採用哪個欄位名稱。
- 是否允許自動記錄上一個 View。
- 自動記錄上一個 View 是 View 責任，還是 Viewer 切換策略。
- 沒有返回目標時，是 hide current、回 base view，還是不處理。

### 4. 決定 Switch / Popup 模型

目的：保留 `UI_Manager.ChangeMenu(current, next, dismiss)` 中真正有價值的差異。

舊行為：

```text
SwitchMenu = 切換頁面，關閉目前
Popup      = 疊出新頁面，不關目前
```

待決策：

- 共用 API 要用 `MoveTo(view)` + 參數，還是分成 `MoveTo(view)` / `Popup(view)`。
- Popup 是否應該改變 `Current`。
- Popup 關閉後要回到哪個 View。
- popup/floating 類 View 的返回是否應該走同一個 Back pipeline。

### 5. 決定顯示生命週期事件

目的：保留舊 `loaded/unlooaded`、`shown/hide` 的實際用途，但避免過度事件化。

已經較穩的事件：

- arrived
- left

懷疑項目：

- `focusedChanged` 不應直接放在 View，因為 focused/current 是 Viewer 管理狀態。
- loaded/unloaded、shown/hide 與 arrived/left 是否只是同一件事的不同名稱。

待決策：

- 共用事件命名是否使用 `Arrived` / `Left`。
- 是否需要 UnityEvent 與 C# event 同時存在。
- 是否需要 current changed 事件，且它應該在 Viewer 而不是 View。

### 6. 決定 shared visible objects 是否進核心

目的：判斷 `Page.visibles` 是否是核心 View 能力。

舊用途：

```text
切換 page 時，某些 GameObject 被多個 page 共用，不應被目前 page 關閉。
```

目前判斷：

- 這個功能有實際價值。
- 但它可能不是最小 `View` 核心，而是額外 component 或可選策略。

待決策：

- `visibles` 是否進 `View`。
- 是否改成 `ViewGroup` / `ViewVisibility` 之類額外元件。
- 初版是否先不處理，保留到遷移 VCP 時再驗證。

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
- 這比較像 Viewer 的切換策略，不像 View 的本質。

可能替代：

- `recordPrevious`
- `rememberPrevious`
- move option
- popup/switch mode 的一部分

### `focusedChanged`

懷疑原因：

- focused/current 是 Viewer 狀態。
- View 不應假設自己一定只有一個 Viewer 管理。

可能替代：

- Viewer 的 `CurrentChanged`。
- 由 Viewer 在切換時通知舊 View / 新 View。

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
- 它比較像 floating objects 或 overlay close policy，不一定屬於 Viewer 核心。

可能替代：

- back handler 註冊。
- floating view。
- overlay viewer。

## 決策清單

請之後逐項回覆：

0. CombatAesthetic 的遊戲核心是否確定採用「符合物理美學」作為基礎原則？
0.1 玩家出生方式要如何解釋：世界外進入、場地生成、物質組成、召喚/投放，還是其他模型？
0.2 是否明確禁止無過程瞬間移動與完全克隆？
1. 初版 Viewer/View 是否只以 `UI_Manager/UI_Menu` 與 `MainMenu/Page` 行為不退化作為成功標準？
2. `View` 的返回欄位要叫 `previous`、`parent`、`returnTarget`，還是其他名稱？
3. `Back()` 是否保留在 `Viewer`，但不進 `INavigator`？
4. back handler 註冊方式要用 event，還是明確方法？
5. back handler 執行順序要用新增順序，還是 LIFO？
6. `Switch` / `Popup` 是否是初版必要功能？
7. Popup 是否會改變 `Viewer.Current`？
8. `View` 是否只保留 `Arrived` / `Left`，不放 `focusedChanged`？
9. `CanNavigate` 是否維持在 `INavigator`？
10. 是否需要 `CanLeave` / `CanHide`，或先由 back handler 解決？
11. `visibles` 是否進初版 `View`，或延後成可選 component？
12. `AllowChangePageOrReturn` 的功能是否要轉成 viewer lock / transition state？
13. `DisallowDismiss` 是否淘汰，不進共用核心？
14. `DisallowPreviouse` 是否淘汰，改由 Viewer move option 控制？
15. CombatAesthetic 和 Virtual-Chat-Partner 哪一個先做遷移驗證？
16. 遷移時是否要保留舊 class 名稱作 wrapper，還是直接改 script GUID 對齊新 component？
17. 新共用元件正式位置是否確認為 `UnityExtension/Packages/common/Runtime/Component/UX`？
18. DotNet core 是否只保留 `INavigable` / `INavigator`，不放 Unity View 行為？

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

這些都重要，但不應該擠進 Viewer/View 初版。Viewer/View 初版先證明一件事：

```text
兩套已存在 UI flow 可以收束到同一個長期維護來源。
```
