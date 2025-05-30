using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections.Generic;

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

    private bool hasDealtCards = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        Debug.Log("🟢 [服务端] GameManager.OnStartServer() 被调用");
        cardDeckManager?.InitCardDeck();
        Invoke(nameof(InitPlayersAfterDelay), 1.0f);
    }

    private void InitPlayersAfterDelay()
    {
        if (hasDealtCards)
        {
            Debug.Log("⛔ 已发过手牌，跳过 InitPlayersAfterDelay()");
            return;
        }
        hasDealtCards = true;

        var sortedPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
            .OrderBy(p => p.netId)
            .ToList();

        Debug.Log($"🧪 InitPlayersAfterDelay：共找到 {sortedPlayers.Count} 名玩家");

        //foreach (var player in sortedPlayers)
        //{
            //player.CmdInit(player.playerName);
        //}

        Invoke(nameof(CallClientGenerateUI), 1.0f);
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
