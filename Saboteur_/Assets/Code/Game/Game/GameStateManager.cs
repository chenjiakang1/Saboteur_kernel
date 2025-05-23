using UnityEngine;
using Mirror;

public class GameStateManager : NetworkBehaviour
{
    [Header("引用")]
    public GameObject victoryPanel;
    public GameObject gameOverVictory;
    public GameObject gameOverLose;

    [HideInInspector] public bool hasGameEnded = false;

    // ✅ 用于服务端调用，通知所有客户端显示胜利/失败
    [ClientRpc]
    public void RpcGameOver(bool isVictory)
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (gameOverVictory != null)
            gameOverVictory.SetActive(isVictory);

        if (gameOverLose != null)
            gameOverLose.SetActive(!isVictory);
    }

    // ✅ 仍然保留本地调用接口（兼容原调用）
    public void GameOver(bool isVictory = true)
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (gameOverVictory != null)
            gameOverVictory.SetActive(isVictory);

        if (gameOverLose != null)
            gameOverLose.SetActive(!isVictory);
    }
}
