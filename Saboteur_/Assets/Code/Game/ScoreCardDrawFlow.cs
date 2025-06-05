using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class ScoreCardDrawFlow : NetworkBehaviour
{
    [Header("积分卡生成相关")]
    public ScoreCardDeckManager deckManager;       // 拖入 ScoreCardDeckManager
    public GameObject scoreCardPrefab;             // 拖入 ScoreCardDisplay 预制体
    public Transform scoreCardParent;              // 拖入 UI Grid 等父物体

    private int numberOfCardsToDraw = 5;

    // ✅ 服务端调用，统一洗牌并抽卡
    [Server]
    public void StartDrawPhaseServer()
    {
        Debug.Log("🟢 [服务端] 开始积分卡抽取流程");

        deckManager.InitScoreDeck();

        List<ScoreCardData> drawnCards = new();

        for (int i = 0; i < numberOfCardsToDraw; i++)
        {
            var card = deckManager.DrawCard();
            var data = card.ToData();            // ✅ 已在 ScoreCardData 中生成 cardId
            drawnCards.Add(data);
        }

        RpcDistributeScoreCards(drawnCards.ToArray());
    }

    // ✅ 客户端接收分发：显示卡牌 UI
    [ClientRpc]
    void RpcDistributeScoreCards(ScoreCardData[] cards)
    {
        Debug.Log($"📦 [客户端] 接收到 {cards.Length} 张积分卡 → 开始生成 UI");

        // 清空原有 UI
        foreach (Transform child in scoreCardParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            GameObject go = Instantiate(scoreCardPrefab, scoreCardParent);
            var display = go.GetComponent<ScoreCardDisplay>();
            display.cardIndex = i;

            // ✅ 用 spriteName 加载图片
            Sprite sprite = LoadSprite(cards[i].spriteName);
            display.Init(cards[i], sprite);

            // ✅ 设置服务端生成的 cardId（关键！）
            display.cardId = cards[i].cardId;
        }
    }

    /// <summary>
    /// 客户端根据 sprite 名称加载图像资源
    /// </summary>
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

    /// <summary>
    /// 玩家点击卡牌后调用（UI 高亮等）
    /// </summary>
    public void OnCardSelected(ScoreCardDisplay display)
    {
        Debug.Log($"✅ 你点击了积分卡：{display.data.cardName}（分数：{display.data.scoreValue}）");

        // 禁用点击
        display.GetComponent<UnityEngine.UI.Button>().interactable = false;

        // 高亮（可选）
        var outline = display.GetComponent<UnityEngine.UI.Outline>();
        if (outline != null)
            outline.enabled = true;
    }

    /// <summary>
    /// 服务端 → 广播销毁指定卡牌 ID，所有客户端执行
    /// </summary>
    [ClientRpc]
    public void RpcDestroyCardById(string id)
    {
        var allCards = FindObjectsByType<ScoreCardDisplay>(FindObjectsSortMode.None);
        foreach (var card in allCards)
        {
            if (card.cardId == id)
            {
                Debug.Log($"🗑️ 销毁卡牌 ID={id}");
                Destroy(card.gameObject);
                break;
            }
        }
    }
}
