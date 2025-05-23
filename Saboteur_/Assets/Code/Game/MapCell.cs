using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

public class MapCell : NetworkBehaviour
{
    public bool isOccupied = false;
    public bool isBlocked = false;
    [SyncVar] public int row;
    [SyncVar] public int col;

    private Image image;

    public Card card;
    public CardDisplay cardDisplay;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // 设置 UI 父对象
        Transform mapParent = GameObject.Find("MapPanel")?.transform;
        if (mapParent != null)
        {
            transform.SetParent(mapParent, false);
        }
        else
        {
            Debug.LogWarning("❗ [MapCell] 找不到 UI 中的 MapPanel，格子不会显示在界面上");
        }

        StartCoroutine(WaitForSyncAndRegister());
    }


    private IEnumerator WaitForSyncAndRegister()
    {
        float timeout = 3f;
        float timer = 0f;

        while (MapGenerator.LocalInstance == null && timer < timeout)
        {
            Debug.LogWarning($"⏳ 等待 MapGenerator.LocalInstance...");
            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        if (MapGenerator.LocalInstance != null)
        {
            Debug.Log($"✅ [OnStartClient] MapCell 注册完成 ({row},{col}) → {name}");
            MapGenerator.LocalInstance.RegisterCell(this);
        }
        else
        {
            Debug.LogWarning($"❌ MapCell 注册失败 ({row},{col}) → LocalInstance 为 null");
        }
    }

    // ✅ 新增：服务端主动修复 row/col 未同步的问题
    [TargetRpc]
    public void TargetFixSync(NetworkConnection target, int fixedRow, int fixedCol)
    {
        row = fixedRow;
        col = fixedCol;
        Debug.Log($"🎯 [TargetFixSync] 客户端补丁设置 MapCell → row:{row}, col:{col}, ID:{GetInstanceID()}");
    }

    public void SetBlocked(Sprite sprite)
    {
        isBlocked = true;
        isOccupied = false;
        card = null;

        if (cardDisplay != null)
        {
            Destroy(cardDisplay.gameObject);
            cardDisplay = null;
        }

        image.sprite = sprite;
        image.color = Color.white;

        PlayerController.DebugClient($"🧱 设置阻断块 ({row},{col})，Sprite: {sprite.name}");
    }

    public void OnClick()
    {
        PlayerController.DebugClient($"🟪 点击地图格子 ({row},{col}) → isBlocked: {isBlocked}, isOccupied: {isOccupied}");

        var pending = GameManager.Instance.pendingCard;

        if (pending.HasValue &&
            pending.Value.cardType == Card.CardType.Action &&
            pending.Value.toolEffect == "Collapse")
        {
            PlayerController.DebugClient($"💥 尝试使用塌方卡在 ({row},{col})");
            GameManager.Instance.collapseManager.ApplyCollapseTo(this);
            return;
        }

        if (GameManager.Instance.gameStateManager.hasGameEnded)
        {
            GameManager.Instance.endGameTip?.SetActive(true);
            return;
        }

        if (isBlocked || isOccupied)
        {
            PlayerController.DebugClient($"⛔ 格子 ({row},{col}) 被阻挡或已占用");
            return;
        }

        if (!pending.HasValue || GameManager.Instance.pendingSprite == null)
        {
            PlayerController.DebugClient("⚠️ 无 pendingCard，点击无效");
            return;
        }

        var cardData = pending.Value;
        var currentPlayer = PlayerController.LocalInstance;
        if (currentPlayer == null)
        {
            PlayerController.DebugClient("❌ LocalInstance 为空，无法出牌");
            return;
        }

        if (cardData.cardType == Card.CardType.Path &&
            (!currentPlayer.hasLamp || !currentPlayer.hasPickaxe || !currentPlayer.hasMineCart))
        {
            var toolUI = GameManager.Instance.toolEffectManager;
            toolUI.toolRepeatTipPanel?.SetActive(true);
            toolUI.textToolAlreadyBroken?.SetActive(true);
            toolUI.textToolAlreadyRepaired?.SetActive(false);
            toolUI.CancelInvoke("HideToolRepeatTip");
            toolUI.Invoke("HideToolRepeatTip", 2f);
            PlayerController.DebugClient("⛏️ 工具破损，不能出路径卡");
            return;
        }

        bool canConnect = false;
        var map = GameManager.Instance.mapGenerator.mapCells;

        if (row > 0)
        {
            var neighbor = map[row - 1, col]?.GetCard();
            if (neighbor != null && cardData.up && neighbor.down) canConnect = true;
        }
        if (row < map.GetLength(0) - 1)
        {
            var neighbor = map[row + 1, col]?.GetCard();
            if (neighbor != null && cardData.down && neighbor.up) canConnect = true;
        }
        if (col > 0)
        {
            var neighbor = map[row, col - 1]?.GetCard();
            if (neighbor != null && cardData.left && neighbor.right) canConnect = true;
        }
        if (col < map.GetLength(1) - 1)
        {
            var neighbor = map[row, col + 1]?.GetCard();
            if (neighbor != null && cardData.right && neighbor.left) canConnect = true;
        }

        if (!canConnect)
        {
            PlayerController.DebugClient($"❌ 放置失败：({row},{col}) 无法连接到邻居路径");
            return;
        }

        int replacedIndex = GameManager.Instance.pendingCardIndex;

        currentPlayer.CmdRequestPlaceCard(
            netId,
            cardData.cardName,
            cardData.spriteName,
            cardData.toolEffect,
            cardData.cardType,
            cardData.up, cardData.down, cardData.left, cardData.right,
            cardData.blockedCenter,
            cardData.isPathPassable,
            replacedIndex);

        GameManager.Instance.ClearPendingCard();

        var checker = Object.FindFirstObjectByType<PathChecker>();
        checker?.CheckWinCondition();

        TurnManager.Instance.NextTurn();
    }

    public void PlaceCardLocally(string cardName, string spriteName, string toolEffect,
                                 Card.CardType cardType,
                                 bool up, bool down, bool left, bool right,
                                 bool blockedCenter, bool isPassable)
    {
        Sprite sprite = GameManager.Instance.cardDeckManager.FindSpriteByName(spriteName);
        if (sprite == null)
        {
            PlayerController.DebugClient($"⚠️ 无法找到图片 {spriteName}，无法显示卡牌");
            return;
        }

        var cardData = new CardData
        {
            cardName = cardName,
            spriteName = spriteName,
            toolEffect = toolEffect,
            cardType = cardType,
            up = up,
            down = down,
            left = left,
            right = right,
            blockedCenter = blockedCenter,
            isPathPassable = isPassable
        };

        GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, transform);
        var display = cardGO.GetComponent<CardDisplay>();
        display.Init(cardData, sprite);

        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        this.cardDisplay = display;
        this.card = new Card(cardData);
        isOccupied = true;

        if (GameManager.Instance?.mapGenerator?.mapCells != null)
        {
            RevealNeighbors(row, col);
        }
        else
        {
            PlayerController.DebugClient($"⚠️ mapCells 尚未初始化，跳过 RevealNeighbors ({row},{col})");
        }

        PlayerController.DebugClient($"✅ PlaceCardLocally 成功放置卡牌 ({row},{col}) → {cardName}");
    }

    public void PlaceCardServer(string cardName, string spriteName, string toolEffect,
                                Card.CardType cardType,
                                bool up, bool down, bool left, bool right,
                                bool blockedCenter, bool isPassable)
    {
        var cardData = new CardData
        {
            cardName = cardName,
            spriteName = spriteName,
            toolEffect = toolEffect,
            cardType = cardType,
            up = up,
            down = down,
            left = left,
            right = right,
            blockedCenter = blockedCenter,
            isPathPassable = isPassable
        };

        this.card = new Card(cardData);
        this.isOccupied = true;

        if (GameManager.Instance?.mapGenerator?.mapCells != null)
        {
            RevealNeighbors(row, col);
        }
        else
        {
            PlayerController.DebugClient($"⚠️ PlaceCardServer → mapCells 尚未初始化，跳过 RevealNeighbors ({row},{col})");
        }
    }

    public Card GetCard()
    {
        if (!isOccupied || card == null || cardDisplay == null)
            return null;
        return card;
    }

    private void RevealNeighbors(int r, int c)
    {
        if (GameManager.Instance == null || GameManager.Instance.mapGenerator == null || GameManager.Instance.mapGenerator.mapCells == null)
        {
            PlayerController.DebugClient($"❌ RevealNeighbors 时 mapCells 未初始化，跳过 ({r},{c})");
            return;
        }

        var map = GameManager.Instance.mapGenerator.mapCells;
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        void TryReveal(int rr, int cc)
        {
            if (rr >= 0 && rr < rows && cc >= 0 && cc < cols)
            {
                if (map[rr, cc] != null)
                {
                    var image = map[rr, cc].GetComponent<Image>();
                    if (image != null)
                        image.enabled = true;
                }
            }
        }

        TryReveal(r - 1, c);
        TryReveal(r + 1, c);
        TryReveal(r, c - 1);
        TryReveal(r, c + 1);
    }

    public void RevealTerminal(Sprite faceSprite)
    {
        if (cardDisplay == null) return;
        cardDisplay.Init("Terminal", faceSprite);
        PlayerController.DebugClient($"🪙 RevealTerminal: ({row},{col}) → 显示终点 sprite: {faceSprite.name}");
    }
}
