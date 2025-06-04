using System;
using UnityEngine;

[Serializable]
public struct ScoreCardData
{
    public string cardName;                    // 积分卡名称
    public ScoreType scoreType;                // 枚举类型（如 Gold3、Gold2）
    public int scoreValue;                     // 实际积分
    public string description;                 // 简要描述
    public string spriteName;                  // 精灵名（用于加载）

    public enum ScoreType { Gold1, Gold2, Gold3} // 可扩展积分类型

    public ScoreCardData(ScoreCard card)
    {
        this.cardName = card.cardName ?? "";
        this.scoreType = card.scoreType;
        this.scoreValue = card.scoreValue;
        this.description = card.description ?? "";
        this.spriteName = card.sprite != null ? card.sprite.name : "";
    }
}
