// MapCellState.cs
using UnityEngine;
using Mirror;

/// <summary>
/// 负责管理地图格子的基础状态：位置、是否被占用、是否被阻断、所持有的卡牌
/// </summary>
public class MapCellState : NetworkBehaviour
{
    /// <summary>
    /// 格子是否已被卡牌占用
    /// </summary>
    [SyncVar]
    public bool isOccupied = false;

    /// <summary>
    /// 格子是否为阻断状态（如塌方或终点）
    /// </summary>
    [SyncVar]
    public bool isBlocked = false;

    /// <summary>
    /// 格子所在的行列坐标
    /// </summary>
    public int row, col;

    /// <summary>
    /// 当前格子拥有的卡牌
    /// </summary>
    public Card card;

    /// <summary>
    /// 获取格子的卡牌，若没有卡牌或未占用，则返回 null
    /// </summary>
    public Card GetCard()
    {
        if (!isOccupied || card == null) return null;
        return card;
    }

    /// <summary>
    /// 设置格子的卡牌，同时标记为已占用
    /// </summary>
    public void SetCard(Card newCard)
    {
        card = newCard;
        isOccupied = true;
    }

    /// <summary>
    /// 清除格子的卡牌，同时标记为未占用
    /// </summary>
    public void ClearCard()
    {
        card = null;
        isOccupied = false;
    }
}
