using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ScoreCardDisplay : NetworkBehaviour
{
    public Image image;                         // 显示积分卡图片
    public ScoreCardData data;                  // 存储结构体数据
    public bool isSelected = false;
    public int cardIndex = -1;

    public string cardId;  // ✅ 由 ScoreCardData 同步赋值，不再本地生成

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
        GetComponent<Button>().interactable = false;

        var outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = true;

        // ✅ 通知流程控制器（如有）
        ScoreCardDrawFlow flow = FindFirstObjectByType<ScoreCardDrawFlow>();
        if (flow != null)
            flow.OnCardSelected(this);

        // ✅ 给本地玩家加分
        var player = PlayerController.LocalInstance;
        if (player != null)
        {
            player.CmdDebugAddScore(data.scoreValue);

            // ✅ 通过玩家对象发起服务端销毁请求
            player.CmdRequestDestroyCard(cardId);
        }
        else
        {
            Debug.LogWarning("❌ 未找到本地玩家，无法发送加分与销毁请求");
        }
    }

    public void Deselect()
    {
        isSelected = false;
    }
}
