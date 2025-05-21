// ✅ 完整 GameManager.cs（包含路径卡、阻断卡、破坏卡、修复卡、修复逻辑、破坏逻辑）
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

    [Header("破坏道具卡图像")]
    public Sprite breakLampSprite;
    public Sprite breakPickaxeSprite;
    public Sprite breakMinecartSprite;

    [Header("恢复道具卡图像")]
    public Sprite repairLampSprite;
    public Sprite repairPickaxeSprite;
    public Sprite repairMinecartSprite;
    public Sprite repairPickaxeAndMinecartSprite;
    public Sprite repairPickaxeAndLampSprite;
    public Sprite repairMinecartAndLampSprite;

    [Header("提示面板")]
    public GameObject breakSelfTipPanel;
    public GameObject toolBrokenTipPanel;

    [Header("游戏结束 UI")]
    public GameObject victoryPanel;
    public GameObject gameOverVictory;
    public GameObject gameOverLose;

    public GameObject endGameTip;

    private Dictionary<string, List<Sprite>> cardTypeToSprites = new();

    [HideInInspector] public Card pendingCard;
    [HideInInspector] public Sprite pendingSprite;
    [HideInInspector] public int pendingCardIndex = -1;

    public List<Card> cardDeck = new List<Card>();
    public int remainingCards = 0;

    public string pendingBreakEffect = null;
    public string pendingRepairEffect = null;
    public int pendingBreakCardIndex = -1;
    public int pendingRepairCardIndex = -1;

    [Header("塌方卡图像")]
    public Sprite collapseCardSprite;
    public bool hasGameEnded = false;

    public int pendingCollapseCardIndex = -1;

    [Header("重复使用工具提示面板")]
    public GameObject toolRepeatTipPanel;
    public GameObject textToolAlreadyBroken;
    public GameObject textToolAlreadyRepaired;


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

        Debug.Log($"🃏 总卡牌数量：{cardDeck.Count + playerGenerator.allPlayers.Count * 5}");
        Debug.Log($"🃏 剩余抽牌堆数量：{remainingCards}");
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
            GameOver(false);
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

    public void ShowBreakToolPanel(string effect, int cardIndex)
    {
        pendingBreakEffect = effect;
        pendingBreakCardIndex = cardIndex;
        Debug.Log($"🎯 等待选择玩家破坏工具：{effect}");
    }

    public void ShowRepairToolPanel(string effect, int cardIndex)
    {
        pendingRepairEffect = effect;
        pendingRepairCardIndex = cardIndex;
        Debug.Log($"🔧 等待选择玩家修复工具：{effect}");
    }

    public void ApplyBreakEffectTo(PlayerData target)
    {
        if (target == playerGenerator.allPlayers[playerID - 1])
        {
            Debug.Log("⚠️ 不能破坏自己的工具！");
            if (breakSelfTipPanel != null)
            {
                breakSelfTipPanel.SetActive(true);
                CancelInvoke("HideBreakSelfTip");
                Invoke("HideBreakSelfTip", 2f);
            }
            return;
        }

        bool alreadyBroken =
            (pendingBreakEffect == "BreakLamp" && !target.HasLamp) ||
            (pendingBreakEffect == "BreakPickaxe" && !target.HasPickaxe) ||
            (pendingBreakEffect == "BreakMinecart" && !target.HasMineCart);

        if (alreadyBroken)
        {
            Debug.Log("⚠️ 工具已被破坏，无法重复破坏！");
            if (toolRepeatTipPanel != null)
            {
                toolRepeatTipPanel.SetActive(true);
                if (textToolAlreadyBroken != null) textToolAlreadyBroken.SetActive(true);
                if (textToolAlreadyRepaired != null) textToolAlreadyRepaired.SetActive(false);
                CancelInvoke("HideToolRepeatTip");
                Invoke("HideToolRepeatTip", 2f);
            }
            return;
        }


        switch (pendingBreakEffect)
        {
            case "BreakLamp": target.HasLamp = false; break;
            case "BreakPickaxe": target.HasPickaxe = false; break;
            case "BreakMinecart": target.HasMineCart = false; break;
        }

        ReplaceUsedCard(pendingBreakCardIndex);
        ClearPendingCard();
        pendingBreakEffect = null;
        pendingBreakCardIndex = -1;

        playerUIManager.UpdateAllUI();
        TurnManager.Instance.NextTurn();
    }


    public void ApplyRepairEffectTo(PlayerData target)
    {
        bool didRepair = false;

        // 单修复逻辑
        if (pendingRepairEffect == "RepairLamp" && !target.HasLamp)
        {
            target.HasLamp = true;
            didRepair = true;
        }
        else if (pendingRepairEffect == "RepairPickaxe" && !target.HasPickaxe)
        {
            target.HasPickaxe = true;
            didRepair = true;
        }
        else if (pendingRepairEffect == "RepairMinecart" && !target.HasMineCart)
        {
            target.HasMineCart = true;
            didRepair = true;
        }

        // 双修复逻辑
        else if (pendingRepairEffect == "RepairPickaxeAndMinecart")
        {
            if (!target.HasPickaxe) { target.HasPickaxe = true; didRepair = true; }
            if (!target.HasMineCart) { target.HasMineCart = true; didRepair = true; }
        }
        else if (pendingRepairEffect == "RepairPickaxeAndLamp")
        {
            if (!target.HasPickaxe) { target.HasPickaxe = true; didRepair = true; }
            if (!target.HasLamp) { target.HasLamp = true; didRepair = true; }
        }
        else if (pendingRepairEffect == "RepairMinecartAndLamp")
        {
            if (!target.HasMineCart) { target.HasMineCart = true; didRepair = true; }
            if (!target.HasLamp) { target.HasLamp = true; didRepair = true; }
        }

        // 如果什么都没修复，提示“已修复过”
        if (!didRepair)
        {
            Debug.Log("⚠️ 所有目标工具都已完好，无法修复！");
            if (toolRepeatTipPanel != null)
            {
                toolRepeatTipPanel.SetActive(true);
                if (textToolAlreadyBroken != null) textToolAlreadyBroken.SetActive(false);
                if (textToolAlreadyRepaired != null) textToolAlreadyRepaired.SetActive(true);
                CancelInvoke("HideToolRepeatTip");
                Invoke("HideToolRepeatTip", 2f);
            }
            return;
        }

        // 替换手牌 + 清除状态
        ReplaceUsedCard(pendingRepairCardIndex);
        ClearPendingCard();
        pendingRepairEffect = null;
        pendingRepairCardIndex = -1;

        playerUIManager.UpdateAllUI();
        TurnManager.Instance.NextTurn();
    }

    void ReplaceUsedCard(int index)
    {
        var currentPlayer = playerGenerator.allPlayers[playerID - 1];
        if (index >= 0 && index < currentPlayer.CardSlots.Length)
        {
            Card newCard = DrawCard();
            currentPlayer.CardSlots[index] = newCard;
        }
    }

    public void GameOver(bool isVictory = true)
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

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

        //路径卡：精确控制每种数量
        AddPathCard("Cross", 5, cardTypeMap);
        AddPathCard("ULR", 4, cardTypeMap);
        AddPathCard("DLR", 1, cardTypeMap);
        AddPathCard("UDL", 2, cardTypeMap);
        AddPathCard("UDR", 3, cardTypeMap);
        AddPathCard("Vertical", 4, cardTypeMap);
        AddPathCard("Horizontal", 3, cardTypeMap);
        AddPathCard("UL", 2, cardTypeMap);
        AddPathCard("UR", 3, cardTypeMap);
        AddPathCard("DL", 2, cardTypeMap);
        AddPathCard("DR", 2, cardTypeMap);

        //阻断卡（共 9 张）
        cardDeck.Add(CreateBlockedCard(false, false, true, false, "BLOCK_L", blockedSprite_L));
        cardDeck.Add(CreateBlockedCard(false, true, false, false, "BLOCK_D", blockedSprite_D));
        cardDeck.Add(CreateBlockedCard(false, false, true, true, "BLOCK_LR", blockedSprite_LR));
        cardDeck.Add(CreateBlockedCard(false, true, true, false, "BLOCK_LD", blockedSprite_LD));
        cardDeck.Add(CreateBlockedCard(true, true, false, false, "BLOCK_UD", blockedSprite_UD));
        cardDeck.Add(CreateBlockedCard(false, true, false, true, "BLOCK_DR", blockedSprite_DR));
        cardDeck.Add(CreateBlockedCard(true, false, true, true, "BLOCK_ULR", blockedSprite_ULR));
        cardDeck.Add(CreateBlockedCard(true, true, true, false, "BLOCK_ULD", blockedSprite_ULD));
        cardDeck.Add(CreateBlockedCard(true, true, true, true, "BLOCK_UDLR", blockedSprite_UDLR));

        //破坏卡：每种 3 张
        for (int i = 0; i < 3; i++)
        {
            cardDeck.Add(CreateToolCard("BreakLamp", Card.CardType.Tool, breakLampSprite));
            cardDeck.Add(CreateToolCard("BreakPickaxe", Card.CardType.Tool, breakPickaxeSprite));
            cardDeck.Add(CreateToolCard("BreakMinecart", Card.CardType.Tool, breakMinecartSprite));
        }

        //单修复卡：每种 2 张
        for (int i = 0; i < 2; i++)
        {
            cardDeck.Add(CreateToolCard("RepairLamp", Card.CardType.Tool, repairLampSprite));
            cardDeck.Add(CreateToolCard("RepairPickaxe", Card.CardType.Tool, repairPickaxeSprite));
            cardDeck.Add(CreateToolCard("RepairMinecart", Card.CardType.Tool, repairMinecartSprite));
        }

        //双修复卡：每种 1 张
        cardDeck.Add(CreateToolCard("RepairPickaxeAndMinecart", Card.CardType.Tool, repairPickaxeAndMinecartSprite));
        cardDeck.Add(CreateToolCard("RepairPickaxeAndLamp", Card.CardType.Tool, repairPickaxeAndLampSprite));
        cardDeck.Add(CreateToolCard("RepairMinecartAndLamp", Card.CardType.Tool, repairMinecartAndLampSprite));

        for (int i = 0; i < 3; i++)
        {
            cardDeck.Add(CreateToolCard("Collapse", Card.CardType.Action, collapseCardSprite));
        }

        ShuffleDeck();
        remainingCards = cardDeck.Count;
    }

    void AddPathCard(string key, int count, Dictionary<string, Card> cardTypeMap)
    {
        if (!cardTypeToSprites.ContainsKey(key) || cardTypeToSprites[key].Count == 0) return;
        if (!cardTypeMap.ContainsKey(key)) return;

        var baseCard = cardTypeMap[key];
        var sprites = cardTypeToSprites[key];

        for (int i = 0; i < count; i++)
        {
            var sprite = sprites[i % sprites.Count]; // 循环使用 sprite
            Card newCard = new Card(baseCard.up, baseCard.down, baseCard.left, baseCard.right, key);
            newCard.sprite = sprite;
            newCard.cardType = Card.CardType.Path;
            newCard.isPathPassable = true;
            newCard.blockedCenter = false;
            cardDeck.Add(newCard);
        }
    }


    private Card CreateBlockedCard(bool u, bool d, bool l, bool r, string name, Sprite sprite)
    {
        Card card = new Card(u, d, l, r, name);
        card.sprite = sprite;
        card.blockedCenter = true;
        card.isPathPassable = false;
        return card;
    }

    private Card CreateToolCard(string effect, Card.CardType type, Sprite sprite)
    {
        Card card = new Card(false, false, false, false, effect);
        card.cardType = type;
        card.toolEffect = effect;
        card.sprite = sprite;
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

    public void HideBreakSelfTip()
    {
        if (breakSelfTipPanel != null)
            breakSelfTipPanel.SetActive(false);
    }

    public void HideToolBrokenTip()
    {
        if (toolBrokenTipPanel != null)
            toolBrokenTipPanel.SetActive(false);
    }
    public void HideToolRepeatTip()
    {
        if (toolRepeatTipPanel != null) toolRepeatTipPanel.SetActive(false);
        if (textToolAlreadyBroken != null) textToolAlreadyBroken.SetActive(false);
        if (textToolAlreadyRepaired != null) textToolAlreadyRepaired.SetActive(false);
    }
    public void ApplyCollapseTo(MapCell cell)
    {
        Debug.Log($"🧨 正在尝试塌方：格子({cell.row},{cell.col}) isOccupied={cell.isOccupied}, card={cell.card}, cardDisplay={cell.cardDisplay}");

        // 判断格子是否合法
        if (!cell.isOccupied || cell.card == null)
        {
            Debug.Log("⛔ 无法塌方：该格子没有路径卡！");
            return;
        }

        if (cell.card.cardName == "Origin" || cell.card.cardName == "Terminal")
        {
            Debug.Log("🚫 不能对起点或终点使用塌方卡！");
            return;
        }

        if (cell.card.cardType != Card.CardType.Path)
        {
            Debug.Log("⛔ 只能塌方路径卡！");
            return;
        }

        // ✅ 正确销毁路径卡 UI 和数据
        if (cell.cardDisplay != null)
        {
            Destroy(cell.cardDisplay.gameObject);
            cell.cardDisplay = null;
        }

        cell.card = null;
        cell.isOccupied = false;

        Debug.Log($"✅ 塌方成功：格子({cell.row},{cell.col}) 已被清除");

        // ✅ 1. 使用掉塌方卡（手牌中）
        ReplaceUsedCard(pendingCollapseCardIndex);

        // ✅ 2. 清除所有出牌状态（避免复制 bug）
        ClearPendingCard();
        pendingCard = null;
        pendingSprite = null;
        pendingCardIndex = -1;
        pendingCollapseCardIndex = -1;

        // ✅ 3. UI刷新 + 下一回合
        playerUIManager.UpdateAllUI();
        TurnManager.Instance.NextTurn();
    }

} 
