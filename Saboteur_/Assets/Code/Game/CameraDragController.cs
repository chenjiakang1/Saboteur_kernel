using Mirror;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerController
{
    [Command]
    public void CmdRequestPlaceCard(uint cellNetId, string cardName, string spriteName, string toolEffect,
        Card.CardType cardType, bool up, bool down, bool left, bool right, bool blockedCenter,
        bool isPathPassable, int handIndex)
    {
        Debug.Log("📦 [服务端] CmdRequestPlaceCard 被调用");

        if (cellNetId != 0 && NetworkServer.spawned.TryGetValue(cellNetId, out var identity))
        {
            var cell = identity.GetComponent<MapCell>();
            var state = cell.GetComponent<MapCellState>();
            if (state.isOccupied || state.isBlocked) return;

            RpcBroadcastPlaceCard(cellNetId, cardName, spriteName, toolEffect,
                cardType, up, down, left, right, blockedCenter, isPathPassable);

            cell.PlaceCardServer(cardName, spriteName, toolEffect, cardType,
                up, down, left, right, blockedCenter, isPathPassable);
        }

        if (handIndex >= 0 && handIndex < hand.Count)
        {
            hand.RemoveAt(handIndex);
            var newCard = GameManager.Instance.cardDeckManager.DrawCard();
            if (newCard != null)
                hand.Insert(handIndex, new CardData(newCard));
        }

        Object.FindFirstObjectByType<PathChecker>()?.CheckWinCondition();
    }

    [ClientRpc]
    public void RpcBroadcastPlaceCard(uint cellNetId, string cardName, string spriteName, string toolEffect,
        Card.CardType cardType, bool up, bool down, bool left, bool right,
        bool blockedCenter, bool isPassable)
    {
        if (NetworkClient.spawned.TryGetValue(cellNetId, out var identity))
        {
            var cell = identity.GetComponent<MapCell>();
            cell?.PlaceCardLocally(cardName, spriteName, toolEffect, cardType,
                up, down, left, right, blockedCenter, isPassable);
        }
    }

    [Command]
    public void CmdUseCollapseCardOnly(int handIndex)
    {
        if (handIndex < 0 || handIndex >= hand.Count) return;

        hand.RemoveAt(handIndex);

        var newCard = GameManager.Instance.cardDeckManager.DrawCard();
        if (newCard != null)
            hand.Insert(handIndex, new CardData(newCard));
    }

    [Command]
    public void CmdCollapseMapCell(uint cellNetId)
    {
        if (!NetworkServer.spawned.TryGetValue(cellNetId, out var identity)) return;
        var cell = identity.GetComponent<MapCell>();
        var state = cell.GetComponent<MapCellState>();

        state.card = null;
        state.isOccupied = false;

        RpcCollapseMapCell(cellNetId);
    }

    [ClientRpc]
    void RpcCollapseMapCell(uint cellNetId)
    {
        if (!NetworkClient.spawned.TryGetValue(cellNetId, out var identity)) return;
        var cell = identity.GetComponent<MapCell>();
        var ui = cell.GetComponent<MapCellUI>();
        var img = cell.GetComponent<Image>();

        if (ui.cardDisplay != null) Destroy(ui.cardDisplay.gameObject);
        ui.cardDisplay = null;

        if (img != null)
        {
            img.sprite = null;
            img.color = new Color32(0, 0, 0, 100);
        }
    }

    [Command]
    public void CmdApplyToolEffect(uint targetNetId, string effectName)
    {
        if (!NetworkServer.spawned.TryGetValue(targetNetId, out var identity)) return;
        var target = identity.GetComponent<PlayerController>();
        if (target == null) return;

        bool didApply = false;

        switch (effectName)
        {
            case "BreakLamp": target.hasLamp = false; didApply = true; break;
            case "BreakPickaxe": target.hasPickaxe = false; didApply = true; break;
            case "BreakMinecart": target.hasMineCart = false; didApply = true; break;
            case "RepairLamp": target.hasLamp = true; didApply = true; break;
            case "RepairPickaxe": target.hasPickaxe = true; didApply = true; break;
            case "RepairMinecart": target.hasMineCart = true; didApply = true; break;
            case "RepairPickaxeAndMinecart": target.hasPickaxe = true; target.hasMineCart = true; didApply = true; break;
            case "RepairPickaxeAndLamp": target.hasPickaxe = true; target.hasLamp = true; didApply = true; break;
            case "RepairMinecartAndLamp": target.hasMineCart = true; target.hasLamp = true; didApply = true; break;
        }

        if (didApply)
        {
            GameManager.Instance.playerUIManager.UpdateAllUI();
            RpcUpdateAllClientUI();
        }
    }

    [ClientRpc]
    void RpcUpdateAllClientUI()
    {
        GameManager.Instance.playerUIManager?.UpdateAllUI();
    }

    [Command]
    public void CmdSendDebug(string msg)
    {
        Debug.Log($"🛰️ [Build客户端调试] {msg}");
    }
}
