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
        //  禁止非当前玩家操作
        if (GameManager.Instance.playerID != TurnManager.Instance.currentPlayer)
        {
            Debug.Log("Not your turn!");
            return;
        }

        if (isSelected)
        {
            Debug.Log("Card deselected.");
            isSelected = false;
            GameManager.Instance.ClearPendingCard();
            return;
        }

        // ✅ 显示调试信息（名称 + 通路）
        Debug.Log("Selected Card to Place: " + cardData.cardName + " | " + cardData);

        GameManager.Instance.SetPendingCard(cardData, image.sprite, cardIndex);
        isSelected = true;

        // 清除其他选中卡
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
