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
}
