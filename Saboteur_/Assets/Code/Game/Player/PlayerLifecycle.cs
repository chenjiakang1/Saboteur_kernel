using Mirror;
using UnityEngine;
using System.Collections.Generic;

public partial class PlayerController
{
    private static int globalPlayerIndex = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[客户端] OnStartClient 被调用，netId = {netId}");
        Debug.Log($"📡 [客户端] Player turnIndex={turnIndex}, isMyTurn={isMyTurn}");

        if (isOwned)
        {
            LocalInstance = this;
            Debug.Log("✅ [客户端] 设置 LocalInstance");
            hand.Callback += OnHandChanged;

            if (PlayerController.isGameplayEnabled)
            {
                Debug.Log("🔁 游戏阶段 → 尝试刷新 UI");
                GameManager.Instance?.playerHandManager?.ShowHand(hand);
            }
        }

        Invoke(nameof(GenerateUIWithDelay), 1.0f);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"🎮 OnStartLocalPlayer 被调用，本地玩家 netId = {netId}");

        if (this.isOwned && isClient)
        {
            Debug.Log("🟢 有权限，准备请求房间数据");
            Invoke(nameof(SafeRequestRoomInfo), 0.3f);
        }
    }

    void SafeRequestRoomInfo()
    {
        CmdRequestRoomInfo();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log("[客户端] OnStartAuthority 被调用");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"🛠️ [服务端] OnStartServer 被调用 → netId = {netId}");

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = $"Player{globalPlayerIndex++}";
        }

        if (!PlayerController.isGameplayEnabled)
        {
            SendRoomInfoToAllClients();
        }
        else
        {
            Debug.Log("🃏 游戏阶段 → 服务端初始化玩家数据并发牌");
            ServerInitPlayer(); // ✅ 正确调用服务端发牌逻辑
        }

        TurnManager.Instance?.RegisterPlayer(this);
    }

    [Server]
    void ServerInitPlayer()
    {
        if (!PlayerController.isGameplayEnabled) return;

        Debug.Log($"🛠️ [服务端] 初始化玩家 {playerName}");
        gold = 0;
        isMyTurn = false;
        hasPickaxe = hasLamp = hasMineCart = true;

        hand.Clear();
        for (int i = 0; i < 5; i++)
        {
            var card = GameManager.Instance.cardDeckManager.DrawCard();
            if (card != null)
            {
                hand.Add(new CardData(card));
                Debug.Log($"🎴 发牌：{card.cardName}");
            }
            else
            {
                Debug.LogWarning("❌ 发牌失败：卡组为空");
            }
        }

        TargetRefreshHandUI(connectionToClient);
    }

    [Command]
    public void CmdInit(string name)
    {
        playerName = name;
        ServerInitPlayer(); // ✅ 客户端调用此接口时，服务端也能初始化
    }

    [Server]
    void SendRoomInfoToAllClients()
    {
        List<string> ids = new();
        List<bool> readyList = new();

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            ids.Add(p.playerName);
            readyList.Add(p.isReady);
        }

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                var player = conn.identity.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TargetInitRoomUI(conn, ids.ToArray(), readyList.ToArray());
                    Debug.Log($"📤 向客户端 [{player.playerName}] 发送房间 UI 状态");
                }
            }
        }
    }

    [Command]
    public void CmdRequestRoomInfo()
    {
        Debug.Log($"[服务端] CmdRequestRoomInfo 被调用");

        List<string> ids = new();
        List<bool> readyList = new();

        foreach (var p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            ids.Add(p.playerName);
            readyList.Add(p.isReady);
        }

        TargetInitRoomUI(connectionToClient, ids.ToArray(), readyList.ToArray());
    }

    [TargetRpc]
    public void TargetInitRoomUI(NetworkConnection target, string[] names, bool[] readies)
    {
        Debug.Log($"[客户端] TargetInitRoomUI 收到 {names.Length} 位玩家信息");

        var roomUI = FindFirstObjectByType<RoomUIManager>();
        if (roomUI != null)
        {
            roomUI.RebuildPlayerUI(names, readies);
        }
    }

    [ClientRpc]
    void RpcNotifyAllClientsToRefreshRoomUI()
    {
        Debug.Log($"🔄 [客户端] 通知刷新房间 UI");
        Invoke(nameof(DelayedRefreshUI), 0.2f);
    }

    void DelayedRefreshUI()
    {
        var roomUI = FindFirstObjectByType<RoomUIManager>();
        if (roomUI != null)
        {
            roomUI.RefreshAllPlayerStatus();
        }
    }

    private void GenerateUIWithDelay()
    {
        if (!PlayerController.isGameplayEnabled) return;

        if (GameManager.Instance?.playerUIManager != null)
        {
            Debug.Log("[客户端] 延迟刷新玩家 UI");
            GameManager.Instance.playerUIManager.GenerateUI();
        }

        if (this == PlayerController.LocalInstance)
        {
            Debug.Log("🃏 客户端 → 延迟刷新手牌 UI");
            GameManager.Instance?.playerHandManager?.ShowHand(hand);
        }
    }


    private void OnHandChanged(SyncList<CardData>.Operation op, int index, CardData oldItem, CardData newItem)
    {
        if (!PlayerController.isGameplayEnabled) return;
        if (this != LocalInstance) return;

        Debug.Log($"🃏 手牌变更 ({op}) → hand.Count = {hand.Count}");
        GameManager.Instance?.playerHandManager?.ShowHand(hand);
    }

    [TargetRpc]
    public void TargetRefreshHandUI(NetworkConnection target)
    {
        Debug.Log("🎯 TargetRpc：刷新本地手牌 UI");

        if (this != PlayerController.LocalInstance)
        {
            Debug.Log("⚠️ 非本地玩家，跳过刷新");
            return;
        }

        GameManager.Instance?.playerHandManager?.ShowHand(hand);
    }
}
