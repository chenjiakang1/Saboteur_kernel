using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("核心模块")]
    public CardDeckManager cardDeckManager;
    public PlayerHandManager playerHandManager;
    public PlayerUIManager playerUIManager;
    public MapGenerator mapGenerator;
    public CollapseManager collapseManager;
    public ToolEffectManager toolEffectManager;
    public GameStateManager gameStateManager;

    [Header("卡牌相关")]
    public GameObject cardPrefab;
    public Transform cardParent;

    [Header("UI")]
    public GameObject endGameTip;

    [Header("出牌状态缓存")]
    [HideInInspector] public CardData? pendingCard = null;
    [HideInInspector] public int pendingCardIndex = -1;
    [HideInInspector] public Sprite pendingSprite = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ 保证 Build 客户端 GameManager 不会销毁
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager 已存在，重复实例被销毁！");
            Destroy(gameObject);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // ✅ 初始化卡组
        cardDeckManager?.InitCardDeck();

        // ✅ 初始化玩家数量
        var allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.totalPlayers = allPlayers.Length;
        }
        else
        {
            Debug.LogWarning("⚠️ TurnManager 尚未初始化，totalPlayers 设置跳过");
        }

        // ✅ 给每个玩家发 5 张初始牌
        foreach (var player in allPlayers)
        {
            player.hand.Clear();
            for (int i = 0; i < 5; i++)
            {
                var card = cardDeckManager.DrawCard();
                if (card != null)
                    player.hand.Add(new CardData(card));
            }
        }

        // ✅ 延迟通知客户端生成玩家 UI 面板
        Invoke(nameof(CallClientGenerateUI), 1.0f);

        Debug.Log($"🃏 剩余抽牌堆数量：{cardDeckManager.remainingCards}");
    }

    private void CallClientGenerateUI()
    {
        RpcGenerateAllPlayerUI();
    }

    [ClientRpc]
    public void RpcGenerateAllPlayerUI()
    {
        Debug.Log("🎮 客户端收到 RpcGenerateAllPlayerUI，开始生成玩家 UI");
        playerUIManager?.GenerateUI();
    }

    public void SetPendingCard(CardData card, Sprite sprite, int index)
    {
        pendingCard = card;
        pendingSprite = sprite;
        pendingCardIndex = index;

        Debug.Log($"✅ [选中手牌] cardIndex = {index}, cardData = {card.cardName}");
        Debug.Log($"🔍 cardType = {card.cardType}, toolEffect = {card.toolEffect}");
    }

    public void ClearPendingCard()
    {
        Debug.Log("🧹 清除选中手牌状态");
        pendingCard = null;
        pendingSprite = null;
        pendingCardIndex = -1;
    }
}
