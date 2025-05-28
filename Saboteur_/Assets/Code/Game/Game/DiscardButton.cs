using UnityEngine;
using System.Collections;

public class DiscardButton : MonoBehaviour
{
    private PlayerController localPlayer;

    void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    private IEnumerator WaitForLocalPlayer()
    {
        while (PlayerController.LocalInstance == null)
            yield return null;

        localPlayer = PlayerController.LocalInstance;
        Debug.Log("✅ 已成功获取本地玩家引用");
    }

    public void OnDiscardButtonClicked()
    {
        if (localPlayer == null)
        {
            Debug.LogError("❌ localPlayer 为 null，无法弃置");
            return;
        }

        // ✅ 新增：检查是否是当前玩家回合
        if (!localPlayer.isMyTurn)
        {
            Debug.Log("⛔ 现在不是你的回合，不能弃置卡牌");
            return;
        }

        if (!GameManager.Instance.pendingCard.HasValue || GameManager.Instance.pendingCardIndex < 0)
        {
            Debug.Log("⚠️ 未选中卡牌，不能弃置");
            return;
        }

        int index = GameManager.Instance.pendingCardIndex;

        localPlayer.CmdUseAndDrawCard(index);
        localPlayer.CmdEndTurn();
        GameManager.Instance.ClearPendingCard();
    }
}
