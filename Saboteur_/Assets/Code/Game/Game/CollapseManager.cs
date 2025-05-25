using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class CollapseManager : MonoBehaviour
{
    public void ApplyCollapseTo(MapCell cell)
    {
        var state = cell.GetComponent<MapCellState>();
        var player = PlayerController.LocalInstance;

        if (!player.isMyTurn)
        {
            Debug.Log("⛔ 不是你的回合，不能使用塌方卡！");
            return;
        }

        Debug.Log($"🧨 使用塌方卡：格子({state.row}, {state.col})");

        if (state.card == null || state.card.cardType != Card.CardType.Path)
        {
            Debug.Log("⛔ 塌方卡只能用于清除路径卡");
            return;
        }

        player.CmdCollapseMapCell(cell.netId);

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
