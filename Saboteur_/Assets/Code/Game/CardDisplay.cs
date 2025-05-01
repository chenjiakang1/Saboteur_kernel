using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Image image;
    public Card cardData;

    // 记录是否被选中
    public bool isSelected = false;

    // 初始化卡牌数据
    public void Init(Card data, Sprite sprite)
    {
        cardData = data;
        image.sprite = sprite;
    }

    // 点击手牌
    public void OnClick()
    {
        // ✅ 新增: 不是当前玩家回合，禁止操作
        if (GameManager.Instance.playerID != TurnManager.Instance.currentPlayer)
        {
            Debug.Log("Not your turn!");
            return;
        }

        // 取消选中
        if (isSelected)
        {
            Debug.Log("Card deselected.");
            isSelected = false;
            GameManager.Instance.ClearPendingCard();
            return;
        }

        // 选中这张卡牌
        Debug.Log("Selected Card to Place: " + cardData);
        GameManager.Instance.SetPendingCard(cardData, image.sprite);
        isSelected = true;

        // 清除其他手牌的选中状态
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
