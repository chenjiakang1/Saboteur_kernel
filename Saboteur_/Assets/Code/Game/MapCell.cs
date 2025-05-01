using UnityEngine;

public class MapCell : MonoBehaviour
{
    public bool isOccupied = false;
    public bool isBlocked = false;  // ✅ 新增：是否禁止放置（特殊格子用）

    public void OnClick()
    {
        if (isBlocked)
        {
            Debug.Log("This cell is blocked!");
            return;
        }

        if (isOccupied)
        {
            Debug.Log("Already occupied");
            return;
        }

        Card card = GameManager.Instance.pendingCard;
        Sprite sprite = GameManager.Instance.pendingSprite;

        // 没有准备卡牌
        if (card == null || sprite == null)
        {
            Debug.Log("No card selected to place!");
            return;
        }

        // 放置卡牌
        GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, transform);
        cardGO.GetComponent<CardDisplay>().Init(card, sprite);

        // 让卡牌填满格子
        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        isOccupied = true;

        // 清除准备状态
        GameManager.Instance.ClearPendingCard();

        // 删除手牌区中已选中的卡牌
        CardDisplay[] handCards = GameManager.Instance.cardParent.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay cardInHand in handCards)
        {
            if (cardInHand.isSelected)
            {
                Destroy(cardInHand.gameObject);
                break;
            }
        }

        // ✅ 切换到下一个玩家
        TurnManager.Instance.NextTurn();
    }
}
