# 多人動作遊戲網路互動技術討論

## 文件定位

本文保存多人動作／格鬥遊戲網路架構的討論結論，作為未來評估用的技術筆記。目前尚未決定 CombatAesthetic 的正式網路方案，也不代表相關功能已經實作。

## 核心結論

Unity 是遊戲引擎，不要求專案必須使用 Unity Gaming Services。Unity Client 可以搭配 Unity Relay、AWS Dedicated Server、自架 Server 或其他網路服務。

需要分開理解三個層次：

1. **遊戲 Client 與模擬**：Unity 負責畫面、輸入、角色、動畫、物理和本地遊戲邏輯。
2. **網路同步技術**：處理封包、Frame、輸入同步、預測、狀態校正與 Rollback。
3. **網路基礎設施**：負責 Lobby、Matchmaking、Relay、Dedicated Server、部署與擴縮。

這三層可以自由組合，不需要綁定同一家供應商。

## Cloudflare Tunnel 能驗證的範圍

Cloudflare Tunnel 支援 HTTP、HTTPS 與 WebSocket，因此可用於早期外網原型：

- 建立與加入房間。
- 房間碼、Ready、選角與場景切換通知。
- 聊天、戰績與一般雙向訊息。
- 透過 SignalR／WebSocket 傳送低頻或中頻互動資料。
- 驗證不同網路的 Unity Client 能否連到本機測試 Server。

它也能做簡單的雙向即時遊戲原型，但不等於專為動作遊戲設計的 UDP Relay。Cloudflare Quick Tunnel 主要適合 HTTP／WebSocket 開發測試，不能直接把一般 Unity UDP Port 轉成公開遊戲服務。

因此 Tunnel 適合驗證房間流程與初期互動，不應單憑 Tunnel 測試成功，就判定正式多人格鬥網路架構已完成。

## SignalR／WebSocket 的適用範圍

SignalR 會優先使用 WebSocket，適合需要可靠、有序傳送的訊息：

- 登入、房間與玩家狀態。
- 選角與 Ready。
- 聊天與通知。
- 對戰開始／結束。
- 低頻合作或回合制互動。

WebSocket 基於 TCP。封包遺失時，TCP 會等待重傳並維持順序；對一般互動很可靠，但高速動作遊戲可能因此產生短暫停頓（head-of-line blocking）。

Remote Pointer 每秒約傳送 20 次小型座標訊息，短暫延遲可以透過顯示平滑吸收，因此 Cloudflare Tunnel 與 SignalR 已足以符合它目前的一對一用途。格鬥遊戲通常以 60 Frame 為基準，對輸入延遲、抖動與封包處理更敏感，不能直接套用相同判斷。

## UDP、Relay 與 Rollback

### UDP

UDP 是免費的網路協定，適合傳送高頻、時效性高的遊戲輸入。它不保證封包抵達或順序，因此遊戲需要自行處理序號、遺失、重複、插值與狀態校正。

玩家直接 P2P 使用 UDP 可以不付雲端費用，但會遇到 NAT、防火牆、Port Forwarding、動態 IP 與玩家 IP 暴露等問題。

### Relay

Relay 提供所有玩家都能連線的公開節點，替玩家轉送 UDP／DTLS／WebSocket 流量，解決 NAT、防火牆與 IP 隱私問題。Relay 需要伺服器與頻寬，通常提供免費額度，超過後按使用量收費。

Unity Relay 適合 Client-hosted／Listen Server 模式，其中一位玩家仍是 Host，Relay 主要負責安全轉送。它不是完整的權威 Dedicated Server。

### Rollback

Rollback 是同步技術，不是必然收費的服務。它通常包含：

- 以 Frame 編號傳送玩家輸入。
- 預測尚未收到的遠端輸入。
- 保存近期遊戲狀態快照。
- 遠端輸入抵達且與預測不同時，回到舊 Frame 重新模擬。

Rollback 可以自行實作或使用開源方案，主要成本是工程複雜度。遊戲模擬需要盡可能可重現，亂數、物理、動畫事件與副作用都必須能安全重播。

## Relay 與 Dedicated Server 的差異

### Client-hosted + Relay

