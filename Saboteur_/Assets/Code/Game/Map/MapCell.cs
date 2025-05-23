using UnityEngine;
using Mirror;

/// <summary>
/// MapCell：地图格子对象的协调主组件，仅负责组合各模块组件
/// </summary>
public class MapCell : NetworkBehaviour
{
    private MapCellState state;
    private MapCellUI ui;
    private MapCellNetwork net;
    private MapCellClickHandler clickHandler;

    private void Awake()
    {
        state = GetComponent<MapCellState>();
        ui = GetComponent<MapCellUI>();
        net = GetComponent<MapCellNetwork>();
        clickHandler = GetComponent<MapCellClickHandler>();
    }

    /// <summary>
    /// 设置为阻断块：清除已有卡牌显示，设置 sprite
    /// </summary>
    public void SetBlocked(Sprite sprite)
    {
        state.isBlocked = true;
        state.isOccupied = false;
        state.card = null;

        ui.ClearCardDisplay();
        ui.SetSprite(sprite);

        PlayerController.DebugClient($"🧱 设置阻断块 ({state.row},{state.col})，Sprite: {sprite.name}");
    }

    /// <summary>
    /// 本地放置卡牌，显示 sprite + 数据结构绑定
    /// </summary>
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

        CardData cardData = new CardData
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

        ui.ShowCard(cardData, sprite);
        state.SetCard(new Card(cardData));

        var map = GameManager.Instance?.mapGenerator?.mapCells;
        if (map != null)
        {
            ui.RevealNeighbors(state.row, state.col);
        }
        else
        {
            PlayerController.DebugClient($"⚠️ mapCells 尚未初始化，跳过 RevealNeighbors ({state.row},{state.col})");
        }

        PlayerController.DebugClient($"✅ PlaceCardLocally 成功放置卡牌 ({state.row},{state.col}) → {cardName}");
    }

    /// <summary>
    /// 服务端放置卡牌，只更新数据，不生成图像
    /// </summary>
    public void PlaceCardServer(string cardName, string spriteName, string toolEffect,
                                Card.CardType cardType,
                                bool up, bool down, bool left, bool right,
                                bool blockedCenter, bool isPassable)
    {
        CardData cardData = new CardData
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

        state.SetCard(new Card(cardData));
        state.isOccupied = true;

        var map = GameManager.Instance?.mapGenerator?.mapCells;
        if (map != null)
        {
            ui.RevealNeighbors(state.row, state.col);
        }
        else
        {
            PlayerController.DebugClient($"⚠️ PlaceCardServer → mapCells 尚未初始化，跳过 RevealNeighbors ({state.row},{state.col})");
        }
    }

    /// <summary>
    /// 获取当前格子的卡牌对象（如无则返回 null）
    /// </summary>
    public Card GetCard()
    {
        return state.GetCard();
    }

    /// <summary>
    /// 显示终点卡图像（金矿或石头）
    /// </summary>
    public void RevealTerminal(Sprite faceSprite)
    {
        ui.RevealTerminal(faceSprite);
    }

    /// <summary>
    /// 点击事件 → 转发给 MapCellClickHandler
    /// </summary>
    public void OnClick()
    {
        clickHandler?.OnClick();
    }
}
