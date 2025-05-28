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
        Debug.Log("🔱 [MapCellClickHandler] 格子被点击");

        var currentPlayer = PlayerController.LocalInstance;
        if (currentPlayer == null)
        {
            Debug.LogError("❌ currentPlayer 为 null，LocalInstance 未正确设置！");
            return;
        }

        if (!currentPlayer.isMyTurn)
        {
            Debug.Log("⛔ 不是你的回合，不能放置卡片！");
            return;
        }

        PlayerController.DebugClient($"🔪 点击地图格子 ({state.row},{state.col}) → isBlocked: {state.isBlocked}, isOccupied: {state.isOccupied}");

        var pending = GameManager.Instance.pendingCard;

        // ✅ 探查卡逻辑
        if (pending.HasValue && pending.Value.toolEffect == "Scout")
        {
            Debug.Log("🧚 [探查逻辑] 判断到探查卡");

            if (!state.isBlocked)
            {
                Debug.Log("❌ 探查卡只能用于终点格");
                return;
            }
            //Debug.Log("🧚 [探查逻辑] 判断到探查卡");

            Debug.Log($"🔍 使用探查卡查看终点格：({state.row}, {state.col})");

            int cardIndex = GameManager.Instance.pendingCardIndex;

            var localPlayer = PlayerController.LocalInstance;

            Debug.Log($"🌟 [Network 验证] NetworkClient.active = {NetworkClient.active}");
            Debug.Log($"🌟 NetworkClient.localPlayer = {NetworkClient.localPlayer}");
            Debug.Log($"🌟 LocalInstance = {localPlayer}, isLocalPlayer = {(localPlayer != null && localPlayer.isLocalPlayer)}");

            if (localPlayer == null)
            {
                Debug.LogError("❌ PlayerController.LocalInstance is null，Cmd 无法发起！");
                return;
            }

            Debug.Log("📤 调用 CmdUseAndDrawCard()");
            localPlayer.CmdUseAndDrawCard(cardIndex);

            uint cellNetId = GetComponent<NetworkIdentity>().netId;
            uint playerNetId = localPlayer.netId;
            Debug.Log($"📤 调用 CmdRequestRevealTerminal(cellNetId={cellNetId}, playerNetId={playerNetId})");
            localPlayer.CmdRequestRevealTerminal(cellNetId, playerNetId);

            GameManager.Instance.ClearPendingCard();

            Debug.Log("📤 调用 CmdEndTurn()");
            localPlayer.CmdEndTurn();
            return;
        }

        // ❄️ 塑断卡
        if (pending.HasValue &&
            pending.Value.cardType == Card.CardType.Action &&
            pending.Value.toolEffect == "Collapse")
        {
            PlayerController.DebugClient($"🔥 尝试使用塑断卡在 ({state.row},{state.col})");
            GameManager.Instance.collapseManager.ApplyCollapseTo(GetComponent<MapCell>());
            var localPlayer = PlayerController.LocalInstance;
            localPlayer.CmdEndTurn();
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

        if (currentPlayer == null)
        {
            PlayerController.DebugClient("❌ LocalInstance 为空，无法出牌");
            return;
        }

        // 工具卡限制
        if (pending.Value.cardType == Card.CardType.Path &&
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

        // 邻居路径连接检查
        bool canConnect = false;
        var map = GameManager.Instance.mapGenerator.mapCells;

        if (state.row > 0)
        {
            var neighbor = map[state.row - 1, state.col]?.GetCard();
            if (neighbor != null && pending.Value.up && neighbor.down) canConnect = true;
        }
        if (state.row < map.GetLength(0) - 1)
        {
            var neighbor = map[state.row + 1, state.col]?.GetCard();
            if (neighbor != null && pending.Value.down && neighbor.up) canConnect = true;
        }
        if (state.col > 0)
        {
            var neighbor = map[state.row, state.col - 1]?.GetCard();
            if (neighbor != null && pending.Value.left && neighbor.right) canConnect = true;
        }
        if (state.col < map.GetLength(1) - 1)
        {
            var neighbor = map[state.row, state.col + 1]?.GetCard();
            if (neighbor != null && pending.Value.right && neighbor.left) canConnect = true;
        }

        if (!canConnect)
        {
            PlayerController.DebugClient($"❌ 放置失败：({state.row},{state.col}) 无法连接到邻居路径");
            return;
        }

        // ✅ 出路径卡请求
        int replacedIndex = GameManager.Instance.pendingCardIndex;

        currentPlayer.CmdRequestPlaceCard(
            GetComponent<NetworkIdentity>().netId,
            pending.Value.cardName,
            pending.Value.spriteName,
            pending.Value.toolEffect,
            pending.Value.cardType,
            pending.Value.up, pending.Value.down, pending.Value.left, pending.Value.right,
            pending.Value.blockedCenter,
            pending.Value.isPathPassable,
            replacedIndex);

        GameManager.Instance.ClearPendingCard();
        currentPlayer.CmdEndTurn();
    }
}