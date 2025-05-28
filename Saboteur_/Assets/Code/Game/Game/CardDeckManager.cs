using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CardDeckManager : NetworkBehaviour
{
    public GameManager gameManager;

    [Header("路径卡图集")]
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

    [Header("阻断卡图像")]
    public Sprite blockedSprite_L;
    public Sprite blockedSprite_D;
    public Sprite blockedSprite_LR;
    public Sprite blockedSprite_LD;
    public Sprite blockedSprite_UD;
    public Sprite blockedSprite_DR;
    public Sprite blockedSprite_ULR;
    public Sprite blockedSprite_ULD;
    public Sprite blockedSprite_UDLR;

    [Header("破坏卡图像")]
    public Sprite breakLampSprite;
    public Sprite breakPickaxeSprite;
    public Sprite breakMinecartSprite;

    [Header("修复卡图像")]
    public Sprite repairLampSprite;
    public Sprite repairPickaxeSprite;
    public Sprite repairMinecartSprite;
    public Sprite repairPickaxeAndMinecartSprite;
    public Sprite repairPickaxeAndLampSprite;
    public Sprite repairMinecartAndLampSprite;

    [Header("塌方卡图像")]
    public Sprite collapseCardSprite;

    public List<Card> cardDeck = new List<Card>();
    public int remainingCards = 0;

    [Header("探查卡图像")]
    public Sprite scoutToolSprite;

    public CardDeckDisplay deckDisplay; // 拖入 UI 脚本


    private Dictionary<string, List<Sprite>> spriteGroups = new();

    private void Awake()
    {
        InitSpriteGroups();
    }

    private void InitSpriteGroups()
    {
        spriteGroups["Cross"] = crossSprites;
        spriteGroups["ULR"] = ulrSprites;
        spriteGroups["DLR"] = dlrSprites;
        spriteGroups["UDL"] = udlSprites;
        spriteGroups["UDR"] = udrSprites;
        spriteGroups["Vertical"] = verticalSprites;
        spriteGroups["Horizontal"] = horizontalSprites;
        spriteGroups["UL"] = ulSprites;
        spriteGroups["UR"] = urSprites;
        spriteGroups["DL"] = dlSprites;
        spriteGroups["DR"] = drSprites;
    }

    public void InitCardDeck()
    {
        cardDeck.Clear();

        AddPathCards("Cross", 5, new Card(true, true, true, true, "Cross"));
        AddPathCards("ULR", 4, new Card(true, false, true, true, "ULR"));
        AddPathCards("DLR", 1, new Card(false, true, true, true, "DLR"));
        AddPathCards("UDL", 2, new Card(true, true, true, false, "UDL"));
        AddPathCards("UDR", 3, new Card(true, true, false, true, "UDR"));
        AddPathCards("Vertical", 4, new Card(true, true, false, false, "Vertical"));
        AddPathCards("Horizontal", 3, new Card(false, false, true, true, "Horizontal"));
        AddPathCards("UL", 2, new Card(true, false, true, false, "UL"));
        AddPathCards("UR", 3, new Card(true, false, false, true, "UR"));
        AddPathCards("DL", 2, new Card(false, true, true, false, "DL"));
        AddPathCards("DR", 2, new Card(false, true, false, true, "DR"));

        // 阻断卡
        cardDeck.Add(CreateBlockedCard(false, false, true, false, "BLOCK_L", blockedSprite_L));
        cardDeck.Add(CreateBlockedCard(false, true, false, false, "BLOCK_D", blockedSprite_D));
        cardDeck.Add(CreateBlockedCard(false, false, true, true, "BLOCK_LR", blockedSprite_LR));
        cardDeck.Add(CreateBlockedCard(false, true, true, false, "BLOCK_LD", blockedSprite_LD));
        cardDeck.Add(CreateBlockedCard(true, true, false, false, "BLOCK_UD", blockedSprite_UD));
        cardDeck.Add(CreateBlockedCard(false, true, false, true, "BLOCK_DR", blockedSprite_DR));
        cardDeck.Add(CreateBlockedCard(true, false, true, true, "BLOCK_ULR", blockedSprite_ULR));
        cardDeck.Add(CreateBlockedCard(true, true, true, false, "BLOCK_ULD", blockedSprite_ULD));
        cardDeck.Add(CreateBlockedCard(true, true, true, true, "BLOCK_UDLR", blockedSprite_UDLR));

        // 破坏卡 ×3
        for (int i = 0; i < 3; i++)
        {
            cardDeck.Add(CreateToolCard("BreakLamp", Card.CardType.Tool, breakLampSprite));
            cardDeck.Add(CreateToolCard("BreakPickaxe", Card.CardType.Tool, breakPickaxeSprite));
            cardDeck.Add(CreateToolCard("BreakMinecart", Card.CardType.Tool, breakMinecartSprite));
        }

        // 修复卡
        for (int i = 0; i < 2; i++)
        {
            cardDeck.Add(CreateToolCard("RepairLamp", Card.CardType.Tool, repairLampSprite));
            cardDeck.Add(CreateToolCard("RepairPickaxe", Card.CardType.Tool, repairPickaxeSprite));
            cardDeck.Add(CreateToolCard("RepairMinecart", Card.CardType.Tool, repairMinecartSprite));
        }

        cardDeck.Add(CreateToolCard("RepairPickaxeAndMinecart", Card.CardType.Tool, repairPickaxeAndMinecartSprite));
        cardDeck.Add(CreateToolCard("RepairPickaxeAndLamp", Card.CardType.Tool, repairPickaxeAndLampSprite));
        cardDeck.Add(CreateToolCard("RepairMinecartAndLamp", Card.CardType.Tool, repairMinecartAndLampSprite));

        // 塌方卡 ×3
        for (int i = 0; i < 3; i++)
        {
            cardDeck.Add(CreateToolCard("Collapse", Card.CardType.Action, collapseCardSprite));
        }

        // ✅ 探查卡 ×6
        for (int i = 0; i < 6; i++)
        {
            cardDeck.Add(CreateToolCard("Scout", Card.CardType.Tool, scoutToolSprite));
        }

        ShuffleDeck();
        remainingCards = cardDeck.Count;
    }


    private void AddPathCards(string key, int count, Card template)
    {
        if (!spriteGroups.ContainsKey(key)) return;

        var sprites = spriteGroups[key];
        for (int i = 0; i < count; i++)
        {
            Sprite sprite = sprites[i % sprites.Count];
            Card card = new Card(template.up, template.down, template.left, template.right, key);
            card.cardType = Card.CardType.Path;
            card.sprite = sprite;
            card.isPathPassable = true;
            card.blockedCenter = false;
            card.toolEffect = "";
            cardDeck.Add(card);
        }
    }

    private Card CreateBlockedCard(bool u, bool d, bool l, bool r, string name, Sprite sprite)
    {
        Card card = new Card(u, d, l, r, name);
        card.sprite = sprite;
        card.cardType = Card.CardType.Path;
        card.isPathPassable = false;
        card.blockedCenter = true;
        return card;
    }

    private Card CreateToolCard(string name, Card.CardType type, Sprite sprite)
    {
        Card card = new Card(false, false, false, false, name);
        card.sprite = sprite;
        card.cardType = type;
        card.toolEffect = name;
        return card;
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < cardDeck.Count; i++)
        {
            int j = Random.Range(i, cardDeck.Count);
            (cardDeck[i], cardDeck[j]) = (cardDeck[j], cardDeck[i]);
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

        RpcUpdateRemainingCards(remainingCards); // ✅ 通知所有客户端更新 UI

        if (remainingCards <= 0 && !gameManager.gameStateManager.hasGameEnded)
        {
            gameManager.gameStateManager.RpcGameOver(false);
        }

        return card;
    }
    [ClientRpc]
    void RpcUpdateRemainingCards(int count)
    {
        if (deckDisplay != null)
        {
            deckDisplay.UpdateText(count);
        }
    }



    public Sprite FindSpriteByName(string spriteName)
    {
        foreach (var card in cardDeck)
        {
            if (card.sprite != null && card.sprite.name == spriteName)
                return card.sprite;
        }

        foreach (var list in spriteGroups.Values)
        {
            foreach (var s in list)
            {
                if (s != null && s.name == spriteName)
                    return s;
            }
        }

        Sprite[] extraSprites = new Sprite[]
        {
            blockedSprite_L, blockedSprite_D, blockedSprite_LR, blockedSprite_LD,
            blockedSprite_UD, blockedSprite_DR, blockedSprite_ULR, blockedSprite_ULD, blockedSprite_UDLR,
            breakLampSprite, breakPickaxeSprite, breakMinecartSprite,
            repairLampSprite, repairPickaxeSprite, repairMinecartSprite,
            repairPickaxeAndMinecartSprite, repairPickaxeAndLampSprite, repairMinecartAndLampSprite,
            collapseCardSprite,
            scoutToolSprite
        };

        foreach (var s in extraSprites)
        {
            if (s != null && s.name == spriteName)
                return s;
        }

        Debug.LogWarning("❓ 未找到 spriteName: " + spriteName);
        return null;
    }
}