using UnityEngine;

[System.Serializable]
public class Card
{
    public bool up, down, left, right;
    public string cardName;

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
