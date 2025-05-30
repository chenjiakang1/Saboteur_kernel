using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public static class RoleAssigner
{
    public static void AssignRolesToPlayers(PlayerController[] players)
    {
        if (!NetworkServer.active) return;

        // ✅ 新增：判断当前是否为游戏场景
        if (!PlayerController.isGameplayEnabled)
        {
            Debug.Log("⚠ 当前不是游戏场景，跳过身份分发");
            return;
        }

        int totalPlayers = players.Length;
        int saboteurCount = GetSaboteurCount(totalPlayers);

        // 随机打乱顺序（避免固定分配）
        List<PlayerController> shuffled = players.OrderBy(p => Random.Range(0f, 1f)).ToList();

        for (int i = 0; i < shuffled.Count; i++)
        {
            shuffled[i].assignedRole = (i < saboteurCount) ? PlayerRole.Saboteur : PlayerRole.Miner;
            Debug.Log($"🎭 {shuffled[i].playerName} is assigned: {shuffled[i].assignedRole}");
        }
    }

    private static int GetSaboteurCount(int playerCount)
    {
        switch (playerCount)
        {
            case 2: return 1;
            case 3: return 1;
            case 4: return 1;
            case 5: return 2;
            case 6: return 2;
            case 7: return 3;
            case 8: return 3;
            case 9: return 3;
            case 10: return 4;
            default:
                Debug.LogWarning($"⚠ 不支持的玩家人数：{playerCount}，默认分配 1 个坏胚子");
                return 1;
        }
    }
}
