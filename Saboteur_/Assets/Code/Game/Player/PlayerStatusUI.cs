using TMPro;
using UnityEngine;

public class PlayerStatusUI : MonoBehaviour
{
    public TextMeshProUGUI playerIdText;
    public TextMeshProUGUI readyStatusText;

    public void SetInfo(string playerId, bool isReady)
    {
        playerIdText.text = playerId;
        readyStatusText.text = isReady ? "O" : "X";
    }

    public void UpdateReadyStatus(bool isReady)
    {
        readyStatusText.text = isReady ? "O" : "X";
    }
}
