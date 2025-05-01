using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    
    public TextMeshProUGUI localPlayerText;  // ✅ 本地操作玩家显示
    public Button actionButton;        // 测试按钮（用于切换本地玩家ID）
    public GameObject cardPrefab;      // 卡牌预制体
    public Transform cardParent;       // 手牌父物体
    public Transform mapParent;        // 地图父物体
    public Sprite[] cardSprites;       // 卡牌图片数组

    // 当前准备放置的卡牌（点击手牌时记录）
    [HideInInspector] public Card pendingCard;
    [HideInInspector] public Sprite pendingSprite;

    // 玩家数据
    private PlayerData player1;
    private PlayerData player2;

    public int playerID = 1; // 当前本地玩家ID（1或2）

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        localPlayerText.text = "Local Player " + playerID;
        // 初始化两个玩家（只用于回合显示）
        player1 = new PlayerData { Name = "Player 1", Gold = 0, IsMyTurn = true };
        player2 = new PlayerData { Name = "Player 2", Gold = 0, IsMyTurn = false };


        // 绑定按钮（测试用按钮切换本地playerID）
        if (actionButton != null)
            actionButton.onClick.AddListener(SwitchPlayerID);

        // 生成手牌
        CreateCardHand();
    }

    // ✅ 测试按钮用，切换本地玩家ID
    void SwitchPlayerID()
    {
        // 切换本地玩家ID（playerID 1 -> 2 -> 1）
        playerID = (playerID == 1) ? 2 : 1;

        // 输出当前玩家信息
        Debug.Log("Local playerID is now Player " + playerID);

        //更新本地玩家UI
        localPlayerText.text = "Local Player " + playerID;
    }

    // ✅ 更新回合UI
    
    // ✅ 创建手牌（开始游戏时生成3张卡牌）
    void CreateCardHand()
    {
        Card c1 = new Card(true, true, false, false, "Vertical Path");
        Card c2 = new Card(false, false, true, true, "Horizontal Path");
        Card c3 = new Card(true, false, false, true, "Corner");

        Card[] cards = { c1, c2, c3 };

        for (int i = 0; i < cards.Length; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, cardParent);
            CardDisplay display = cardGO.GetComponent<CardDisplay>();
            display.Init(cards[i], cardSprites[i]);
        }
    }

    // ✅ 设置准备放置的卡牌
    public void SetPendingCard(Card card, Sprite sprite)
    {
        pendingCard = card;
        pendingSprite = sprite;
    }

    // ✅ 清除准备放置状态
    public void ClearPendingCard()
    {
        pendingCard = null;
        pendingSprite = null;
    }
}
