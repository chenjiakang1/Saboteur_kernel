using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// 负责根据同步的手牌列表在客户端生成/更新手牌 UI
/// </summary>
public class PlayerHandManager : MonoBehaviour
{
    [Header("Hand UI Prefabs and Containers")]
    public GameObject cardPrefab;
    public Transform cardParent;

    /// <summary>
    /// 清空当前手牌 UI，并根据给定的手牌数据生成新的卡牌
    /// </summary>
    /// <param name="handData">来自 PlayerController.hand 的同步列表</param>
    public void ShowHand(IList<CardData> handData)
    {
        // 1. 销毁旧卡牌
        for (int i = cardParent.childCount - 1; i >= 0; i--)
        {
            Destroy(cardParent.GetChild(i).gameObject);
        }

        // 2. 生成新卡牌
        for (int i = 0; i < handData.Count; i++)
        {
            CardData data = handData[i];
            Sprite sprite = GameManager.Instance.cardDeckManager.FindSpriteByName(data.spriteName);
            if (sprite == null)
            {
                Debug.LogWarning($"[PlayerHandManager] 未找到卡牌图片：{data.spriteName}");
                continue;
            }

            GameObject cardGO = Instantiate(cardPrefab, cardParent);
            cardGO.name = $"Card_{i}_{data.spriteName}";
            var display = cardGO.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.Init(data, sprite);
                display.cardIndex = i;
            }
            else
            {
                Debug.LogWarning("[PlayerHandManager] CardPrefab 缺少 CardDisplay 脚本！");
            }
        }
    }
}
