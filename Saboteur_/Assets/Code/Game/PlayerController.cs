using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance;

    [SyncVar] public string playerName;
    [SyncVar] public int gold;
    [SyncVar] public bool isMyTurn;
    [SyncVar] public bool hasPickaxe = true;
    [SyncVar] public bool hasMineCart = true;
    [SyncVar] public bool hasLamp = true;

    public readonly SyncList<CardData> hand = new SyncList<CardData>();

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("[客户端] OnStartLocalPlayer");
        CmdInit("Player" + netId);
        hand.Callback += OnHandChanged;
        GameManager.Instance.playerHandManager.ShowHand(hand);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        LocalInstance = this;
        Debug.Log("[客户端] 获得 authority 权限");
    }

    private void OnHandChanged(SyncList<CardData>.Operation op, int index, CardData oldItem, CardData newItem)
    {
        if (!isLocalPlayer) return;
        Debug.Log($"[客户端] 手牌列表变更({op}) → 刷新 UI");
        GameManager.Instance.playerHandManager.ShowHand(hand);
    }

    [Command]
    public void CmdInit(string name)
    {
        Debug.Log("[服务端] 执行 CmdInit: " + name);
        playerName = name;
        gold = 0;
        isMyTurn = false;
        hasPickaxe = true;
        hasLamp = true;
        hasMineCart = true;

        hand.Clear();
        for (int i = 0; i < 5; i++)
        {
            var card = GameManager.Instance.cardDeckManager.DrawCard();
            if (card != null)
                hand.Add(new CardData(card));
        }
    }

    [Command]
    public void CmdRequestPlaceCard(uint cellNetId,
        string cardName, string spriteName, string toolEffect,
        Card.CardType cardType,
        bool up, bool down, bool left, bool right,
        bool blockedCenter,
        bool isPathPassable,
        int handIndex)
    {
        Debug.Log("[服务端] 收到 CmdRequestPlaceCard");
        if (!NetworkServer.spawned.TryGetValue(cellNetId, out NetworkIdentity identity))
        {
            Debug.LogWarning("[服务端] 找不到 CellNetId: " + cellNetId);
            return;
        }
        var cell = identity.GetComponent<MapCell>();
        var state = cell.GetComponent<MapCellState>();
        if (state.isOccupied || state.isBlocked)
        {
            Debug.LogWarning("[服务端] Cell 不可用或已占用");
            return;
        }

        RpcBroadcastPlaceCard(cellNetId, cardName, spriteName, toolEffect,
                              cardType, up, down, left, right, blockedCenter, isPathPassable);

        cell.PlaceCardServer(cardName, spriteName, toolEffect, cardType,
                             up, down, left, right, blockedCenter, isPathPassable);

        Object.FindFirstObjectByType<PathChecker>()?.CheckWinCondition();

        if (handIndex >= 0 && handIndex < hand.Count)
        {
            hand.RemoveAt(handIndex);
            var newCard = GameManager.Instance.cardDeckManager.DrawCard();
            if (newCard != null)
                hand.Insert(handIndex, new CardData(newCard));
        }
        else
        {
            Debug.LogWarning("[服务端] handIndex 超出范围: " + handIndex);
        }
    }

    [ClientRpc]
    public void RpcBroadcastPlaceCard(uint cellNetId,
        string cardName, string spriteName, string toolEffect,
        Card.CardType cardType,
        bool up, bool down, bool left, bool right,
        bool blockedCenter, bool isPassable)
    {
        Debug.Log("[客户端] 执行 RpcBroadcastPlaceCard");
        if (NetworkClient.spawned.TryGetValue(cellNetId, out NetworkIdentity identity))
        {
            var cell = identity.GetComponent<MapCell>();
            cell?.PlaceCardLocally(cardName, spriteName, toolEffect, cardType,
                                   up, down, left, right, blockedCenter, isPassable);
        }
        else
        {
            Debug.LogWarning("[客户端] 找不到 netId = " + cellNetId);
        }
    }

    [Command]
    public void CmdSendDebug(string msg)
    {
        Debug.Log($"🛰️ [Build客户端调试] {msg}");
    }

    [Command]
    public void CmdUseCollapseCardOnly(int handIndex)
    {
        Debug.Log($"[服务端] 使用塌方卡，仅移除手牌 index = {handIndex}");

        if (handIndex < 0 || handIndex >= hand.Count)
        {
            Debug.LogWarning("[服务端] handIndex 越界，忽略操作");
            return;
        }

        hand.RemoveAt(handIndex);

        var newCard = GameManager.Instance.cardDeckManager.DrawCard();
        if (newCard != null)
        {
            hand.Insert(handIndex, new CardData(newCard));
            Debug.Log("[服务端] 塌方卡使用成功，补发新卡");
        }
        else
        {
            Debug.Log("[服务端] 塌方卡使用成功，但牌堆为空，不再补牌");
        }
    }

    [Command]
    public void CmdCollapseMapCell(uint cellNetId)
    {
        if (!NetworkServer.spawned.TryGetValue(cellNetId, out NetworkIdentity identity)) return;
        var cell = identity.GetComponent<MapCell>();
        var state = cell.GetComponent<MapCellState>();

        state.card = null;
        state.isOccupied = false;

        RpcCollapseMapCell(cellNetId);
    }

    [ClientRpc]
    void RpcCollapseMapCell(uint cellNetId)
    {
        if (!NetworkClient.spawned.TryGetValue(cellNetId, out NetworkIdentity identity)) return;
        var cell = identity.GetComponent<MapCell>();
        var ui = cell.GetComponent<MapCellUI>();
        var img = cell.GetComponent<Image>();

        if (ui.cardDisplay != null)
        {
            Destroy(ui.cardDisplay.gameObject);
            ui.cardDisplay = null;
        }

        if (img != null)
        {
            img.sprite = null;
            img.color = new Color32(0, 0, 0, 100);
        }
    }
    public static void DebugClient(string msg)
    {
        if (LocalInstance != null)
            LocalInstance.CmdSendDebug(msg);
        else
            Debug.LogWarning("❗ LocalInstance 为 null，无法发送调试信息：" + msg);
    }
    
}