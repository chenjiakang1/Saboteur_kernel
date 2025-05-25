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
    [SyncVar(hook = nameof(OnPickaxeChanged))] public bool hasPickaxe = true;
    [SyncVar(hook = nameof(OnMinecartChanged))] public bool hasMineCart = true;
    [SyncVar(hook = nameof(OnLampChanged))] public bool hasLamp = true;

    public readonly SyncList<CardData> hand = new SyncList<CardData>();

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[本地玩家] 我的名字是：{playerName}，netId = {netId}");

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

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[客户端] OnStartClient 被调用，netId = {netId}");

        // 所有客户端（包括 Host 和 Client）进入时尝试生成 UI
        Invoke(nameof(GenerateUIWithDelay), 1.0f);
    }

    private void GenerateUIWithDelay()
    {
        if (GameManager.Instance?.playerUIManager != null)
        {
            Debug.Log("[客户端] 延迟调用 → 生成/刷新所有玩家 UI");
            GameManager.Instance.playerUIManager.GenerateUI();
        }
        else
        {
            Debug.LogWarning("⚠️ 无法访问 GameManager 或 UI 管理器，UI 未刷新");
        }
    }

    private void OnHandChanged(SyncList<CardData>.Operation op, int index, CardData oldItem, CardData newItem)
    {
        if (this != PlayerController.LocalInstance) return;

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

        if (cellNetId != 0)
        {
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
        }

        // ✅ 不论是否是地图卡，只要用了就移除手牌
        if (handIndex >= 0 && handIndex < hand.Count)
        {
            Debug.Log($"✅ 从手牌中移除卡片 index={handIndex} → {cardName}");
            hand.RemoveAt(handIndex);
            var newCard = GameManager.Instance.cardDeckManager.DrawCard();
            if (newCard != null)
                hand.Insert(handIndex, new CardData(newCard));
        }
        else
        {
            Debug.LogWarning($"❌ handIndex 越界或无效: {handIndex}");
        }
        Object.FindFirstObjectByType<PathChecker>()?.CheckWinCondition();
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

    [Command]
    public void CmdApplyToolEffect(uint targetNetId, string effectName)
    {
        if (!NetworkServer.spawned.TryGetValue(targetNetId, out var identity)) return;
        var target = identity.GetComponent<PlayerController>();
        if (target == null) return;

        bool didApply = false;

        switch (effectName)
        {
            case "BreakLamp":
                if (target.hasLamp) { target.hasLamp = false; didApply = true; }
                break;
            case "BreakPickaxe":
                if (target.hasPickaxe) { target.hasPickaxe = false; didApply = true; }
                break;
            case "BreakMinecart":
                if (target.hasMineCart) { target.hasMineCart = false; didApply = true; }
                break;
            case "RepairLamp":
                if (!target.hasLamp) { target.hasLamp = true; didApply = true; }
                break;
            case "RepairPickaxe":
                if (!target.hasPickaxe) { target.hasPickaxe = true; didApply = true; }
                break;
            case "RepairMinecart":
                if (!target.hasMineCart) { target.hasMineCart = true; didApply = true; }
                break;
            case "RepairPickaxeAndMinecart":
                if (!target.hasPickaxe) { target.hasPickaxe = true; didApply = true; }
                if (!target.hasMineCart) { target.hasMineCart = true; didApply = true; }
                break;
            case "RepairPickaxeAndLamp":
                if (!target.hasPickaxe) { target.hasPickaxe = true; didApply = true; }
                if (!target.hasLamp) { target.hasLamp = true; didApply = true; }
                break;
            case "RepairMinecartAndLamp":
                if (!target.hasMineCart) { target.hasMineCart = true; didApply = true; }
                if (!target.hasLamp) { target.hasLamp = true; didApply = true; }
                break;
        }

        if (didApply)
        {
            GameManager.Instance.playerUIManager.UpdateAllUI(); // 本地服务端也刷新
            RpcUpdateAllClientUI(); // 通知所有客户端刷新
        }
    }


    [ClientRpc]
    void RpcUpdateAllClientUI()
    {
        GameManager.Instance.playerUIManager?.UpdateAllUI();
    }

    private void OnPickaxeChanged(bool oldValue, bool newValue)
    {
        GameManager.Instance?.playerUIManager?.UpdateAllUI();
    }


    private void OnMinecartChanged(bool oldValue, bool newValue)
    {
        GameManager.Instance?.playerUIManager?.UpdateAllUI();
    }


    private void OnLampChanged(bool oldValue, bool newValue)
    {
        GameManager.Instance?.playerUIManager?.UpdateAllUI();
    }

}
