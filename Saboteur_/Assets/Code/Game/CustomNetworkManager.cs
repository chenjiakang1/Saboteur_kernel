using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager
{
    public List<PlayerController> roomPlayers = new List<PlayerController>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);

        PlayerController pc = player.GetComponent<PlayerController>();
        pc.playerName = $"Player{conn.connectionId}";
        pc.isReady = false;
        roomPlayers.Add(pc);

        Debug.Log($"🧍 玩家加入房间：{pc.playerName}，当前人数 = {roomPlayers.Count}");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            PlayerController pc = conn.identity.GetComponent<PlayerController>();
            if (roomPlayers.Contains(pc))
            {
                roomPlayers.Remove(pc);
                Debug.Log($"❌ 玩家离开房间：{pc.playerName}，剩余人数 = {roomPlayers.Count}");
            }
        }

        base.OnServerDisconnect(conn);
    }

    public void CheckAllPlayersReady()
    {
        if (roomPlayers.Count == 0)
        {
            Debug.Log("⚠️ 房间中没有玩家");
            return;
        }

        foreach (var player in roomPlayers)
        {
            if (!player.isReady)
            {
                Debug.Log($"⏳ 玩家未准备：{player.playerName}");
                return;
            }
        }

        Debug.Log("✅ 所有玩家已准备 → 开始切换场景");
        ServerChangeScene("Game_Scene"); // 替换为你游戏的实际场景名称
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (sceneName == "Game_Scene") // 替换为你的游戏场景名称
        {
            Debug.Log("🚪 已进入游戏场景，启用游戏状态标记");

            // ✅ 启用游戏逻辑标记
            PlayerController.isGameplayEnabled = true;

            // ✅ 延迟初始化玩家（防止还未生成完）
            Invoke(nameof(InitAllPlayersForGame), 1.0f);
        }
    }

    /// <summary>
    /// 游戏场景初始化：发牌 + 注册回合系统
    /// </summary>
    private void InitAllPlayersForGame()
    {
        Debug.Log("🎴 正在初始化所有玩家数据并发牌...");

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            // ✅ 为每个玩家发手牌（服务端调用 Command）
            //player.CmdInit(player.playerName);

            // ✅ 注册进回合系统
            TurnManager.Instance?.RegisterPlayer(player);

            Debug.Log($"✅ 初始化完成：{player.playerName} (netId={player.netId})");
        }

        Debug.Log($"🌀 当前已初始化并注册的玩家总数 = {players.Length}");
    }
}
