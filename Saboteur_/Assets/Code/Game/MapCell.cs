using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapCell : MonoBehaviour
{
    public bool isOccupied = false;
    public bool isBlocked = false;
    public int row, col;

    private Image image;

    private void Awake()
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
        if (isBlocked || isOccupied)
            return;

        Card card = GameManager.Instance.pendingCard;
        Sprite sprite = GameManager.Instance.pendingSprite;

        if (card == null || sprite == null)
        {
            Debug.LogWarning("No card selected");
            return;
        }

        bool canConnect = false;
        var map = GameManager.Instance.mapGenerator.mapCells;

        // 检查上下左右四个方向是否有连通
        if (row > 0)
        {
            MapCell neighbor = map[row - 1, col];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && card.up && neighborCard.down)
                canConnect = true;
        }
        if (row < map.GetLength(0) - 1)
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
        if (col < map.GetLength(1) - 1)
        {
            MapCell neighbor = map[row, col + 1];
            Card neighborCard = neighbor.GetCard();
            if (neighborCard != null && card.right && neighborCard.left)
                canConnect = true;
        }

        if (!canConnect)
        {
            Debug.LogWarning("Card cannot connect to any neighbors.");
            return;
        }

        // ✅ 放置卡牌到当前格子
        GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, transform);
        cardGO.GetComponent<CardDisplay>().Init(card, sprite);

        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        isOccupied = true;

        // ✅ 替换当前玩家手牌中的已出卡牌
        var currentPlayer = GameManager.Instance.playerGenerator.allPlayers[GameManager.Instance.playerID - 1];
        int replacedIndex = GameManager.Instance.pendingCardIndex;

        if (replacedIndex >= 0 && replacedIndex < currentPlayer.CardSlots.Length)
        {
            if (GameManager.Instance.cardDeck.Count > 0)
            {
                // 从牌堆抽一张替换
                currentPlayer.CardSlots[replacedIndex] = GameManager.Instance.cardDeck[0];
                GameManager.Instance.cardDeck.RemoveAt(0);
            }
            else
            {
                Debug.LogWarning("⚠️ 卡组为空，无法补牌，保留空位或原卡不变");
                // 不修改此位卡牌，保留为空或原状态（你也可以设为 null）
                currentPlayer.CardSlots[replacedIndex] = null;
            }
        }
        else
        {
            Debug.LogError("❌ 替换失败：pendingCardIndex 超出范围！");
        }

        // ✅ 清除选中卡
        GameManager.Instance.ClearPendingCard();

        // ✅ 检查胜利条件
        PathChecker checker = Object.FindFirstObjectByType<PathChecker>();
        checker?.CheckWinCondition();

        // ✅ 进入下一回合
        TurnManager.Instance.NextTurn();

        // ✅ 调试输出
        Debug.Log($"🟢 玩家 {GameManager.Instance.playerID} 当前手牌数：{currentPlayer.CardSlots.Length}");
        Debug.Log($"🃏 当前卡组剩余：{GameManager.Instance.cardDeck.Count}");
        for (int i = 0; i < currentPlayer.CardSlots.Length; i++)
        {
            Debug.Log($"➡️ 手牌{i + 1}：{currentPlayer.CardSlots[i]?.cardName ?? "空"}");
        }
    }

    public Card GetCard()
    {
        var display = GetComponentInChildren<CardDisplay>();
        return display != null ? display.cardData : null;
    }

    public bool IsConnectedToNeighbor()
    {
        Card card = GetCard();
        if (card == null)
            return false;

        var map = GameManager.Instance.mapGenerator.mapCells;

        if (row > 0)
        {
            Card neighbor = map[row - 1, col].GetCard();
            if (neighbor != null && card.up && neighbor.down)
                return true;
        }
        if (row < map.GetLength(0) - 1)
        {
            Card neighbor = map[row + 1, col].GetCard();
            if (neighbor != null && card.down && neighbor.up)
                return true;
        }
        if (col > 0)
        {
            Card neighbor = map[row, col - 1].GetCard();
            if (neighbor != null && card.left && neighbor.right)
                return true;
        }
        if (col < map.GetLength(1) - 1)
        {
            Card neighbor = map[row, col + 1].GetCard();
            if (neighbor != null && card.right && neighbor.left)
                return true;
        }

        return false;
    }
}
