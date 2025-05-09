using UnityEngine;
using UnityEngine.UI;

public class MapCell : MonoBehaviour
{
    public bool isOccupied = false;
    public bool isBlocked = false;
    public int row, col;

    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetBlocked(Sprite sprite)
    {
        isBlocked = true;
        image.sprite = sprite;
        image.color = Color.white;
    }

    public void OnClick()
    {
        if (isBlocked)
        {
            Debug.Log("This cell is blocked and cannot be used.");
            return;
        }

        if (isOccupied)
        {
            Debug.Log("This cell is already occupied.");
            return;
        }

        Card card = GameManager.Instance.pendingCard;
        Sprite sprite = GameManager.Instance.pendingSprite;

        if (card == null || sprite == null)
        {
            Debug.Log("No card is selected for placement.");
            return;
        }

        //  检查是否能连接到邻居
        bool canConnect = false;
        MapCell[,] map = GameManager.Instance.mapGenerator.mapCells;
        int maxRow = map.GetLength(0);
        int maxCol = map.GetLength(1);

        if (row > 0)
        {
            MapCell neighbor = map[row - 1, col];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && card.up && neighborCard.down)
                canConnect = true;
        }

        if (row < maxRow - 1)
        {
            MapCell neighbor = map[row + 1, col];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && card.down && neighborCard.up)
                canConnect = true;
        }

        if (col > 0)
        {
            MapCell neighbor = map[row, col - 1];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && card.left && neighborCard.right)
                canConnect = true;
        }

        if (col < maxCol - 1)
        {
            MapCell neighbor = map[row, col + 1];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && card.right && neighborCard.left)
                canConnect = true;
        }

        if (!canConnect)
        {
            Debug.LogWarning("Invalid placement: This card cannot connect to any neighboring card.");
            return;
        }

        // ✅ 放置卡牌到地图格
        GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, transform);
        cardGO.GetComponent<CardDisplay>().Init(card, sprite);

        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        isOccupied = true;

        GameManager.Instance.ClearPendingCard();

        // 获取手牌中被选中的那一张
        CardDisplay selectedCard = null;
        CardDisplay[] handCards = GameManager.Instance.cardParent.GetComponentsInChildren<CardDisplay>();
        foreach (CardDisplay cardInHand in handCards)
        {
            if (cardInHand.isSelected)
            {
                selectedCard = cardInHand;
                break;
            }
        }

        if (selectedCard != null)
        {
            // 立即销毁卡牌，确保 UI 减一张
            DestroyImmediate(selectedCard.gameObject);

            // 再判断并补一张牌
            int currentHandCount = GameManager.Instance.cardParent.childCount;
            if (currentHandCount < 5)
            {
                GameManager.Instance.DrawCard();
            }
        }

        TurnManager.Instance.NextTurn();
        PathChecker checker = UnityEngine.Object.FindFirstObjectByType<PathChecker>();
        if (checker != null)
        {
            checker.CheckWinCondition();
        }

        // Debug：检测是否连接成功
        bool connected = IsConnectedToNeighbor();
        Debug.Log($"Connected to neighbor: {connected}");
    }

    public Card GetCard()
    {
        return GetComponentInChildren<CardDisplay>()?.cardData;
    }

    public bool IsConnectedToNeighbor()
    {
        Card currentCard = GetCard();
        if (currentCard == null) return false;

        MapCell[,] map = GameManager.Instance.mapGenerator.mapCells;
        int maxRow = map.GetLength(0);
        int maxCol = map.GetLength(1);

        bool connected = false;

        if (row > 0)
        {
            MapCell neighbor = map[row - 1, col];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && currentCard.up && neighborCard.down)
                connected = true;
        }

        if (row < maxRow - 1)
        {
            MapCell neighbor = map[row + 1, col];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && currentCard.down && neighborCard.up)
                connected = true;
        }

        if (col > 0)
        {
            MapCell neighbor = map[row, col - 1];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && currentCard.left && neighborCard.right)
                connected = true;
        }

        if (col < maxCol - 1)
        {
            MapCell neighbor = map[row, col + 1];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && currentCard.right && neighborCard.left)
                connected = true;
        }

        return connected;
    }
}
