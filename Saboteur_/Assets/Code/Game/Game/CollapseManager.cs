using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class CollapseManager : MonoBehaviour
{
    /// <summary>
    /// 使用塌方卡清除路径卡，让格子恢复为初始状态（可再次放置）
    /// </summary>
    public void ApplyCollapseTo(MapCell cell)
    {
        Debug.Log($"🧨 使用塌方卡：格子({cell.row}, {cell.col})");

        if (cell.card == null || cell.card.cardType != Card.CardType.Path)
        {
            Debug.Log("⛔ 塌方卡只能用于清除路径卡");
            return;
        }

        // ✅ 清除格子中路径卡的逻辑状态与显示
        cell.card = null;
        if (cell.cardDisplay != null)
        {
            Destroy(cell.cardDisplay.gameObject);
            cell.cardDisplay = null;
        }
        cell.isOccupied = false;

        // ✅ 恢复格子为未放置状态
        var img = cell.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = null;
            img.color = new Color32(0, 0, 0, 100);
        }

        // ✅ 替换手牌（使用 Command）
        int index = GameManager.Instance.pendingCardIndex;
        var player = NetworkClient.connection.identity.GetComponent<PlayerController>();
        if (index >= 0)
        {
            player.CmdReplaceUsedCard(index);
        }
        else
        {
            Debug.LogError("❗ 塌方卡替换失败：索引越界");
        }

        GameManager.Instance.ClearPendingCard();
        TurnManager.Instance.NextTurn();
        Debug.Log($"✅ 清除完成，格子({cell.row},{cell.col}) 现在可以重新放牌");
    }

}
