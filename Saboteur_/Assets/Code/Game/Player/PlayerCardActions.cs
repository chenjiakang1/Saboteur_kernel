using UnityEngine;
using Mirror;
using System.Collections.Generic;

public partial class PlayerController : NetworkBehaviour
{
    /// <summary>
    /// 使用探查卡：客户端点击终点格 → 服务端判断 → 客户端显示终点图像
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdRequestRevealTerminal(uint targetCellNetId, uint callerPlayerNetId)
    {
        Debug.Log($"📩 [CmdRequestRevealTerminal] 被调用，目标格子 netId = {targetCellNetId}, 调用者玩家 netId = {callerPlayerNetId}");

        if (!NetworkServer.spawned.TryGetValue(targetCellNetId, out var cellObj))
        {
            Debug.LogWarning("❌ 找不到终点格子对象");
            return;
        }

        MapCell cell = cellObj.GetComponent<MapCell>();
        if (cell == null)
        {
            Debug.LogWarning("❌ 终点格子没有 MapCell 脚本");
            return;
        }

        var state = cell.GetComponent<MapCellState>();
        if (state == null || !state.isBlocked)
        {
            Debug.LogWarning("❌ 此格子不是终点格");
            return;
        }

        // ✅ 用更可靠方式查找玩家对象
        if (!NetworkServer.spawned.TryGetValue(callerPlayerNetId, out var playerObj))
        {
            Debug.LogWarning("❌ 找不到调用者玩家对象！");
            return;
        }

        var callerConn = playerObj.connectionToClient;
        if (callerConn == null)
        {
            Debug.LogWarning("❌ 找到玩家对象但连接为空！");
            return;
        }

        Vector2Int pos = new Vector2Int(state.row, state.col);
        bool isGold = MapGenerator.LocalInstance.IsGoldAt(pos);
        string spriteName = isGold ? "Gold" : $"Rock_{Random.Range(0, GameManager.Instance.mapGenerator.rockSprites.Count)}";

        Debug.Log($"🎯 终点格是 {(isGold ? "金矿" : "石头")} → spriteName = {spriteName}");

        TargetRevealTerminalSprite(callerConn, targetCellNetId, spriteName);
    }


    /// <summary>
    /// 客户端本地揭示终点内容，仅发给使用探查卡的客户端
    /// </summary>
    [TargetRpc]
    public void TargetRevealTerminalSprite(NetworkConnection target, uint cellNetId, string spriteName)
    {
        Debug.Log($"🎯 [TargetRevealTerminalSprite] 调用：cellNetId = {cellNetId}, sprite = {spriteName}");

        if (!NetworkClient.spawned.TryGetValue(cellNetId, out var obj))
        {
            Debug.LogWarning("❌ TargetRevealTerminalSprite: 找不到目标 MapCell");
            return;
        }

        var cell = obj.GetComponent<MapCell>();
        Debug.Log("✅ 找到 MapCell，准备 Reveal");

        Sprite sprite = Resources.Load<Sprite>($"Images/{spriteName}");
        if (sprite != null)
        {
            Debug.Log($"👁️ 本地揭示终点：{spriteName}");
            cell.RevealTerminal(sprite);
        }
        else
        {
            Debug.LogWarning($"⚠️ 图片资源未找到：Images/{spriteName}");
        }
    }

    /// <summary>
    /// 通用卡牌使用：移除旧卡并补发一张新卡（Mirror 自动同步 hand）
    /// </summary>
    [Command]
    public void CmdUseAndDrawCard(int index)
    {
        Debug.Log($"🛠️ [CmdUseAndDrawCard] index={index}, hand.Count={hand.Count}");

        if (index >= 0 && index < hand.Count)
        {
            Debug.Log($"🗑️ 使用卡 index={index} → {hand[index].cardName}");
            hand.RemoveAt(index);

            var card = GameManager.Instance.cardDeckManager.DrawCard();
            if (card != null)
            {
                hand.Add(new CardData(card)); // ✅ Mirror 自动同步 → 客户端 OnHandChanged 刷新 UI
            }
            else
            {
                Debug.LogWarning("❗ 卡组已空，无法补牌");
            }
        }
        else
        {
            Debug.LogWarning("❌ 无效的卡片索引");
        }
    }
}