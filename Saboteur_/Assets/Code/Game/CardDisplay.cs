using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class CardDisplay : MonoBehaviour
{
    public Image image;
    public Card cardData;
    public TextMeshProUGUI nameText;

    public bool isSelected = false;
    public int cardIndex;

    // 初始化方式 1：使用 Card + sprite
    public void Init(Card data, Sprite sprite = null)
    {
        cardData = data;
        image.sprite = sprite ?? cardData?.sprite;
    }

    // 初始化方式 2：仅展示卡名和图片（如终点卡）
    public void Init(string cardName, Sprite sprite)
    {
        image.sprite = sprite;
        if (nameText != null)
            nameText.text = cardName;
    }

    // 初始化方式 3：使用 CardData + sprite，支持网络同步结构
    public void Init(CardData data, Sprite sprite)
    {
        cardData = new Card(data);
        cardData.sprite = sprite;
        image.sprite = sprite;

        if (nameText != null)
            nameText.text = data.cardName;
    }

    public void OnClick()
    {
        Debug.Log($"🟡【点击卡牌】Index: {cardIndex}, Name: {cardData?.cardName ?? "NULL"}");

        if (cardData == null)
        {
            Debug.LogWarning("⚠️ cardData 为 null！");
            return;
        }

        // 地图中的卡牌不可点击（支持塌方卡特殊操作）
        if (transform.parent != GameManager.Instance.cardParent)
        {
            var pending = GameManager.Instance.pendingCard;

            if (pending.HasValue &&
                pending.Value.cardType == Card.CardType.Action &&
                pending.Value.toolEffect == "Collapse")
            {
                MapCell cell = GetComponentInParent<MapCell>();
                if (cell != null)
                {
                    GameManager.Instance.collapseManager.ApplyCollapseTo(cell);
                    return;
                }
            }

            Debug.Log("⛔ 地图卡牌不可点击操作（仅支持塌方）！");
            return;
        }

        if (GameManager.Instance.gameStateManager.hasGameEnded)
        {
            GameManager.Instance.endGameTip?.SetActive(true);
            Debug.Log("🛑 游戏结束，无法操作手牌");
            return;
        }

        // 再次点击取消选中
        if (isSelected)
        {
            isSelected = false;
            GameManager.Instance.ClearPendingCard();
            return;
        }

        // 设置为选中手牌（不会消耗卡牌）
        GameManager.Instance.SetPendingCard(new CardData(cardData), image.sprite, cardIndex);
        isSelected = true;

        // 清除其他卡牌的选中状态
        CardDisplay[] handCards = transform.parent.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay card in handCards)
        {
            if (card != this)
                card.isSelected = false;
        }

        // 工具卡特殊交互
        if (cardData.cardType == Card.CardType.Tool)
        {
            if (cardData.toolEffect.StartsWith("Break"))
            {
                Debug.Log("💥 使用破坏工具卡，选择目标玩家");
                GameManager.Instance.toolEffectManager.ShowBreakToolPanel(cardData.toolEffect, cardIndex);
                return;
            }
            if (cardData.toolEffect.StartsWith("Repair"))
            {
                Debug.Log("🔧 使用修复工具卡，选择目标玩家");
                GameManager.Instance.toolEffectManager.ShowRepairToolPanel(cardData.toolEffect, cardIndex);
                return;
            }
        }

        // ✅ 取消：此处不应替换卡牌！
        // ✅ 替换应由 MapCell.cs 控制：放置卡片成功后，再真正替换手牌
    }
}
