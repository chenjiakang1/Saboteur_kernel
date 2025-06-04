using UnityEngine;
using UnityEngine.UI;

public class ScoreCardDisplay : MonoBehaviour
{
    public Image image;                         // 显示积分卡图片
    public ScoreCardData data;                  // 存储结构体数据
    public bool isSelected = false;
    public int cardIndex = -1;

    // 初始化：传入数据和图片
    public void Init(ScoreCardData cardData, Sprite sprite)
    {
        data = cardData;
        image.sprite = sprite;
    }

    public void OnClick()
{
    Debug.Log($"🟨 点击积分卡：{data.cardName}，分数：{data.scoreValue}");

    if (isSelected)
        return;

    isSelected = true;

    // 禁用按钮
    GetComponent<Button>().interactable = false;

    // 启用 Outline（可选高亮）
    var outline = GetComponent<Outline>();
    if (outline != null)
        outline.enabled = true;

    // ✅ 通知流程管理器处理点击逻辑
    ScoreCardDrawFlow flow = FindFirstObjectByType<ScoreCardDrawFlow>();
    if (flow != null)
        flow.OnCardSelected(this);
}

    public void Deselect()
    {
        isSelected = false;
    }
}
