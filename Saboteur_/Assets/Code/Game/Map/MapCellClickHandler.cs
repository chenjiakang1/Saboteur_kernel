// MapCellClickHandler.cs
using UnityEngine;
using Mirror;

/// <summary>
/// 处理玩家点击地图格子的交互逻辑
/// </summary>
public class MapCellClickHandler : MonoBehaviour
{
    private MapCellState state;
    private MapCellUI ui;
    private MapCellNetwork net;

    private void Awake()
    {
        state = GetComponent<MapCellState>();
        ui = GetComponent<MapCellUI>();
        net = GetComponent<MapCellNetwork>();
    }

    public void OnClick()
    {
        PlayerController.DebugClient($"🟪 点击地图格子 ({state.row},{state.col}) → isBlocked: {state.isBlocked}, isOccupied: {state.isOccupied}");

        var pending = GameManager.Instance.pendingCard;

        if (pending.HasValue &&
            pending.Value.cardType == Card.CardType.Action &&
            pending.Value.toolEffect == "Collapse")
        {
            PlayerController.DebugClient($"💥 尝试使用塌方卡在 ({state.row},{state.col})");
            GameManager.Instance.collapseManager.ApplyCollapseTo(GetComponent<MapCell>());
            return;
        }

        if (GameManager.Instance.gameStateManager.hasGameEnded)
        {
            GameManager.Instance.endGameTip?.SetActive(true);
            return;
        }

        if (state.isBlocked || state.isOccupied)
        {
            PlayerController.DebugClient($"⛔ 格子 ({state.row},{state.col}) 被阻挡或已占用");
            return;
        }

        if (!pending.HasValue || GameManager.Instance.pendingSprite == null)
        {
            PlayerController.DebugClient("⚠️ 无 pendingCard，点击无效");
            return;
        }

        var cardData = pending.Value;
        var currentPlayer = PlayerController.LocalInstance;
        if (currentPlayer == null)
        {
            PlayerController.DebugClient("❌ LocalInstance 为空，无法出牌");
            return;
        }

        // 工具卡限制：必须修复后才能出路径卡
        if (cardData.cardType == Card.CardType.Path &&
            (!currentPlayer.hasLamp || !currentPlayer.hasPickaxe || !currentPlayer.hasMineCart))
        {
            var toolUI = GameManager.Instance.toolEffectManager;
            toolUI.toolRepeatTipPanel?.SetActive(true);
            toolUI.textToolAlreadyBroken?.SetActive(true);
            toolUI.textToolAlreadyRepaired?.SetActive(false);
            toolUI.CancelInvoke("HideToolRepeatTip");
            toolUI.Invoke("HideToolRepeatTip", 2f);
            PlayerController.DebugClient("⛏️ 工具破损，不能出路径卡");
            return;
        }

        // 邻居检查：是否能连接到已有路径
        bool canConnect = false;
        var map = GameManager.Instance.mapGenerator.mapCells;

        if (state.row > 0)
        {
            var neighbor = map[state.row - 1, state.col]?.GetCard();
            if (neighbor != null && cardData.up && neighbor.down) canConnect = true;
        }
        if (state.row < map.GetLength(0) - 1)
        {
            var neighbor = map[state.row + 1, state.col]?.GetCard();
            if (neighbor != null && cardData.down && neighbor.up) canConnect = true;
        }
        if (state.col > 0)
        {
            var neighbor = map[state.row, state.col - 1]?.GetCard();
            if (neighbor != null && cardData.left && neighbor.right) canConnect = true;
        }
        if (state.col < map.GetLength(1) - 1)
        {
            var neighbor = map[state.row, state.col + 1]?.GetCard();
            if (neighbor != null && cardData.right && neighbor.left) canConnect = true;
        }

        if (!canConnect)
        {
            PlayerController.DebugClient($"❌ 放置失败：({state.row},{state.col}) 无法连接到邻居路径");
            return;
        }

        // 发送出牌请求给服务端
        int replacedIndex = GameManager.Instance.pendingCardIndex;

        currentPlayer.CmdRequestPlaceCard(
            net.netId,
            cardData.cardName,
            cardData.spriteName,
            cardData.toolEffect,
            cardData.cardType,
            cardData.up, cardData.down, cardData.left, cardData.right,
            cardData.blockedCenter,
            cardData.isPathPassable,
            replacedIndex);

        GameManager.Instance.ClearPendingCard();

        var checker = Object.FindFirstObjectByType<PathChecker>();
        checker?.CheckWinCondition();

        TurnManager.Instance.NextTurn();
    }
}
