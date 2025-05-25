using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

/// <summary>
/// 回合管理器：负责维护回合顺序、广播当前回合状态
/// </summary>
public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    private List<PlayerController> playerList = new List<PlayerController>();
    private int currentIndex = 0;

    public int requiredPlayerCount = 2; // ✅ 可调整所需玩家数（默认 2）

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 由 PlayerController.OnStartServer 调用：逐个注册玩家
    /// </summary>
    public void RegisterPlayer(PlayerController player)
    {
        if (!isServer) return;

        if (!playerList.Contains(player))
        {
            playerList.Add(player);
            Debug.Log($"✅ 注册玩家: netId={player.netId}，当前共 {playerList.Count} 人");

            // 自动初始化回合（人数够时）
            if (playerList.Count >= requiredPlayerCount)
            {
                InitTurnOrder(playerList);
            }
        }
    }

    /// <summary>
    /// 初始化回合顺序（只调用一次）
    /// </summary>
    public void InitTurnOrder(List<PlayerController> sortedPlayers)
    {
        playerList = sortedPlayers
            .OrderBy(p => p.netId) // ✅ 按加入顺序排序
            .ToList();

        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].turnIndex = i + 1;
            playerList[i].isMyTurn = (i == 0); // 第一人先手
        }

        BroadcastTurnState();
    }

    /// <summary>
    /// 服务端调用：切换到下一个玩家
    /// </summary>
    public void NextTurn()
    {
        if (playerList.Count == 0)
        {
            Debug.LogWarning("⚠️ 无玩家，无法轮换回合");
            return;
        }

        currentIndex = (currentIndex + 1) % playerList.Count;
        BroadcastTurnState();
    }

    /// <summary>
    /// 服务端广播每位玩家是否轮到他
    /// </summary>
    private void BroadcastTurnState()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            bool isTurn = (i == currentIndex);
            playerList[i].isMyTurn = isTurn;

            if (playerList[i].connectionToClient != null && playerList[i].connectionToClient.isReady)
            {
                playerList[i].TargetSetTurn(playerList[i].connectionToClient, isTurn);
            }
        }

        Debug.Log($"🌀 当前回合玩家：Player{playerList[currentIndex].turnIndex}");
    }

    /// <summary>
    /// 对外提供：当前回合的玩家编号（供调试 UI 使用）
    /// </summary>
    public int CurrentPlayerTurnIndex
    {
        get
        {
            if (playerList == null || playerList.Count == 0) return -1;
            return playerList[currentIndex].turnIndex;
        }
    }
}
