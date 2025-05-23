using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerHandManager : MonoBehaviour
{
    [Header("引用")]
    public GameObject cardPrefab;
    public Transform cardParent;

    public void ShowPlayerHandByIndex(int index)
    {
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);

        var allPlayers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
                         .OrderBy(p => p.netId).ToList();

        if (index < 0 || index >= allPlayers.Count)
        {
            Debug.LogWarning("❌ 玩家索引超出范围");
            return;
        }

        var player = allPlayers[index];
        var hand = player.syncCardSlots;

        for (int i = 0; i < hand.Count; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, cardParent);
            var display = cardGO.GetComponent<CardDisplay>();
            Sprite sprite = GameManager.Instance.cardDeckManager.FindSpriteByName(hand[i].spriteName);
            if (sprite == null)
            {
                Debug.LogWarning($"⚠️ 找不到卡牌图片 {hand[i].spriteName}");
                continue;
            }

            display.Init(hand[i], sprite); // ✅ 使用 CardData 初始化
            display.cardIndex = i;
        }
    }

    public void ShowLocalPlayerHand()
    {
        // Step 1: 清空旧手牌（如有）
        foreach (Transform child in cardParent)
            Destroy(child.gameObject);

        // Step 2: 检查 Network 状态
        if (NetworkClient.connection == null || NetworkClient.connection.identity == null)
        {
            Debug.LogWarning("❌ NetworkClient 或 identity 为空，无法获取本地玩家");
            return;
        }

        // Step 3: 获取本地玩家对象
        var localPlayer = NetworkClient.connection.identity.GetComponent<PlayerController>();
        if (localPlayer == null)
        {
            Debug.LogWarning("❌ 无法获取本地玩家 PlayerController");
            return;
        }

        Debug.Log($"🖐️ 正在生成本地玩家手牌，数量：{localPlayer.syncCardSlots.Count}");
        Debug.Log($"📦 cardParent is: {(cardParent != null ? cardParent.name : "❌ NULL")}");

        // Step 4: 遍历手牌生成卡片
        for (int i = 0; i < localPlayer.syncCardSlots.Count; i++)
        {
            var cardData = localPlayer.syncCardSlots[i];

            Sprite sprite = GameManager.Instance.cardDeckManager.FindSpriteByName(cardData.spriteName);
            if (sprite == null)
            {
                Debug.LogWarning($"⚠️ 未找到卡牌图片：{cardData.spriteName}");
                continue;
            }

            GameObject cardGO = Instantiate(cardPrefab, cardParent);
            Debug.Log($"✅ 成功生成手牌卡牌对象：{cardGO.name}");

            var display = cardGO.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.Init(cardData, sprite); // ✅ 使用 CardData 初始化，带有行为数据
                display.cardIndex = i;
            }
            else
            {
                Debug.LogWarning("⚠️ 生成的 CardPrefab 没有挂 CardDisplay 脚本！");
            }
        }

        Debug.Log("✅ 本地玩家手牌生成完毕");
    }
}
