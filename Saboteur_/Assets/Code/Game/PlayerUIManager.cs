using System.Collections.Generic;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [Header("玩家UI预制体与显示面板")]
    public GameObject playerUIPrefab;            // 绑定你的 PlayerUI 预制体
    public Transform playerUIPanelParent;        // 绑定用于放置 UI 的父容器（带 VerticalLayoutGroup）

    /// <summary>
    /// 根据玩家数据列表，生成所有 UI 显示
    /// </summary>
    public void GenerateUI(List<PlayerData> players)
    {
        // 清空旧的 UI（可选）
        foreach (Transform child in playerUIPanelParent)
        {
            Destroy(child.gameObject);
        }

        // 生成每个玩家的 UI
        foreach (PlayerData player in players)
        {
            GameObject ui = Instantiate(playerUIPrefab, playerUIPanelParent);
            PlayerUI playerUI = ui.GetComponent<PlayerUI>();
            playerUI.SetPlayer(player);
        }
    }
}
