using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class ScoreCardDrawFlow : NetworkBehaviour
{
    [Header("积分卡生成相关")]
    public ScoreCardDeckManager deckManager;       // 拖入 ScoreCardDeckManager
    public GameObject scoreCardPrefab;             // 拖入 ScoreCardDisplay 预制体
    public Transform scoreCardParent;              // 拖入 UI Grid 等父物体

    // ✅ 服务端调用，统一洗牌并抽卡（每人一张）
    [Server]
    public void StartDrawPhaseServer()
    {
        Debug.Log("🟢 [服务端] 开始积分卡抽取流程");

        deckManager.InitScoreDeck();

        List<ScoreCardData> drawnCards = new();

        // ✅ 获取玩家数量（动态决定抽几张卡）
        int numberOfPlayers = TurnManager.Instance != null ? TurnManager.Instance.GetPlayerCount() : 1;

        for (int i = 0; i < numberOfPlayers; i++)
        {
            var card = deckManager.DrawCard();
            var data = card.ToData();  // ✅ cardId 已在此生成
            drawnCards.Add(data);
        }

        // ✅ 广播所有客户端生成 UI
        RpcDistributeScoreCards(drawnCards.ToArray());
    }

    // ✅ 客户端生成积分卡 UI（由服务端广播调用）
    [ClientRpc]
    void RpcDistributeScoreCards(ScoreCardData[] cards)
    {
        Debug.Log($"📦 [客户端] 接收到 {cards.Length} 张积分卡 → 开始生成 UI");

        // 清空已有卡牌 UI
        foreach (Transform child in scoreCardParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            GameObject go = Instantiate(scoreCardPrefab, scoreCardParent);
            var display = go.GetComponent<ScoreCardDisplay>();
            display.cardIndex = i;

            // ✅ 加载图像资源
            Sprite sprite = LoadSprite(cards[i].spriteName);
            display.Init(cards[i], sprite);

            // ✅ 从服务端同步设置统一 cardId
            display.cardId = cards[i].cardId;
        }
    }

    // ✅ 用于加载积分卡图片
    private Sprite LoadSprite(string name)
    {
        Debug.Log($"🖼️ 正在加载图像：{name}");

        switch (name)
        {
            case "Gold1": return deckManager.gold1Sprite;
            case "Gold2": return deckManager.gold2Sprite;
            case "Gold3": return deckManager.gold3Sprite;
            default:
                Debug.LogWarning($"❌ 未找到名为 {name} 的图像");
                return null;
        }
    }

    // ✅ 点击卡片时通知逻辑层（UI 高亮等）
    public void OnCardSelected(ScoreCardDisplay display)
    {
        Debug.Log($"✅ 玩家点击积分卡：{display.data.cardName}（分数：{display.data.scoreValue}）");

        // 禁用点击按钮
        display.GetComponent<UnityEngine.UI.Button>().interactable = false;

        // 高亮边框（可选）
        var outline = display.GetComponent<UnityEngine.UI.Outline>();
        if (outline != null)
            outline.enabled = true;
    }

    // ✅ 服务端 → 所有客户端广播销毁指定卡牌
    [ClientRpc]
    public void RpcDestroyCardById(string id)
    {
        var allCards = FindObjectsByType<ScoreCardDisplay>(FindObjectsSortMode.None);
        foreach (var card in allCards)
        {
            if (card.cardId == id)
            {
                Debug.Log($"🗑️ 客户端销毁卡牌 ID={id}");
                Destroy(card.gameObject);
                break;
            }
        }
    }
}
