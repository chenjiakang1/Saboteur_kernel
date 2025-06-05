using UnityEngine;
using Mirror;
using System.Collections;

public class GameStateManager : NetworkBehaviour
{
    [Header("引用")]
    public GameObject victoryPanel;
    public GameObject gameOverVictory;
    public GameObject gameOverLose;

    [Header("胜利后展示的积分面板")]
    public GameObject scorePanel; // ✅ 拖入积分面板 UI

    [Header("积分卡生成控制器")]
    public ScoreCardDrawFlow scoreDrawFlow; // ✅ 拖入 ScoreCardDrawFlow 脚本对象

    [HideInInspector] public bool hasGameEnded = false;

    private void Start()
    {
        // ✅ 默认隐藏积分面板
        if (scorePanel != null)
            scorePanel.SetActive(false);
    }

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

        StartCoroutine(HideVictoryPanelAfterDelay());
    }

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

        StartCoroutine(HideVictoryPanelAfterDelay());
    }

    private IEnumerator HideVictoryPanelAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (gameOverVictory != null)
            gameOverVictory.SetActive(false);

        if (gameOverLose != null)
            gameOverLose.SetActive(false);

        // ✅ 显示积分面板
        if (scorePanel != null)
            scorePanel.SetActive(true);

        // ✅ 只让服务端调用生成积分卡逻辑
        if (isServer && scoreDrawFlow != null)
            scoreDrawFlow.StartDrawPhaseServer();
    }

}
