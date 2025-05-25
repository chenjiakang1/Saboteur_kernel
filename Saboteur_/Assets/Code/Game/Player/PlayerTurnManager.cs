using Mirror;
using UnityEngine;

/// <summary>
/// 管理每位玩家的回合信息，并处理服务端轮换通知
/// </summary>
public partial class PlayerController
{

    /// <summary>
    /// 由服务端调用，明确告诉该客户端是否轮到其出牌
    /// </summary>
    [TargetRpc]
    public void TargetSetTurn(NetworkConnection target, bool isTurn)
    {
        isMyTurn = isTurn;
        Debug.Log($"🎯 [TargetSetTurn] netId={netId}, isMyTurn={isTurn}");

        // 可选：在此处更新 UI 提示（例如“轮到你了”）
        if (isLocalPlayer)
        {
            var ui = GameManager.Instance?.playerUIManager;
            ui?.UpdateAllUI(); // 若 UI 依赖 isMyTurn，可触发刷新
        }
    }

    /// <summary>
    /// 客户端在出牌后调用此命令 → 通知服务端轮换到下一位玩家
    /// </summary>
    [Command]
    public void CmdEndTurn()
    {
        Debug.Log("🌀 [服务端] CmdEndTurn 被调用 → 执行 TurnManager.NextTurn()");
        TurnManager.Instance.NextTurn();
    }
}
