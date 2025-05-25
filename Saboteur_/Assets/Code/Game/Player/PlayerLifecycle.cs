using Mirror;
using UnityEngine;

public partial class PlayerController
{
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[本地玩家] 我的名字是：{playerName}，netId = {netId}");

        CmdInit("Player" + netId);
        hand.Callback += OnHandChanged;
        GameManager.Instance.playerHandManager.ShowHand(hand);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        LocalInstance = this;
        Debug.Log("[客户端] 获得 authority 权限");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[客户端] OnStartClient 被调用，netId = {netId}");
        Debug.Log($"📡 [客户端] Player turnIndex={turnIndex}, isMyTurn={isMyTurn}");
        Invoke(nameof(GenerateUIWithDelay), 1.0f);
    }

    // ✅ 新增：服务端执行时注册玩家
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"🛠️ [服务端] OnStartServer 被调用 → 注册玩家 netId = {netId}");
        TurnManager.Instance?.RegisterPlayer(this);
    }

    private void GenerateUIWithDelay()
    {
        if (GameManager.Instance?.playerUIManager != null)
        {
            Debug.Log("[客户端] 延迟调用 → 生成/刷新所有玩家 UI");
            GameManager.Instance.playerUIManager.GenerateUI();
        }
        else
        {
            Debug.LogWarning("⚠️ 无法访问 GameManager 或 UI 管理器，UI 未刷新");
        }
    }

    private void OnHandChanged(SyncList<CardData>.Operation op, int index, CardData oldItem, CardData newItem)
    {
        if (this != LocalInstance) return;
        Debug.Log($"[客户端] 手牌列表变更({op}) → 刷新 UI");
        GameManager.Instance.playerHandManager.ShowHand(hand);
    }

    [Command]
    public void CmdInit(string name)
    {
        Debug.Log("[服务端] 执行 CmdInit: " + name);
        playerName = name;
        gold = 0;
        isMyTurn = false;
        hasPickaxe = hasLamp = hasMineCart = true;

        hand.Clear();
        for (int i = 0; i < 5; i++)
        {
            var card = GameManager.Instance.cardDeckManager.DrawCard();
            if (card != null)
                hand.Add(new CardData(card));
        }
    }
}
