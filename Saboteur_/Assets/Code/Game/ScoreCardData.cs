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
    public string cardId;                      // ✅ 唯一标识符，用于同步销毁

    public enum ScoreType { Gold1, Gold2, Gold3 }

    public ScoreCardData(ScoreCard card)
    {
        this.cardName = card.cardName ?? "";
        this.scoreType = card.scoreType;
        this.scoreValue = card.scoreValue;
        this.description = card.description ?? "";
        this.spriteName = card.scoreType.ToString(); // 或 card.sprite.name，如果保持一致
        this.cardId = Guid.NewGuid().ToString();     // ✅ 由服务端统一生成
    }
}
