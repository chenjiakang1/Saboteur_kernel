using UnityEngine;
using UnityEngine.UI;

public class MapCell : MonoBehaviour
{
    public bool isOccupied = false;   // 是否已被放置卡牌
    public bool isBlocked = false;    // 是否为禁止放置格子
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    // ✅ 设置禁用格子（特殊格子用，如Origin、Terminus等）
    public void SetBlocked(Sprite blockSprite)
    {
        isBlocked = true;

        if (image != null)
        {
            image.sprite = blockSprite;
            image.color = Color.white;
        }
    }

    // ✅ 点击格子逻辑
    public void OnClick()
    {
        // 禁用格子不能放置
        if (isBlocked)
        {
            Debug.Log("This cell is blocked!");
            return;
        }

        // 已经被占用
        if (isOccupied)
        {
            Debug.Log("Already occupied");
            return;
        }

        Card card = GameManager.Instance.pendingCard;
        Sprite sprite = GameManager.Instance.pendingSprite;

        // 没选卡牌
        if (card == null || sprite == null)
        {
            Debug.Log("No card selected to place!");
            return;
        }

        // ✅ 放置卡牌
        GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, transform);
        cardGO.GetComponent<CardDisplay>().Init(card, sprite);

        // 填满整个格子
        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        isOccupied = true;

        // ✅ 清空准备卡牌
        GameManager.Instance.ClearPendingCard();

        // ✅ 删除手牌区中被选中的卡牌
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
