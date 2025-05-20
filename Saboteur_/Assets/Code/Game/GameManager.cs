// ✅ 完整 GameManager.cs（支持胜利与失败判定）
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("玩家控制")]
    public int playerID = 1;
    public int viewPlayerID = 1;
    public TextMeshProUGUI localPlayerText;
    public Button actionButton;

    [Header("卡牌展示")]
    public GameObject cardPrefab;
    public Transform cardParent;

    [Header("地图与生成器")]
    public Transform mapParent;
    public MapGenerator mapGenerator;
    public PlayerGenerator playerGenerator;

    [Header("玩家 UI 控制器")]
    public PlayerUIManager playerUIManager;

    [Header("卡牌资源")]
    public List<Sprite> crossSprites;
    public List<Sprite> ulrSprites;
    public List<Sprite> dlrSprites;
    public List<Sprite> udlSprites;
    public List<Sprite> udrSprites;
    public List<Sprite> verticalSprites;
    public List<Sprite> horizontalSprites;
    public List<Sprite> ulSprites;
    public List<Sprite> urSprites;
    public List<Sprite> dlSprites;
    public List<Sprite> drSprites;

    [Header("阻断卡牌图像")]
    public Sprite blockedSprite_L;
    public Sprite blockedSprite_D;
    public Sprite blockedSprite_LR;
    public Sprite blockedSprite_LD;
    public Sprite blockedSprite_UD;
    public Sprite blockedSprite_DR;
    public Sprite blockedSprite_ULR;
    public Sprite blockedSprite_ULD;
    public Sprite blockedSprite_UDLR;

    [Header("游戏结束 UI")]
    public GameObject victoryPanel;      // 共用面板
    public GameObject gameOverVictory;   // 胜利文本
    public GameObject gameOverLose;      // 失败文本

    [Header("提示文字")]
    public GameObject endGameTip;        // 点击卡牌时的结束提示

    private Dictionary<string, List<Sprite>> cardTypeToSprites = new();

    [HideInInspector] public Card pendingCard;
    [HideInInspector] public Sprite pendingSprite;
    [HideInInspector] public int pendingCardIndex = -1;

    public List<Card> cardDeck = new List<Card>();
    public int remainingCards = 0; // ✅ 剩余可抽牌数

    public bool hasGameEnded = false;

    void Awake() { Instance = this; }

    void Start()
    {
        playerID = 1;
        viewPlayerID = 1;

        if (localPlayerText != null)
            localPlayerText.text = "Local Player " + viewPlayerID;

        if (actionButton != null)
            actionButton.onClick.AddListener(SwitchToNextViewPlayer);

        InitCardSpriteMap();
        InitCardDeck();

        playerGenerator.GeneratePlayers(cardDeck);
        TurnManager.Instance.totalPlayers = playerGenerator.allPlayers.Count;

        ShowPlayerHand(viewPlayerID - 1);
        playerUIManager.GenerateUI(playerGenerator.allPlayers);
    }

    public void SwitchToNextViewPlayer()
    {
        viewPlayerID++;
        if (viewPlayerID > TurnManager.Instance.totalPlayers)
            viewPlayerID = 1;

        if (localPlayerText != null)
            localPlayerText.text = "Local Player " + viewPlayerID;

        ShowPlayerHand(viewPlayerID - 1);
    }

    public void ShowPlayerHand(int index)
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);

        var player = playerGenerator.allPlayers[index];
        var hand = player.CardSlots;

        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] == null) continue;

            GameObject cardGO = Instantiate(cardPrefab, cardParent);
            var display = cardGO.GetComponent<CardDisplay>();
            display.Init(hand[i], hand[i].sprite);
            display.cardIndex = i;
        }
    }

    public Card DrawCard()
    {
        if (cardDeck.Count == 0)
        {
            Debug.LogWarning("卡组已空");
            return null;
        }

        Card card = cardDeck[0];
        cardDeck.RemoveAt(0);
        remainingCards--;

        if (remainingCards <= 0 && !hasGameEnded)
        {
            GameOver(false); // ✅ 卡牌用尽未胜利 → 游戏失败
        }

        return card;
    }

    public void SetPendingCard(Card card, Sprite sprite, int cardIndex)
    {
        pendingCard = card;
        pendingSprite = sprite;
        pendingCardIndex = cardIndex;
    }

    public void ClearPendingCard()
    {
        pendingCard = null;
        pendingSprite = null;
    }

    public void GameOver(bool isVictory = true)
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

        Debug.Log(isVictory ? "🎉 游戏胜利：找到金矿卡" : "💀 游戏失败：卡牌用尽未找到金矿");

        if (actionButton != null)
            actionButton.interactable = false;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (gameOverVictory != null)
            gameOverVictory.SetActive(isVictory);

        if (gameOverLose != null)
            gameOverLose.SetActive(!isVictory);
    }

    void InitCardSpriteMap()
    {
        cardTypeToSprites["Cross"] = crossSprites;
        cardTypeToSprites["ULR"] = ulrSprites;
        cardTypeToSprites["DLR"] = dlrSprites;
        cardTypeToSprites["UDL"] = udlSprites;
        cardTypeToSprites["UDR"] = udrSprites;
        cardTypeToSprites["Vertical"] = verticalSprites;
        cardTypeToSprites["Horizontal"] = horizontalSprites;
        cardTypeToSprites["UL"] = ulSprites;
        cardTypeToSprites["UR"] = urSprites;
        cardTypeToSprites["DL"] = dlSprites;
        cardTypeToSprites["DR"] = drSprites;
    }

    void InitCardDeck()
    {
        cardDeck.Clear();

        Dictionary<string, Card> cardTypeMap = new()
        {
            { "Cross",      new Card(true, true, true, true, "Cross") },
            { "ULR",        new Card(true, false, true, true, "ULR") },
            { "DLR",        new Card(false, true, true, true, "DLR") },
            { "UDL",        new Card(true, true, true, false, "UDL") },
            { "UDR",        new Card(true, true, false, true, "UDR") },
            { "Vertical",   new Card(true, true, false, false, "Vertical") },
            { "Horizontal", new Card(false, false, true, true, "Horizontal") },
            { "UL",         new Card(true, false, true, false, "UL") },
            { "UR",         new Card(true, false, false, true, "UR") },
            { "DL",         new Card(false, true, true, false, "DL") },
            { "DR",         new Card(false, true, false, true, "DR") }
        };

        foreach (var pair in cardTypeToSprites)
        {
            if (!cardTypeMap.ContainsKey(pair.Key)) continue;

            foreach (var sprite in pair.Value)
            {
                Card baseCard = cardTypeMap[pair.Key];
                Card newCard = new Card(baseCard.up, baseCard.down, baseCard.left, baseCard.right, pair.Key);
                newCard.sprite = sprite;
                newCard.blockedCenter = false;
                newCard.isPathPassable = true;
                cardDeck.Add(newCard);
            }
        }

        // ✅ 阻断卡
        cardDeck.Add(CreateBlockedCard(false, false, true, false, "BLOCK_L", blockedSprite_L));
        cardDeck.Add(CreateBlockedCard(false, true, false, false, "BLOCK_D", blockedSprite_D));
        cardDeck.Add(CreateBlockedCard(false, false, true, true, "BLOCK_LR", blockedSprite_LR));
        cardDeck.Add(CreateBlockedCard(false, true, true, false, "BLOCK_LD", blockedSprite_LD));
        cardDeck.Add(CreateBlockedCard(true, true, false, false, "BLOCK_UD", blockedSprite_UD));
        cardDeck.Add(CreateBlockedCard(false, true, false, true, "BLOCK_DR", blockedSprite_DR));
        cardDeck.Add(CreateBlockedCard(true, false, true, true, "BLOCK_ULR", blockedSprite_ULR));
        cardDeck.Add(CreateBlockedCard(true, true, true, false, "BLOCK_ULD", blockedSprite_ULD));
        cardDeck.Add(CreateBlockedCard(true, true, true, true, "BLOCK_UDLR", blockedSprite_UDLR));

        foreach (var card in cardDeck)
        {
            if (card.cardName.StartsWith("BLOCK"))
            {
                card.blockedCenter = true;
                card.isPathPassable = false;
            }
        }

        ShuffleDeck();
        remainingCards = cardDeck.Count; // ✅ 初始化剩余卡牌数
    }

    Card CreateBlockedCard(bool u, bool d, bool l, bool r, string name, Sprite sprite)
    {
        Card card = new Card(u, d, l, r, name);
        card.sprite = sprite;
        card.blockedCenter = true;
        card.isPathPassable = false;
        return card;
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < cardDeck.Count; i++)
        {
            int j = Random.Range(i, cardDeck.Count);
            (cardDeck[i], cardDeck[j]) = (cardDeck[j], cardDeck[i]);
        }
    }
}
