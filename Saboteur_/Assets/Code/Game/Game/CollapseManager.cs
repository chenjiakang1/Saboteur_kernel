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

        Debug.Log($"🧨 使用塌方卡：格子({state.row}, {state.col})");

        if (state.card == null || state.card.cardType != Card.CardType.Path)
        {
            Debug.Log("⛔ 塌方卡只能用于清除路径卡");
            return;
        }

        // ✅ 通过服务端广播地图格子清除状态
        var player = NetworkClient.connection.identity.GetComponent<PlayerController>();
        player.CmdCollapseMapCell(cell.netId);

        // ✅ 使用塌方卡（不放置卡，仅销毁并补发）
        int index = GameManager.Instance.pendingCardIndex;
        if (index >= 0)
        {
            player.CmdUseCollapseCardOnly(index);
        }
        else
        {
            Debug.LogError("❗ 塌方卡使用失败：pendingCardIndex 无效");
        }

        GameManager.Instance.ClearPendingCard();
        TurnManager.Instance.NextTurn();

        Debug.Log($"✅ 清除完成，格子({state.row},{state.col}) 现在可以重新放牌");
    }
}
