using System.Linq;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [Header("玩家UI预制体与显示面板")]
    public GameObject playerUIPrefab;            // 绑定你的 PlayerUI 预制体
    public Transform playerUIPanelParent;        // 用于放置 UI 的父容器

    /// <summary>
    /// 联机模式下自动查找所有玩家，生成对应的 UI
    /// </summary>
    public void GenerateUI()
    {
        // 清空旧的 UI
        foreach (Transform child in playerUIPanelParent)
        {
            Destroy(child.gameObject);
        }

        // 查找所有联机玩家（按 netId 排序）
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
               .OrderBy(p => p.netId)
               .ToList();

        foreach (var player in players)
        {
            GameObject ui = Instantiate(playerUIPrefab, playerUIPanelParent);
            PlayerUI playerUI = ui.GetComponent<PlayerUI>();
            playerUI.SetPlayer(player);
        }
    }

    /// <summary>
    /// 手动刷新所有玩家 UI 状态
    /// </summary>
    public void UpdateAllUI()
    {
        PlayerUI[] allUI = playerUIPanelParent.GetComponentsInChildren<PlayerUI>();
        foreach (var ui in allUI)
        {
            ui.UpdateUI();
        }
    }
}
