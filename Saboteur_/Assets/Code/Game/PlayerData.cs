using System.Collections.Generic;

public class PlayerData
{
    public string Name;
    public int Gold;
    public List<string> HandCards = new List<string>(); // 字符串手牌（如卡牌名）
    public Card[] CardSlots = new Card[5];               // ✅ 实际五张卡牌对象数组

    public bool IsMyTurn;

    // 工具状态，默认均为 true
    public bool HasPickaxe = true;    // 稿子
    public bool HasMineCart = true;   // 矿车
    public bool HasLamp = true;       // 矿灯
}
