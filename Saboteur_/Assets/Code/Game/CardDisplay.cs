using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Image image;
    public Card cardData;

    public bool isSelected = false;
    public int cardIndex;

    public void Init(Card data, Sprite sprite = null)
    {
        cardData = data;

        if (cardData != null && cardData.sprite != null)
            image.sprite = cardData.sprite;
        else if (sprite != null)
            image.sprite = sprite;
        else
            image.sprite = null;
    }

    public void OnClick()
    {
        // ✅ 处理塌方卡点击地图中已有路径卡：允许销毁
        if (transform.parent != GameManager.Instance.cardParent)
        {
            if (GameManager.Instance.pendingCard != null &&
                GameManager.Instance.pendingCard.cardType == Card.CardType.Action &&
                GameManager.Instance.pendingCard.toolEffect == "Collapse")
            {
                MapCell cell = GetComponentInParent<MapCell>();
                if (cell != null)
                {
                    GameManager.Instance.ApplyCollapseTo(cell);
                    return;
                }
            }

            // ❌ 非塌方卡点击地图卡牌禁止操作
            Debug.Log("⛔ 地图中的卡牌不可直接点击操作（仅支持塌方）！");
            return;
        }

        if (GameManager.Instance.hasGameEnded)
        {
            if (GameManager.Instance.endGameTip != null)
                GameManager.Instance.endGameTip.SetActive(true);
            Debug.Log("🛑 游戏结束，无法点击手牌");
            return;
        }

        if (GameManager.Instance.viewPlayerID != GameManager.Instance.playerID)
        {
            Debug.LogWarning("⛔ 当前不是你的出牌视角，无法操作卡牌！");
            return;
        }

        if (isSelected)
        {
            isSelected = false;
            GameManager.Instance.ClearPendingCard();
            return;
        }

        GameManager.Instance.SetPendingCard(cardData, image.sprite, cardIndex);
        isSelected = true;

        // ✅ 处理破坏类道具卡
        if (cardData.cardType == Card.CardType.Tool && cardData.toolEffect.StartsWith("Break"))
        {
            Debug.Log("💥 使用破坏工具卡，选择目标玩家");
            GameManager.Instance.ShowBreakToolPanel(cardData.toolEffect, cardIndex);
            return;
        }

        // ✅ 处理恢复类道具卡
        if (cardData.cardType == Card.CardType.Tool && cardData.toolEffect.StartsWith("Repair"))
        {
            Debug.Log("🔧 使用修复工具卡，选择目标玩家");
            GameManager.Instance.ShowRepairToolPanel(cardData.toolEffect, cardIndex);
            return;
        }

        // 切换手牌选中状态
        CardDisplay[] handCards = transform.parent.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay card in handCards)
        {
            if (card != this)
                card.isSelected = false;
        }
    }
}