using System.Collections.Generic;
using UnityEngine;

public class ScoreCardDeckManager : MonoBehaviour
{
    [Header("积分卡图像")]
    public Sprite gold1Sprite;
    public Sprite gold2Sprite;
    public Sprite gold3Sprite;
    // public Sprite coalSprite; // ❌ 已删除

    public List<ScoreCard> scoreDeck = new();
    public int remainingCards = 0;

    public void InitScoreDeck()
    {
        scoreDeck.Clear();

        // ✅ 仅添加金币卡（不包含石头卡）
        AddScoreCards(16, "Gold1", 1, ScoreCardData.ScoreType.Gold1, gold1Sprite);
        AddScoreCards(8, "Gold2", 2, ScoreCardData.ScoreType.Gold2, gold2Sprite);
        AddScoreCards(4, "Gold3", 3, ScoreCardData.ScoreType.Gold3, gold3Sprite);

        ShuffleDeck();
        remainingCards = scoreDeck.Count;
    }

    private void AddScoreCards(int count, string name, int score, ScoreCardData.ScoreType type, Sprite sprite)
    {
        for (int i = 0; i < count; i++)
        {
            var card = new GameObject(name).AddComponent<ScoreCard>();
            card.cardName = name;
            card.scoreType = type;
            card.scoreValue = score;
            card.description = name;
            card.sprite = sprite;

            scoreDeck.Add(card);
        }
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < scoreDeck.Count; i++)
        {
            int j = Random.Range(i, scoreDeck.Count);
            (scoreDeck[i], scoreDeck[j]) = (scoreDeck[j], scoreDeck[i]);
        }
    }

    public ScoreCard DrawCard()
    {
        if (scoreDeck.Count == 0)
        {
            Debug.LogWarning("🎴 积分卡组为空");
            return null;
        }

        var card = scoreDeck[0];
        scoreDeck.RemoveAt(0);
        remainingCards--;
        return card;
    }
}
