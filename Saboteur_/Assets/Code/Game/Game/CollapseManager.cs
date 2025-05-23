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
        var state = cell.GetComponent<MapCellState>();
        var ui = cell.GetComponent<MapCellUI>();

        Debug.Log($"🧨 使用塌方卡：格子({state.row}, {state.col})");

        if (state.card == null || state.card.cardType != Card.CardType.Path)
        {
            Debug.Log("⛔ 塌方卡只能用于清除路径卡");
            return;
        }

        // ✅ 清除格子中路径卡的逻辑状态与显示
        state.card = null;
        state.isOccupied = false;

        if (ui.cardDisplay != null)
        {
            Destroy(ui.cardDisplay.gameObject);
            ui.cardDisplay = null;
        }

        // ✅ 恢复格子背景（图像恢复为灰色背景）
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
        Debug.Log($"✅ 清除完成，格子({state.row},{state.col}) 现在可以重新放牌");
    }
}
