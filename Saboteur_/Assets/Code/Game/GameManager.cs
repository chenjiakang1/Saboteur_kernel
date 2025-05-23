using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
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

    private void Start()
    {
        // ✅ 安全检查
        if (cardDeckManager == null) Debug.LogError("❌ cardDeckManager 未赋值");
        if (mapGenerator == null) Debug.LogError("❌ mapGenerator 未赋值");
        if (playerHandManager == null) Debug.LogError("❌ playerHandManager 未赋值");
        if (playerUIManager == null) Debug.LogError("❌ playerUIManager 未赋值");

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

        // ✅ 初始化 UI
        playerUIManager?.GenerateUI();

        // ✅ 显示本地手牌
        if (NetworkClient.active || NetworkServer.active)
        {
            playerHandManager?.ShowLocalPlayerHand();
        }

        // ✅ 调试输出
        Debug.Log($"🃏 总卡牌数量：{cardDeckManager.cardDeck.Count + allPlayers.Length * 5}");
        Debug.Log($"🃏 剩余抽牌堆数量：{cardDeckManager.remainingCards}");
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
