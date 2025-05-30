using Mirror;
using UnityEngine;

public partial class PlayerController
{
    [SyncVar]
    public bool isReady = false; // ✅ 默认必须为 false

    [Command]
    public void CmdToggleReady()
    {
        isReady = !isReady;
        Debug.Log($"📣 玩家准备状态切换：{playerName} → {(isReady ? "已准备" : "未准备")}");

        RpcUpdateReadyStatus(playerName, isReady);

        CustomNetworkManager networkManager = (CustomNetworkManager)NetworkManager.singleton;
        networkManager.CheckAllPlayersReady();

        // ⏬ 主动刷新所有客户端房间 UI 状态
        SendRoomInfoToAllClients();
    }


    [ClientRpc]
    void RpcUpdateReadyStatus(string playerId, bool ready)
    {
        var roomUI = FindFirstObjectByType<RoomUIManager>();
        if (roomUI != null)
        {
            roomUI.UpdatePlayerReadyUI(playerId, ready);
        }
    }
}

