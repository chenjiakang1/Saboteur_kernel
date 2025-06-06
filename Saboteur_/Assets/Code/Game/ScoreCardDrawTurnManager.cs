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

    /// <summary>
    /// Called by the server to initiate the score card drawing phase.
    /// </summary>
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

    /// <summary>
    /// Server begins the turn for the current player.
    /// </summary>
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

    /// <summary>
    /// Server ends the current player's turn and moves to the next.
    /// </summary>
    [Server]
    public void EndCurrentTurnAndMoveNext()
    {
        if (turnList.Count == 0) return;

        // Turn off current player's turn
        turnList[currentTurnIndex].TargetSetDrawTurn(turnList[currentTurnIndex].connectionToClient, false);

        // Check if there are still cards left
        int remainingCards = FindObjectsByType<ScoreCardDisplay>(FindObjectsSortMode.None).Length;

        if (remainingCards == 0)
        {
            Debug.Log("✅ All score cards have been drawn. Ending draw phase.");
            EndDrawPhase();
            return;
        }

        // Move to the next player in a loop
        currentTurnIndex = (currentTurnIndex + 1) % turnList.Count;

        BeginTurn();
    }

    /// <summary>
    /// Called by the server to end the draw phase.
    /// </summary>
    [Server]
    private void EndDrawPhase()
    {
        Debug.Log("🏁 Score card draw phase complete.");

        foreach (var player in turnList)
        {
            player.TargetSetDrawTurn(player.connectionToClient, false);
        }

        // TODO: You may want to call into final scoring, game end, or transition here.
    }

    /// <summary>
    /// Called by a client to request their turn end after drawing.
    /// </summary>
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

    /// <summary>
    /// Alternative server-side method that uses direct player reference.
    /// </summary>
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

    /// <summary>
    /// Get current player in turn.
    /// </summary>
    public PlayerController GetCurrentPlayer()
    {
        if (currentTurnIndex >= 0 && currentTurnIndex < turnList.Count)
            return turnList[currentTurnIndex];
        return null;
    }

    /// <summary>
    /// Get the list of players allowed to draw.
    /// </summary>
    public List<PlayerController> GetDrawTurnOrder()
    {
        return new List<PlayerController>(turnList);
    }

    /// <summary>
    /// Checks whether the given player is in the draw list.
    /// </summary>
    public bool IsPlayerInDrawList(PlayerController player)
    {
        return turnList.Contains(player);
    }

    /// <summary>
    /// Get index of a player in draw turn list.
    /// </summary>
    public int GetTurnIndex(PlayerController player)
    {
        return turnList.IndexOf(player);
    }
}
