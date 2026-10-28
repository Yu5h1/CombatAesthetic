# Combat-Aesthetics 重構計畫

> Task ID: refactor-plan
>
> 任務與進度：[重構計畫任務卡](http://127.0.0.1:8001/?scope=combat-aesthetic&pin=refactor-plan)。

## 系統階層與關係

```text
Combat-Aesthetics
└─ 重構計畫
   ├─ 文件命名：統一專案名稱與文件引用
   └─ Camera Controller 重構
      ├─ 共用控制：由 UnityExtension 擁有
      └─ 消費端整合與附屬功能拆分：由 Combat-Aesthetics 擁有
```

本計畫集中整理文件命名與攝影機重構兩條工作線。Camera 的設計由共用分析文件維護，具體工作沿用既有任務卡。

## 問題與目標

- 文件仍混用 `Combat Aesthetic`、`CombatAesthetic` 等名稱，統一對人呈現的專案名稱為 `Combat-Aesthetics`。
- 收斂攝影機控制責任，讓共用控制方法、輸入來源及消費端表現各有擁有者。
- 以現有專案與呼叫端作為相容性依據，分開安排文件整理與程式遷移。

## 文件名稱統一為 Combat-Aesthetics

範圍包含本專案 README、計畫、Documentation、Sample 說明，以及 handoff 中用來指稱專案的文字。盤點包含舊專案名稱的文件檔名、標題、說明、路徑與超連結，區分顯示名稱和技術識別碼後再修改。

執行內容：

1. 盤點 `Combat Aesthetic`、`CombatAesthetic`、`Combat Aesthetics` 等寫法，建立需要調整的文件與引用清單。
2. 將專案顯示名稱統一為 `Combat-Aesthetics`；文件檔名含舊專案名稱時，一併整理檔名與所有引用。
3. 保留程式型別、namespace、UPM 套件 ID、TaskProgress scope／任務 ID，以及必要的歷史路徑說明。這些識別碼若要遷移，另訂相容計畫。
4. 確認文件相對連結、章節錨點與路徑仍能定位。外部專案內的引用由其擁有者處理，不直接批次改寫。

驗收條件：文件用來指稱本專案的名稱一致；保留舊名稱的地方有明確用途；沒有因文件更名而產生失效引用。

## Camera Controller 重構

設計依據：[Camera Controller 重構分析](../../../UnityExtension/Documentation/Camera-Controller-Refactor.md)。

工作分工：

- [共用攝影機控制任務卡](http://127.0.0.1:8001/?scope=unity-extension&pin=camera-controller)：CameraController、InputSource、Follow、LookDirection、2D 組合與既有消費端遷移。
- [Combat-Aesthetics 攝影機附屬功能拆分卡](http://127.0.0.1:8001/?scope=combat-aesthetic&pin=camera-controller-extraction)：拆出游標、點擊特效、Sorting Layer 遮罩與淡出，保存既有呼叫與接線。

本項作為這兩張卡的總覽入口；設計與細項以連結目標為準，不在此複製。驗收須同時涵蓋共用控制與 Combat-Aesthetics 的消費端相容性，不能只以新增類別或搬移檔案判定完成。

## 本輪交付範圍

建立計畫、分析文件與任務入口；文件更名、程式重構、場景／Prefab 遷移及執行期驗證由後續工作執行。各項進度以任務卡為準。
