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
