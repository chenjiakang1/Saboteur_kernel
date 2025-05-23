using TMPro;
using UnityEngine;
using Mirror;

public class DebugDisplay : MonoBehaviour
{
    public TextMeshProUGUI debugText;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f) // 每 1 秒刷新一次
        {
            timer = 0f;
            UpdateDebugInfo();
        }
    }

    void UpdateDebugInfo()
    {
        string log = "";

        if (NetworkClient.active && NetworkClient.connection?.identity != null)
        {
            var player = NetworkClient.connection.identity.GetComponent<PlayerController>();
            log += $" Player: {player.playerName}\n";
            log += $" Hand Cards: {player.syncCardSlots.Count}\n";
            log += $" Pickaxe: {(player.hasPickaxe ? "OK" : "Broken")}, ";
            log += $" Minecart: {(player.hasMineCart ? "OK" : "Broken")}, ";
            log += $" Lamp: {(player.hasLamp ? "OK" : "Broken")}\n";
        }
        else
        {
            log += " NetworkClient not active or no local player\n";
        }

        if (GameManager.Instance.pendingCard.HasValue)
        {
            log += $" Pending Card: {GameManager.Instance.pendingCard.Value.cardName}\n";
        }
        else
        {
            log += " Pending Card: None\n";
        }

        log += $" Remaining Deck: {GameManager.Instance.cardDeckManager.cardDeck.Count}\n";

        if (TurnManager.Instance != null)
        {
            log += $" Turn: Player #{TurnManager.Instance.currentPlayer}\n";
        }

        debugText.text = log;
    }

}
