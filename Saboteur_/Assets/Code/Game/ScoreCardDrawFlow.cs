using UnityEngine;

public class ScoreCardDrawFlow : MonoBehaviour
{
    [Header("积分卡生成相关")]
    public ScoreCardDeckManager deckManager;       // 积分卡卡组管理器
    public GameObject scoreCardPrefab;             // 拖入 ScoreCardDisplay 预制体
    public Transform scoreCardParent;              // 拖入 UI Grid 等父物体

    private int numberOfCardsToDraw = 5;

    public void StartDrawPhase()
    {
        Debug.Log("🟢 固定抽取 5 张积分卡");

        deckManager.InitScoreDeck();

        // 清空原有卡片 UI
        foreach (Transform child in scoreCardParent)
        {
            Destroy(child.gameObject);
        }

        // 从卡组中抽取 5 张积分卡
        for (int i = 0; i < numberOfCardsToDraw; i++)
        {
            var card = deckManager.DrawCard();
            var data = card.ToData();

            GameObject go = Instantiate(scoreCardPrefab, scoreCardParent);
            var display = go.GetComponent<ScoreCardDisplay>();
            display.cardIndex = i;
            display.Init(data, card.sprite);
        }
    }

    public void OnCardSelected(ScoreCardDisplay display)
    {
        Debug.Log($"✅ 你点击了积分卡：{display.data.cardName}（分数：{display.data.scoreValue}）");

        // 禁用点击
        display.GetComponent<UnityEngine.UI.Button>().interactable = false;

        // 高亮（可选）
        var outline = display.GetComponent<UnityEngine.UI.Outline>();
        if (outline != null)
            outline.enabled = true;
    }
}
