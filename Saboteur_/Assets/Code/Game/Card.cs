using UnityEngine;

[System.Serializable]
public class Card
{
    public string cardName = "";
    public Sprite sprite;

    public bool up, down, left, right;
    public bool blockedCenter;
    public bool isPathPassable;

    public enum CardType { None, Path, Tool, Action }
    public CardType cardType = CardType.None;

    public string toolEffect = "";

    // ✅ 构造：路径卡专用
    public Card(bool up, bool down, bool left, bool right, string name = "")
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
        this.cardName = name ?? "";
    }

    // ✅ 从 CardData 构造（用于网络还原）
    public Card(CardData data)
    {
        this.cardName = data.cardName ?? "";
        this.toolEffect = data.toolEffect ?? "";
        this.cardType = data.cardType;
        this.up = data.up;
        this.down = data.down;
        this.left = data.left;
        this.right = data.right;
        this.blockedCenter = data.blockedCenter;
        this.isPathPassable = data.isPathPassable;
        this.sprite = null; // ✅ sprite 本地加载 via spriteName
    }

    // ✅ 拷贝构造函数
    public Card(Card other)
    {
        this.cardName = other.cardName ?? "";
        this.sprite = other.sprite;
        this.up = other.up;
        this.down = other.down;
        this.left = other.left;
        this.right = other.right;
        this.blockedCenter = other.blockedCenter;
        this.isPathPassable = other.isPathPassable;
        this.cardType = other.cardType;
        this.toolEffect = other.toolEffect ?? "";
    }
}
