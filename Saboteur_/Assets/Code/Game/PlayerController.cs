using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance;

    [SyncVar] public string playerName;
    [SyncVar] public int gold;
    [SyncVar] public bool isMyTurn;
    [SyncVar] public bool hasPickaxe = true;
    [SyncVar] public bool hasMineCart = true;
    [SyncVar] public bool hasLamp = true;

    public readonly SyncList<CardData> syncCardSlots = new SyncList<CardData>();

    public override void OnStartLocalPlayer()
    {
        Debug.Log("[客户端] OnStartLocalPlayer 被调用");
        CmdInit("Player" + netId);
        Invoke(nameof(RefreshLocalHand), 0.2f);
    }

    public override void OnStartAuthority()
    {
        Debug.Log("✅ PlayerController 获得 authority 权限");
        LocalInstance = this;
    }

    private void RefreshLocalHand()
    {
        Debug.Log("[客户端] 调用 ShowLocalPlayerHand");
        GameManager.Instance.playerHandManager.ShowLocalPlayerHand();
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

        syncCardSlots.Clear();
        for (int i = 0; i < 5; i++)
        {
            var card = GameManager.Instance.cardDeckManager.DrawCard();
            if (card != null)
                syncCardSlots.Add(new CardData(card));
        }
    }

    [Command]
    public void CmdReplaceUsedCard(int index)
    {
        Debug.Log($"[服务端] 替换第 {index} 张手牌");
        if (index < 0 || index >= syncCardSlots.Count) return;
        var newCard = GameManager.Instance.cardDeckManager.DrawCard();
        if (newCard != null)
            syncCardSlots[index] = new CardData(newCard);
    }

    [Command]
    public void CmdRequestPlaceCard(uint cellNetId,
        string cardName, string spriteName, string toolEffect,
        Card.CardType cardType,
        bool up, bool down, bool left, bool right,
        bool blockedCenter, bool isPassable,
        int replacedIndex)
    {
        Debug.Log("[服务端] 收到 CmdRequestPlaceCard");

        if (!NetworkServer.spawned.TryGetValue(cellNetId, out NetworkIdentity identity))
        {
            Debug.LogWarning("[服务端] 找不到 CellNetId 对象: " + cellNetId);
            return;
        }

        var cell = identity.GetComponent<MapCell>();
        if (cell == null)
        {
            Debug.LogWarning("[服务端] 找不到 MapCell");
            return;
        }

        var state = cell.GetComponent<MapCellState>();
        if (state.isOccupied || state.isBlocked)
        {
            Debug.LogWarning("[服务端] Cell 不可用或已占用");
            return;
        }

        Debug.Log("[服务端] 成功识别 Cell，广播 ClientRpc");
        RpcBroadcastPlaceCard(cellNetId, cardName, spriteName, toolEffect,
                              cardType, up, down, left, right, blockedCenter, isPassable);

        cell.PlaceCardServer(cardName, spriteName, toolEffect, cardType,
                             up, down, left, right, blockedCenter, isPassable);

        CmdReplaceUsedCard(replacedIndex);
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
            Debug.LogWarning("[客户端] 无法找到 netId = " + cellNetId);
        }
    }

    // ✅ 客户端调用此方法发送调试信息到服务端（Host 控制台输出）
    public static void DebugClient(string msg)
    {
        if (LocalInstance != null)
        {
            LocalInstance.CmdSendDebug(msg);
        }
        else
        {
            Debug.LogWarning("❗ LocalInstance 为 null，无法发送调试信息：" + msg);
        }
    }

    [Command]
    public void CmdSendDebug(string msg)
    {
        Debug.Log($"🛰️ [Build客户端调试] {msg}");
    }
}
