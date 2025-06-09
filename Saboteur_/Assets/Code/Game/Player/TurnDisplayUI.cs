using TMPro;
using UnityEngine;
using Mirror;

public class TurnDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI turnText;

    private void Update()
    {
        if (!NetworkClient.active || PlayerController.LocalInstance == null || TurnManager.Instance == null)
            return;

        int current = TurnManager.Instance.CurrentPlayerTurnIndex;
        int mine = PlayerController.LocalInstance.turnIndex;
        bool isMyTurn = PlayerController.LocalInstance.isMyTurn;

        if (isMyTurn)
        {
            turnText.text = $"It's your turn!";
        }
        else
        {
            turnText.text = $"It's not your turn.";
        }

    }
}
