using System;
using UnityEngine;

[Serializable]
public struct CardData
{
    public string cardName;
    public string toolEffect;
    public Card.CardType cardType;
    public bool up, down, left, right;
    public bool blockedCenter;
    public bool isPathPassable;

    // ✅ 用 spriteName 代替同步 Sprite
    public string spriteName;

    public CardData(Card card)
    {
        this.cardName = card.cardName ?? "";
        this.toolEffect = card.toolEffect ?? "";
        this.cardType = card.cardType;
        this.up = card.up;
        this.down = card.down;
        this.left = card.left;
        this.right = card.right;
        this.blockedCenter = card.blockedCenter;
        this.isPathPassable = card.isPathPassable;
        this.spriteName = card.sprite != null ? card.sprite.name : "";
    }
}
