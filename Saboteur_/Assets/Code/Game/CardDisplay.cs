using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Image image;
    public Card cardData;

    public bool isSelected = false;

    public int cardIndex;

    // 初始化卡牌数据
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
        if (GameManager.Instance.hasGameEnded)
        {
            if (GameManager.Instance.endGameTip != null)
                GameManager.Instance.endGameTip.SetActive(true); // 显示提示
            Debug.Log("🛑 游戏结束，无法点击手牌");
            return;
        }

        // ✅ 严格限制：视角玩家必须是出牌玩家
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

        // 清除其他卡选中状态
        CardDisplay[] handCards = transform.parent.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay card in handCards)
        {
            if (card != this)
            {
                card.isSelected = false;
            }
        }
    }

}
