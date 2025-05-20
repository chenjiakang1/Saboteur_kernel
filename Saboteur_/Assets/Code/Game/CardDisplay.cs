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
        //  判断是否是当前回合的玩家
        if (GameManager.Instance.playerID != TurnManager.Instance.currentPlayer)
        {
            Debug.LogWarning("⛔ 不是你的回合，不能出牌！");
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
