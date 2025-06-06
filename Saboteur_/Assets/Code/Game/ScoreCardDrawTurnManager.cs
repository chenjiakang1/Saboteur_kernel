using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ScoreCardDrawTurnManager : NetworkBehaviour
{
    public static ScoreCardDrawTurnManager Instance;

    private List<PlayerController> turnList = new();
    private int currentTurnIndex = 0;

    public PlayerRole CurrentWinningRole { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public void StartDrawPhase(PlayerRole winnerRole)
    {
        Debug.Log($"🏁 Score card draw phase started. Winning role: {winnerRole}");

        turnList.Clear();
        currentTurnIndex = 0;
        CurrentWinningRole = winnerRole;

        var allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        PlayerController winner = GameStateManager.Instance?.GetWinnerPlayer();

        foreach (var player in allPlayers)
        {
            if (player.assignedRole == winnerRole)
            {
                turnList.Add(player);
            }
        }

        if (winner != null && turnList.Contains(winner))
        {
            turnList.Remove(winner);
            turnList.Insert(0, winner); // Ensure the winner starts first
        }

        if (turnList.Count == 0)
        {
            Debug.LogWarning("⚠️ No players of the winning role found.");
            return;
        }

        foreach (var player in allPlayers)
        {
            bool isMyTurn = (turnList.Count > 0 && player == turnList[0]);
            bool isInTurnList = turnList.Contains(player);
            player.TargetSetDrawTurn(player.connectionToClient, isMyTurn && isInTurnList);
        }

        BeginTurn();
    }

    [Server]
    private void BeginTurn()
    {
        for (int i = 0; i < turnList.Count; i++)
        {
            bool isMyTurn = (i == currentTurnIndex);
            turnList[i].TargetSetDrawTurn(turnList[i].connectionToClient, isMyTurn);

            if (isMyTurn)
            {
                Debug.Log($"🎯 Player {turnList[i].playerName}'s turn to draw a score card.");
            }
        }
    }

    [Server]
    public void EndCurrentTurnAndMoveNext()
    {
        if (turnList.Count == 0) return;

        turnList[currentTurnIndex].TargetSetDrawTurn(turnList[currentTurnIndex].connectionToClient, false);

        int remainingCards = FindObjectsByType<ScoreCardDisplay>(FindObjectsSortMode.None).Length;

        if (remainingCards == 0)
        {
            Debug.Log("✅ All score cards have been drawn. Ending draw phase.");
            EndDrawPhase();
            return;
        }
        currentTurnIndex = (currentTurnIndex + 1) % turnList.Count;

        BeginTurn();
    }

    [Server]
    private void EndDrawPhase()
    {
        Debug.Log("🏁 Score card draw phase complete.");

        foreach (var player in turnList)
        {
            player.TargetSetDrawTurn(player.connectionToClient, false);
        }
    }

    [Command]
    public void CmdRequestEndTurn(NetworkIdentity senderNetId)
    {
        if (!isServer) return;

        var player = senderNetId.GetComponent<PlayerController>();
        if (player != null && player == GetCurrentPlayer())
        {
            Debug.Log($"🔁 Player {player.playerName} requested to end their draw turn.");
            EndCurrentTurnAndMoveNext();
        }
        else
        {
            Debug.LogWarning($"⛔ Invalid end turn request by {player?.playerName}");
        }
    }

    [Server]
    public void ServerReceiveDrawEnd(PlayerController sender)
    {
        if (sender == GetCurrentPlayer())
        {
            Debug.Log($"✅ Valid draw turn end by {sender.playerName}");
            EndCurrentTurnAndMoveNext();
        }
        else
        {
            Debug.LogWarning($"⛔ Invalid draw end by {sender.playerName}");
        }
    }

    public PlayerController GetCurrentPlayer()
    {
        if (currentTurnIndex >= 0 && currentTurnIndex < turnList.Count)
            return turnList[currentTurnIndex];
        return null;
    }

    public List<PlayerController> GetDrawTurnOrder()
    {
        return new List<PlayerController>(turnList);
    }

    public bool IsPlayerInDrawList(PlayerController player)
    {
        return turnList.Contains(player);
    }

    public int GetTurnIndex(PlayerController player)
    {
        return turnList.IndexOf(player);
    }
}
