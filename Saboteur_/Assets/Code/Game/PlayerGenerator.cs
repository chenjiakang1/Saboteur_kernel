using System.Collections.Generic;
using UnityEngine;

public class PlayerGenerator : MonoBehaviour
{
    public int playerCount = 4;
    public List<PlayerData> allPlayers = new List<PlayerData>();

    public void GeneratePlayers(List<Card> fullDeck)
    {
        allPlayers.Clear();

        int totalNeededCards = playerCount * 5;

        if (fullDeck.Count < totalNeededCards)
        {
            Debug.LogError($" 卡牌不足：仅有 {fullDeck.Count} 张卡，但需要 {totalNeededCards} 张发牌！");
            return;
        }

        for (int i = 0; i < playerCount; i++)
        {
            PlayerData player = new PlayerData
            {
                Name = $"Player {i + 1}",
                Gold = 0,
                IsMyTurn = (i == 0),
                CardSlots = new Card[5]
            };

            for (int j = 0; j < 5; j++)
            {
                player.CardSlots[j] = fullDeck[0];     // 抽第一张
                fullDeck.RemoveAt(0);                  // ✅ 移除已抽的卡
            }

            allPlayers.Add(player);
        }

        Debug.Log($"✅ 成功创建 {allPlayers.Count} 位玩家，每人获得 5 张牌。剩余卡组：{fullDeck.Count} 张");
    }
}
