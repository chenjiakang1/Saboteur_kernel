using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public int currentPlayer = 1;
    public int totalPlayers = 1;

    public TextMeshProUGUI turnText;
    public TextMeshProUGUI localPlayerText;

    private void Awake() => Instance = this;

    private void Start() => UpdateTurnUI();

    public void NextTurn()
    {
        currentPlayer++;
        if (currentPlayer > totalPlayers)
            currentPlayer = 1;

        UpdateTurnUI();

        // ✅ 加入回合切换后自动检测卡牌耗尽 + 所有手牌为空 → 判定失败
        if (!GameManager.Instance.hasGameEnded)
        {
            bool allHandCardsEmpty = true;

            foreach (var player in GameManager.Instance.playerGenerator.allPlayers)
            {
                foreach (var card in player.CardSlots)
                {
                    if (card != null)
                    {
                        allHandCardsEmpty = false;
                        break;
                    }
                }

                if (!allHandCardsEmpty)
                    break;
            }

            if (GameManager.Instance.cardDeck.Count == 0 && allHandCardsEmpty)
            {
                Debug.Log("❌ 所有卡牌已出完且玩家手牌为空，触发失败！");
                GameManager.Instance.GameOver(false);
            }
        }
    }

    public void UpdateTurnUI()
    {
        if (turnText != null)
            turnText.text = "Player " + currentPlayer + "'s Turn";

        GameManager.Instance.playerID = currentPlayer;

        if (GameManager.Instance.localPlayerText != null)
            GameManager.Instance.localPlayerText.text = "Local Player " + GameManager.Instance.viewPlayerID;

        GameManager.Instance.ShowPlayerHand(GameManager.Instance.viewPlayerID - 1);
    }
}
