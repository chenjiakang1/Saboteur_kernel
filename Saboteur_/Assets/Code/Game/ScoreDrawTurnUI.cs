using UnityEngine;
using TMPro;

public class ScoreDrawTurnUI : MonoBehaviour
{
    public TextMeshProUGUI statusText;

    private float checkInterval = 0.5f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            UpdateTurnUI();
        }
    }

    void UpdateTurnUI()
    {
        var me = PlayerController.LocalInstance;
        if (me == null || ScoreCardDrawTurnManager.Instance == null)
        {
            statusText.text = "Loading player data...";
            return;
        }

        if (me.isMyTurn)
        {
            statusText.text = "It's your turn to draw a score card!";
        }
        else
        {
            statusText.text = "Waiting for other players to draw...";
        }
    }
}
