using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("当前玩家回合信息")]
    public int currentPlayer = 1;            // 当前回合玩家编号，从 1 开始
    public int totalPlayers = 2;             // 玩家总数（由 GameManager 设置）

    [Header("UI 显示")]
    public TextMeshProUGUI turnText;         // 显示当前回合玩家
    public TextMeshProUGUI localPlayerText;  // 显示本地玩家ID（如有）

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateTurnUI();
    }

    public void NextTurn()
    {
        currentPlayer++;

        if (currentPlayer > totalPlayers)
            currentPlayer = 1;

        UpdateTurnUI();
    }

    void UpdateTurnUI()
    {
        // 更新 UI 文本显示
        if (turnText != null)
            turnText.text = "Player " + currentPlayer + "'s Turn";

        // 同步 GameManager 的 playerID
        GameManager.Instance.playerID = currentPlayer;

        if (localPlayerText != null)
            localPlayerText.text = "Local Player " + currentPlayer;

        // 显示该玩家手牌
        GameManager.Instance.ShowPlayerHand(currentPlayer - 1);
    }
}
