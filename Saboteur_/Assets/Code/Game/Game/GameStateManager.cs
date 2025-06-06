using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class GameStateManager : NetworkBehaviour
{
    [Header("引用")]
    public GameObject victoryPanel;
    public GameObject gameOverVictory;
    public GameObject gameOverLose;

    [Header("胜利后展示的积分面板")]
    public GameObject scorePanel; // 拖入积分面板 UI

    [Header("积分卡生成控制器")]
    public ScoreCardDrawFlow scoreDrawFlow; // 拖入 ScoreCardDrawFlow 脚本对象

    [Header("胜者文本 UI")]
    public TMP_Text winnerText;
    [HideInInspector] public bool hasGameEnded = false;

    private uint winnerNetId = 0; // ✅ 本局胜者 NetId

    public static GameStateManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        if (scorePanel != null)
            scorePanel.SetActive(false);

        if (winnerText != null)
            winnerText.gameObject.SetActive(false); // 默认隐藏胜者文本
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

        // ✅ 显示胜者文本
        ShowWinnerText();

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

        // ✅ 显示胜者文本
        ShowWinnerText();

        StartCoroutine(HideVictoryPanelAfterDelay());
    }

    private void ShowWinnerText()
    {
        if (winnerText == null) return;

        var winner = GetWinnerPlayer();
        if (winner != null)
            winnerText.text = $" {winner.playerName} has reached the goal!";
        else
            winnerText.text = $"A player has reached the goal!";

        winnerText.gameObject.SetActive(true);
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

        if (winnerText != null)
            winnerText.gameObject.SetActive(false); // ✅ 隐藏胜者文本

        if (scorePanel != null)
            scorePanel.SetActive(true);

        if (isServer)
        {
            var winner = GetWinnerPlayer();
            if (winner != null)
            {
                var role = winner.assignedRole;
                Debug.Log($"🎯 胜利玩家身份是：{role} → 开始该身份玩家的抽卡流程");

                ScoreCardDrawTurnManager.Instance?.StartDrawPhase(role);
            }

            ResetWinner();
            if (scoreDrawFlow != null)
                scoreDrawFlow.StartDrawPhaseServer(); // ✅ UI部分继续保留
        }

    }

    [Server]
    public void RegisterPlayerReachedGoal(NetworkIdentity identity)
    {
        if (identity == null) return;

        if (winnerNetId == 0)
        {
            winnerNetId = identity.netId;
            Debug.Log($"🏁 玩家 {winnerNetId} 到达终点，本局胜者已记录");

            GameOver(true);
        }
    }

    [Server]
    public void ResetWinner()
    {
        winnerNetId = 0;
        Debug.Log("🔁 已重置胜者 NetId");
    }

    public uint GetWinnerNetId()
    {
        return winnerNetId;
    }

    public PlayerController GetWinnerPlayer()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("❌ GetWinnerPlayer() called on client side, must be server.");
            return null;
        }

        if (winnerNetId == 0)
        {
            Debug.LogWarning("❌ winnerNetId is 0. No player has been registered as winner.");
            return null;
        }

        if (NetworkServer.spawned.TryGetValue(winnerNetId, out NetworkIdentity identity))
        {
            var pc = identity.GetComponent<PlayerController>();
            if (pc == null)
                Debug.LogWarning("❌ Winner found but missing PlayerController component.");
            else
                Debug.Log($"✅ Winner is: {pc.playerName}, NetId: {winnerNetId}");
            return pc;
        }

        Debug.LogWarning($"❌ No spawned object found with winnerNetId: {winnerNetId}");
        return null;
    }

}
