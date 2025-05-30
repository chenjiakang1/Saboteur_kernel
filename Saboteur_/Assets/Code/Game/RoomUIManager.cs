using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System.Collections.Generic;

public class RoomUIManager : MonoBehaviour
{
    [Header("UI 绑定")]
    public Button readyButton;
    public Transform playerStatusParent; // 容器（推荐使用 Horizontal/Vertical Layout Group）
    public GameObject playerStatusPrefab; // PlayerStatusUI 预制体

    // 存储玩家ID与对应UI组件
    private Dictionary<string, PlayerStatusUI> playerStatusDict = new();

    void Start()
    {
        readyButton.onClick.AddListener(OnReadyClicked);

        // 兼容旧的同步逻辑：延迟刷新一次，确保兼容性
        Invoke(nameof(RefreshAllPlayerStatus), 1f);
    }

    /// <summary>
    /// 本地玩家点击准备按钮时触发
    /// </summary>
    void OnReadyClicked()
    {
        if (NetworkClient.connection?.identity != null)
        {
            var player = NetworkClient.connection.identity.GetComponent<PlayerController>();
            player.CmdToggleReady();

            // 本地立即更新 UI（SyncVar 有延迟）
            if (playerStatusDict.TryGetValue(player.playerName, out var ui))
            {
                ui.UpdateReadyStatus(!player.isReady); // 取反代表即将状态
            }
        }
        else
        {
            Debug.LogWarning("❗ 无法获取本地玩家身份，可能连接未完成。");
        }
    }

    /// <summary>
    /// 服务端通过 TargetRpc 主动分发的房间 UI 初始化
    /// </summary>
    public void RebuildPlayerUI(string[] ids, bool[] readies)
    {
        playerStatusDict.Clear();

        // 清空旧的 UI 项
        foreach (Transform child in playerStatusParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < ids.Length; i++)
        {
            string playerId = ids[i];
            bool isReady = readies[i];

            var uiObj = Instantiate(playerStatusPrefab, playerStatusParent);
            var ui = uiObj.GetComponent<PlayerStatusUI>();
            ui.SetInfo(playerId, isReady);
            playerStatusDict[playerId] = ui;
        }

        Debug.Log($"✅ Room UI 已更新，共 {ids.Length} 位玩家");
    }

    /// <summary>
    /// 旧逻辑：遍历场景中 PlayerController，刷新 UI
    /// </summary>
    public void RefreshAllPlayerStatus()
    {
        playerStatusDict.Clear();

        foreach (Transform child in playerStatusParent)
        {
            Destroy(child.gameObject);
        }

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            string playerId = player.playerName;
            bool isReady = player.isReady;

            var uiObj = Instantiate(playerStatusPrefab, playerStatusParent);
            var ui = uiObj.GetComponent<PlayerStatusUI>();
            ui.SetInfo(playerId, isReady);
            playerStatusDict[playerId] = ui;
        }

        Debug.Log($"🔄 RefreshAllPlayerStatus()：遍历场景，共 {players.Length} 位玩家");
    }

    /// <summary>
    /// 单独更新某位玩家的准备状态（由 SyncVar/Rpc 调用）
    /// </summary>
    public void UpdatePlayerReadyUI(string playerId, bool isReady)
    {
        if (playerStatusDict.TryGetValue(playerId, out var ui))
        {
            ui.UpdateReadyStatus(isReady);
        }
    }
}
