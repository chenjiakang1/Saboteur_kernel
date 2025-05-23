using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("引用")]
    public GameObject victoryPanel;
    public GameObject gameOverVictory;
    public GameObject gameOverLose;

    [HideInInspector] public bool hasGameEnded = false;

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
