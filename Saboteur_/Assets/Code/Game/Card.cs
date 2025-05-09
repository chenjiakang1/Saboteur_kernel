using UnityEngine;

[System.Serializable]
public class Card
{
    public bool up, down, left, right;
    public string cardName;

    public Sprite sprite; // 每张卡唯一绑定的图像

    public bool blockedCenter = false; // 中心是否被阻断
    public bool isPathPassable = true; // ✅ 新增：是否允许被 DFS 通过

    public Card(bool up, bool down, bool left, bool right, string name = "")
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
        this.cardName = name;
    }

    public override string ToString()
    {
        return $"Card [{cardName}] - U:{up} D:{down} L:{left} R:{right}";
    }
}
