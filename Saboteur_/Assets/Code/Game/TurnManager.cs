using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public int currentPlayer = 1;
    public int totalPlayers = 1;

    public TextMeshProUGUI turnText;

    private void Awake() => Instance = this;

    private void Start() => UpdateTurnUI();

    public void NextTurn()
    {
        currentPlayer++;
        if (currentPlayer > totalPlayers)
            currentPlayer = 1;

        UpdateTurnUI();

        if (!GameManager.Instance.gameStateManager.hasGameEnded)
        {
            bool allHandCardsEmpty = true;

            foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
            {
                if (player.hand.Count > 0)
                {
                    allHandCardsEmpty = false;
                    break;
                }
            }

            if (GameManager.Instance.cardDeckManager.cardDeck.Count == 0 && allHandCardsEmpty)
            {
                Debug.Log("❌ 所有卡牌已出完且玩家手牌为空，触发失败！");
                GameManager.Instance.gameStateManager.GameOver(false);
            }
        }
    }

    public void UpdateTurnUI()
    {
        if (turnText != null)
            turnText.text = $"Player {currentPlayer}'s Turn";
    }
}