using Mirror;
using UnityEngine;

public partial class PlayerController
{
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // ✅ 设置 LocalInstance，确保客户端能访问本地玩家控制器
        LocalInstance = this;

        Debug.Log($"🟢 [本地玩家] OnStartLocalPlayer 被调用 → 设置 LocalInstance，netId = {netId}");

        // 初始化玩家信息并同步
        CmdInit("Player" + netId);

        // 绑定手牌列表变化事件，刷新手牌 UI
        hand.Callback += OnHandChanged;

        // 显示初始手牌 UI
        GameManager.Instance.playerHandManager.ShowHand(hand);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        Debug.Log("[客户端] OnStartAuthority 被调用，获得 authority 权限");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[客户端] OnStartClient 被调用，netId = {netId}");
        Debug.Log($"📡 [客户端] Player turnIndex={turnIndex}, isMyTurn={isMyTurn}");

        // 延迟生成全体 UI，避免未初始化
        Invoke(nameof(GenerateUIWithDelay), 1.0f);
    }

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

        Debug.Log($"🃏 [客户端] 手牌列表变更({op}) → 刷新 UI");
        GameManager.Instance.playerHandManager.ShowHand(hand);
    }

    [Command]
    public void CmdInit(string name)
    {
        Debug.Log($"🛠️ [服务端] 执行 CmdInit 初始化玩家: {name}");
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
