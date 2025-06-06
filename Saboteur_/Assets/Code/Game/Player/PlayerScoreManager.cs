using Mirror;
using UnityEngine;

public partial class PlayerController
{
    [SyncVar(hook = nameof(OnScoreChanged))]
    public int score = 0;

    /// <summary>
    /// 服务端调用：增加玩家积分
    /// </summary>
    [Server]
    public void AddScore(int amount)
    {
        if (!PlayerController.isGameplayEnabled) return;

        score += amount;
        Debug.Log($"🏆 玩家 {playerName} 获得 {amount} 分 → 当前积分：{score}");
    }

    /// <summary>
    /// 服务端调用：设置玩家积分
    /// </summary>
    [Server]
    public void SetScore(int value)
    {
        if (!PlayerController.isGameplayEnabled) return;

        score = value;
        Debug.Log($"🎯 玩家 {playerName} 的积分被设置为 {score}");
    }

    /// <summary>
    /// 自动触发的钩子：积分变化时调用
    /// </summary>
    private void OnScoreChanged(int oldScore, int newScore)
    {
        if (!PlayerController.isGameplayEnabled) return;

        Debug.Log($"🔁 玩家 {playerName} 的积分从 {oldScore} 变为 {newScore}");
    }

    /// <summary>
    /// 客户端调试用：请求服务端增加积分
    /// </summary>
    [Command]
    public void CmdDebugAddScore(int value)
    {
        if (!PlayerController.isGameplayEnabled) return;

        AddScore(value);
    }

    [Command]
    public void CmdRequestDestroyCard(string id)
    {
        var drawFlow = FindFirstObjectByType<ScoreCardDrawFlow>();
        if (drawFlow != null)
        {
            drawFlow.RpcDestroyCardById(id);
        }
    }

    [TargetRpc]
    public void TargetSetDrawTurn(NetworkConnection target, bool isTurn)
    {
        if (!PlayerController.isGameplayEnabled) return;

        isMyTurn = isTurn;

        Debug.Log($"🎯 TargetSetDrawTurn: 是否轮到我抽卡 = {isTurn}");

        if (isLocalPlayer && isTurn)
        {
            Debug.Log("🟢 轮到你抽积分卡，请选择一张");
            // TODO: 可触发 UI 提示，例如显示一个“请抽卡”图标
        }
    }

    [Command]
    public void CmdRequestScoreDrawEnd()
    {
        if (ScoreCardDrawTurnManager.Instance != null)
        {
            Debug.Log($"📨 CmdRequestScoreDrawEnd 被调用，玩家：{playerName}");
            ScoreCardDrawTurnManager.Instance.ServerReceiveDrawEnd(this);
        }
    }



    [Command]
    public void CmdEndMyTurn()
    {
        if (isMyTurn && isServer)
        {
            TurnManager.Instance.NextTurn();
        }
    }


}
