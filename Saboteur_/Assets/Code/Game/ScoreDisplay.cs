using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    public TMP_Text scoreText; // 拖入显示积分和ID的 UI 元素
    private PlayerController player;

    void Start()
    {
        player = PlayerController.LocalInstance;
        scoreText.text = "Test output";

        if (player == null)
        {
            Debug.LogWarning("❌ 未找到本地玩家！");
        }
    }

    void Update()
    {
        if (player == null)
        {
            player = PlayerController.LocalInstance;

            if (player != null)
            {
                Debug.Log($"✅ Found local player: {player.playerName}");
            }
        }

        if (player != null && scoreText != null)
        {
            scoreText.text = $"Player: {player.playerName} (NetID: {player.netId})\nScore: {player.score}";
        }
    }
}
