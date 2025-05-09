using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public TextMeshProUGUI localPlayerText;
    public Button actionButton;
    public GameObject cardPrefab;
    public Transform cardParent;
    public Transform mapParent;
    public MapGenerator mapGenerator;

    [Header("四通卡")]
    public List<Sprite> crossSprites;
    [Header("三通卡 - 上 左 右")]
    public List<Sprite> ulrSprites;
    [Header("三通卡 - 下 左 右")]
    public List<Sprite> dlrSprites;
    [Header("三通卡 - 上 下 左")]
    public List<Sprite> udlSprites;
    [Header("三通卡 - 上 下 右")]
    public List<Sprite> udrSprites;
    [Header("两通卡 - 上 下")]
    public List<Sprite> verticalSprites;
    [Header("两通卡 - 左 右")]
    public List<Sprite> horizontalSprites;
    [Header("两通卡 - 上 左")]
    public List<Sprite> ulSprites;
    [Header("两通卡 - 上 右")]
    public List<Sprite> urSprites;
    [Header("两通卡 - 下 左")]
    public List<Sprite> dlSprites;
    [Header("两通卡 - 下 右")]
    public List<Sprite> drSprites;

    [Header("9张阻断卡牌图像（请按顺序拖入）")]
    public Sprite blockedSprite_L;
    public Sprite blockedSprite_D;
    public Sprite blockedSprite_LR;
    public Sprite blockedSprite_LD;
    public Sprite blockedSprite_UD;
    public Sprite blockedSprite_DR;
    public Sprite blockedSprite_ULR;
    public Sprite blockedSprite_ULD;
    public Sprite blockedSprite_UDLR;

    private Dictionary<string, List<Sprite>> cardTypeToSprites = new();

    [HideInInspector] public Card pendingCard;
    [HideInInspector] public Sprite pendingSprite;

    public List<Card> cardDeck = new List<Card>();
    private int drawIndex = 0;

    public int playerID = 1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        localPlayerText.text = "Local Player " + playerID;
        if (actionButton != null)
            actionButton.onClick.AddListener(SwitchPlayerID);

        InitCardSpriteMap();
        InitCardDeck();
        CreateCardHand();
    }

    void SwitchPlayerID()
    {
        playerID = (playerID == 1) ? 2 : 1;
        Debug.Log("Switched to Player " + playerID);
        localPlayerText.text = "Local Player " + playerID;
    }

    void InitCardSpriteMap()
    {
        cardTypeToSprites.Clear();
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
            string cardName = pair.Key;
            List<Sprite> spriteList = pair.Value;

            if (!cardTypeMap.ContainsKey(cardName)) continue;

            foreach (Sprite sprite in spriteList)
            {
                Card baseCard = cardTypeMap[cardName];
                Card newCard = new Card(baseCard.up, baseCard.down, baseCard.left, baseCard.right, cardName);
                newCard.sprite = sprite;
                newCard.blockedCenter = false;
                cardDeck.Add(newCard);
            }
        }

        // ✅ 添加 9 张阻断卡（方向真实）
        cardDeck.Add(CreateBlockedCard(false, false, true, false, "BLOCK_L", blockedSprite_L));        // 左通
        cardDeck.Add(CreateBlockedCard(false, true, false, false, "BLOCK_D", blockedSprite_D));        // 下通
        cardDeck.Add(CreateBlockedCard(false, false, true, true, "BLOCK_LR", blockedSprite_LR));       // 左右通
        cardDeck.Add(CreateBlockedCard(false, true, true, false, "BLOCK_LD", blockedSprite_LD));       // 左下通
        cardDeck.Add(CreateBlockedCard(true, true, false, false, "BLOCK_UD", blockedSprite_UD));       // 上下通
        cardDeck.Add(CreateBlockedCard(false, true, false, true, "BLOCK_DR", blockedSprite_DR));       // 下右通
        cardDeck.Add(CreateBlockedCard(true, false, true, true, "BLOCK_ULR", blockedSprite_ULR));      // 上左右通
        cardDeck.Add(CreateBlockedCard(true, true, true, false, "BLOCK_ULD", blockedSprite_ULD));      // 上下左通
        cardDeck.Add(CreateBlockedCard(true, true, true, true, "BLOCK_UDLR", blockedSprite_UDLR)); // 全封闭

        ShuffleDeck();
    }

    Card CreateBlockedCard(bool u, bool d, bool l, bool r, string name, Sprite sprite)
    {
        Card card = new Card(u, d, l, r, name);
        card.sprite = sprite;
        card.blockedCenter = true;
        card.isPathPassable = false; // ✅ 阻断卡不可通
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

    void CreateCardHand()
    {
        for (int i = 0; i < 5; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (drawIndex >= cardDeck.Count)
        {
            Debug.Log("Deck is empty!");
            return;
        }

        int currentHandCount = cardParent.GetComponentsInChildren<CardDisplay>().Length;
        if (currentHandCount >= 5)
        {
            Debug.Log("Hand is full, cannot draw more.");
            return;
        }

        Card card = cardDeck[drawIndex];
        drawIndex++;

        GameObject cardGO = Instantiate(cardPrefab, cardParent);
        CardDisplay display = cardGO.GetComponent<CardDisplay>();
        display.Init(card, card.sprite);
    }

    public void SetPendingCard(Card card, Sprite sprite)
    {
        pendingCard = card;
        pendingSprite = sprite;
    }

    public void ClearPendingCard()
    {
        pendingCard = null;
        pendingSprite = null;
    }

   
}
