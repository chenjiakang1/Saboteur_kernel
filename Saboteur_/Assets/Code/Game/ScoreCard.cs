using UnityEngine;

public class ScoreCard : MonoBehaviour
{
    public string cardName;
    public ScoreCardData.ScoreType scoreType;
    public int scoreValue;
    public string description;
    public Sprite sprite;

    // 将 ScoreCard 转为结构体数据
    public ScoreCardData ToData()
    {
        return new ScoreCardData(this);
    }

    public override string ToString()
    {
        return $"{cardName} ({scoreType}, +{scoreValue} pts)";
    }
}
