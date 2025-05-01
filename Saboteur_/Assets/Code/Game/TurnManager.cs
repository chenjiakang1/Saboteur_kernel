using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public int currentPlayer = 1;  // 1 or 2

    public TextMeshProUGUI turnText;      // 回合UI文本
    public TextMeshProUGUI localPlayerText;  // ✅ 本地操作玩家UI（新增）

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateTurnUI();
    }

    public void NextTurn()
    {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateTurnUI();
    }

    void UpdateTurnUI()
    {
        // 更新当前回合文字
        turnText.text = "Player " + currentPlayer + "'s Turn";

        // 同步本地玩家ID（用于出牌检测）
        GameManager.Instance.playerID = currentPlayer;

        // ✅ 同步LocalPlayerText
        if (localPlayerText != null)
        {
            localPlayerText.text = "Local Player " + GameManager.Instance.playerID;
        }
    }
}