```text
Player A (Host)
    ↕
Relay
    ↕
Player B / C / D
```

優點：

- 原型速度快。
- 不需要維護 Dedicated Server Build。
- 小型測試通常可落在免費額度內。

限制：

- Host 仍可能有優勢。
- Host 離線需要處理遷移或結束對戰。
- 權威判定與防作弊能力較弱。

### Authoritative Dedicated Server

```text
Player A ─┐
Player B ─┼→ Authoritative Game Server
Player C ─┤
Player D ─┘
```

優點：

- 所有玩家接受同一個權威判定。
- 較容易處理多人競技、公平性與防作弊。
- 玩家離線不必直接終止整個 Server Process。
- 適合多人排名、3v3 或更大型競技模式。

代價：

- 需要部署、監控、區域選擇與容量管理。
- 需要持續支付主機和頻寬成本。
- 仍需自行完成遊戲同步、戰鬥判定與延遲處理。

AWS GameLift、EC2 或容器平台可以承載 Dedicated Server，但 AWS 不會自動替遊戲完成角色同步或 Rollback。

## Smash Legends 案例的可確認範圍

公開資料可以確認 Smash Legends 的遊戲 Server 營運涉及 AWS，官方曾公告 AWS data center 問題造成遊戲 Server 不穩定，其隱私政策也將 AWS 列為雲端 Server 營運供應商。

目前沒有足夠的一手技術資料確認它是否同時使用 Unity Lobby、Relay、Matchmaker，或具體使用哪一項 AWS 遊戲服務。因此不能只根據「使用 AWS」推論它一定使用 GameLift，也不能推論它完全沒有使用 Unity 的任何服務。

## Unity Multiplayer Services 的改版風險

Unity 已將原本分離的 Lobby、Relay 與 Matchmaker standalone SDK 整合到 `com.unity.services.multiplayer`。這證明服務仍可使用，但 API 與 Package 邊界可能改變，舊教學也可能過時。

若專案重視長期維護，遊戲邏輯不應直接散布供應商 API。可以先定義自己的邊界，例如：

```csharp
public interface IMatchmakingService
{
    Task<string> CreateMatchAsync();
    Task JoinMatchAsync(string matchId);
}

public interface IGameTransport
{
    void SendInput(InputFrame input);
}
```

再提供 Unity Services、AWS、本機測試等不同 Adapter。這是降低服務改版影響的方向，不代表目前已決定要建立完整抽象框架。

## 階段性建議

### 房間與外網概念驗證

- 使用 ASP.NET Core／SignalR 或其他 WebSocket Server。
- 透過 Cloudflare Quick Tunnel 驗證外網開房、加入與雙向訊息。
- 量測 Ping、抖動、斷線與重連，而不是只確認「連得上」。

### 小型可玩原型

- 可先使用 Unity Multiplayer Services 的 Lobby + Relay。
- 對戰先採簡單 Input Delay，再逐步評估 Rollback。
- 將服務 API 隔離在少數 Adapter 中。

### 正式多人競技

- 評估 UDP／DTLS、權威 Dedicated Server 與區域部署。
- 評估 Unity Relay、AWS GameLift／EC2 或其他供應商的成本與維運能力。
- 將同步模型、反作弊與戰鬥權威設計視為核心遊戲工程，而不是雲端服務會自動解決的問題。

## 參考資料

- [Cloudflare Tunnel published application protocols](https://developers.cloudflare.com/cloudflare-one/networks/connectors/cloudflare-tunnel/routing-to-tunnel/protocols/)
- [Unity Relay networking protocols](https://docs.unity.com/en-us/relay/networking)
- [Unity Multiplayer Services SDK migration](https://docs.unity.com/en-us/mps-sdk/migration-path)
- [Unity Multiplayer Services subservices](https://docs.unity.com/en-us/mps-sdk/subservices-toc)
- [Amazon GameLift Servers overview](https://docs.aws.amazon.com/gameliftservers/latest/developerguide/gamelift-intro.html)
- [Smash Legends AWS data center incident notice](https://smashlegends.com/nov-5th-sat-server-instability-due-to-aws-data-center/)

