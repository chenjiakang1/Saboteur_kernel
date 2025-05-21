using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapCell : MonoBehaviour
{
    public bool isOccupied = false;
    public bool isBlocked = false;
    public int row, col;

    private Image image;

    public Card card;
    public CardDisplay cardDisplay;

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
        // ✅ 塌方卡逻辑优先
        if (GameManager.Instance.pendingCard != null &&
            GameManager.Instance.pendingCard.cardType == Card.CardType.Action &&
            GameManager.Instance.pendingCard.toolEffect == "Collapse")
        {
            GameManager.Instance.ApplyCollapseTo(this);
            return;
        }

        if (GameManager.Instance.hasGameEnded)
        {
            if (GameManager.Instance.endGameTip != null)
                GameManager.Instance.endGameTip.SetActive(true);
            Debug.Log("🛑 游戏结束，无法点击地图格子放牌");
            return;
        }

        if (GameManager.Instance.viewPlayerID != GameManager.Instance.playerID)
        {
            Debug.LogWarning("当前不是你的出牌回合，请勿操作卡牌。");
            return;
        }

        // ✅ 禁止放牌到已有路径卡的格子（除了塌方卡上面已放行）
        if (isBlocked || isOccupied)
        {
            Debug.Log("⛔ 此格已放置卡牌，不能重复操作！");
            return;
        }

        Card card = GameManager.Instance.pendingCard;
        Sprite sprite = GameManager.Instance.pendingSprite;

        if (card == null || sprite == null)
        {
            Debug.LogWarning("No card selected");
            return;
        }

        // ✅ 工具损坏检查
        if (card.cardType == Card.CardType.Path)
        {
            var currentPlayer = GameManager.Instance.playerGenerator.allPlayers[GameManager.Instance.playerID - 1];
            if (!currentPlayer.HasLamp || !currentPlayer.HasPickaxe || !currentPlayer.HasMineCart)
            {
                Debug.LogWarning("⛔ 工具损坏，不能放置路径卡！");
                if (GameManager.Instance.toolBrokenTipPanel != null)
                {
                    GameManager.Instance.toolBrokenTipPanel.SetActive(true);
                    GameManager.Instance.CancelInvoke("HideToolBrokenTip");
                    GameManager.Instance.Invoke("HideToolBrokenTip", 2f);
                }
                return;
            }
        }

        // ✅ 连通性检查
        bool canConnect = false;
        var map = GameManager.Instance.mapGenerator.mapCells;

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
            Debug.LogWarning("❌ 该卡无法连接到任意邻居");
            return;
        }

        // ✅ 放置路径卡
        GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, transform);
        cardGO.GetComponent<CardDisplay>().Init(card, sprite);
        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        this.card = card;
        this.cardDisplay = cardGO.GetComponent<CardDisplay>();
        isOccupied = true;

        // ✅ 替换手牌
        var currentPlayer2 = GameManager.Instance.playerGenerator.allPlayers[GameManager.Instance.playerID - 1];
        int replacedIndex = GameManager.Instance.pendingCardIndex;

        if (replacedIndex >= 0 && replacedIndex < currentPlayer2.CardSlots.Length)
        {
            Card newCard = GameManager.Instance.DrawCard();
            currentPlayer2.CardSlots[replacedIndex] = newCard;
        }
        else
        {
            Debug.LogError("❗替换失败：pendingCardIndex 超出范围");
        }

        GameManager.Instance.ClearPendingCard();

        Debug.Log($"🧩 玩家 {GameManager.Instance.playerID} 放置 [{card.cardName}] 于 ({row},{col})");

        PathChecker checker = Object.FindFirstObjectByType<PathChecker>();
        checker?.CheckWinCondition();

        TurnManager.Instance.NextTurn();

        Debug.Log($"🟢 玩家{GameManager.Instance.playerID} 当前手牌数：{currentPlayer2.CardSlots.Length}");
        Debug.Log($"🃏 当前卡组剩余：{GameManager.Instance.cardDeck.Count}");
        for (int i = 0; i < currentPlayer2.CardSlots.Length; i++)
        {
            Debug.Log($"➡️ 手牌{i + 1}：{currentPlayer2.CardSlots[i]?.cardName ?? "空"}");
        }

        RevealNeighbors(row, col);
    }

    public Card GetCard()
    {
        if (!isOccupied || card == null || cardDisplay == null) return null;
        return card;
    }


    public bool IsConnectedToNeighbor()
    {
        Card card = GetCard();
        if (card == null) return false;

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

    private void RevealNeighbors(int r, int c)
    {
        var map = GameManager.Instance.mapGenerator.mapCells;
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        void TryReveal(int rr, int cc)
        {
            if (rr >= 0 && rr < rows && cc >= 0 && cc < cols)
            {
                var cell = map[rr, cc];
                cell.GetComponent<Image>().enabled = true;
            }
        }

        TryReveal(r - 1, c);
        TryReveal(r + 1, c);
        TryReveal(r, c - 1);
        TryReveal(r, c + 1);
    }

    public void RevealTerminal(Sprite faceSprite)
    {
        if (card == null || cardDisplay == null)
            return;

        card.sprite = faceSprite;
        cardDisplay.Init(card, faceSprite);

        Debug.Log($"🎯 终点 ({row},{col}) 被翻开为：{faceSprite.name}");
    }
}
