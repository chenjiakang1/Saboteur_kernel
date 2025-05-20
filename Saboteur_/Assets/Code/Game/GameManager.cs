using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("玩家控制")]
    public int playerID = 1;         // 当前出牌玩家（固定由 TurnManager 控制）
    public int viewPlayerID = 1;     // 当前观察的玩家（点击按钮切换）
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
    public List<Sprite> crossSprites, ulrSprites, dlrSprites, udlSprites, udrSprites;
    public List<Sprite> verticalSprites, horizontalSprites;
    public List<Sprite> ulSprites, urSprites, dlSprites, drSprites;
    public Sprite blockedSprite_L, blockedSprite_D, blockedSprite_LR, blockedSprite_LD;
    public Sprite blockedSprite_UD, blockedSprite_DR, blockedSprite_ULR, blockedSprite_ULD, blockedSprite_UDLR;

    private Dictionary<string, List<Sprite>> cardTypeToSprites = new();
    [HideInInspector] public Card pendingCard;
    [HideInInspector] public Sprite pendingSprite;
    [HideInInspector] public int pendingCardIndex = -1;
    public List<Card> cardDeck = new List<Card>();

    void Awake() => Instance = this;

    void Start()
    {
        playerID = 1;
        viewPlayerID = 1;

        // ✅ 绑定按钮：只切换观察视角，不调用任何 UI 或回合逻辑
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                int next = viewPlayerID + 1;
                if (next > TurnManager.Instance.totalPlayers)
                    next = 1;

                viewPlayerID = next;
                if (localPlayerText != null)
                    localPlayerText.text = "Local Player " + viewPlayerID;

                ShowPlayerHand(viewPlayerID - 1);
            });
        }

        if (localPlayerText != null)
            localPlayerText.text = "Local Player " + viewPlayerID;

        InitCardSpriteMap();
        InitCardDeck();

        playerGenerator.GeneratePlayers(cardDeck);
        TurnManager.Instance.totalPlayers = playerGenerator.allPlayers.Count;

        ShowPlayerHand(viewPlayerID - 1);
        playerUIManager.GenerateUI(playerGenerator.allPlayers);
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

        Dictionary<string, Card> map = new()
        {
            { "Cross", new Card(true, true, true, true, "Cross") },
            { "ULR", new Card(true, false, true, true, "ULR") },
            { "DLR", new Card(false, true, true, true, "DLR") },
            { "UDL", new Card(true, true, true, false, "UDL") },
            { "UDR", new Card(true, true, false, true, "UDR") },
            { "Vertical", new Card(true, true, false, false, "Vertical") },
            { "Horizontal", new Card(false, false, true, true, "Horizontal") },
            { "UL", new Card(true, false, true, false, "UL") },
            { "UR", new Card(true, false, false, true, "UR") },
            { "DL", new Card(false, true, true, false, "DL") },
            { "DR", new Card(false, true, false, true, "DR") }
        };

        foreach (var entry in cardTypeToSprites)
        {
            if (!map.ContainsKey(entry.Key)) continue;
            foreach (var sprite in entry.Value)
            {
                var c = map[entry.Key];
                var newCard = new Card(c.up, c.down, c.left, c.right, entry.Key);
                newCard.sprite = sprite;
                newCard.blockedCenter = false;
                newCard.isPathPassable = true;
                cardDeck.Add(newCard);
            }
        }

        AddBlockedCards();
        ShuffleDeck();
    }

    void AddBlockedCards()
    {
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
    }

    Card CreateBlockedCard(bool u, bool d, bool l, bool r, string name, Sprite sprite)
    {
        Card card = new Card(u, d, l, r, name);
        card.sprite = sprite;
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
