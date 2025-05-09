using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Image image;
    public Card cardData;

    public bool isSelected = false;

    // 初始化卡牌数据
    public void Init(Card data, Sprite sprite = null)
    {
        cardData = data;

        // 从 cardData 读取 sprite，如果没传就默认
        if (cardData.sprite != null)
            image.sprite = cardData.sprite;
        else if (sprite != null)
            image.sprite = sprite;
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

        GameManager.Instance.SetPendingCard(cardData, image.sprite);
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
