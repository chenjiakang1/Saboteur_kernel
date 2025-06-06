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
        {
            Debug.Log("⚠️ 积分卡已被选中，无法重复选择");
            return;
        }

        var player = PlayerController.LocalInstance;
        if (player == null)
        {
            Debug.LogWarning("❌ 未找到本地玩家");
            return;
        }

        // ✅ 只判断是否轮到你（服务端已限制抽卡顺序）
        if (!player.isMyTurn)
        {
            Debug.Log("⛔ 当前不是你的回合，无法选择积分卡");
            return;
        }

        isSelected = true;
        GetComponent<Button>().interactable = false;

        var outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = true;

        // 通知流程控制器（可选）
        ScoreCardDrawFlow flow = FindFirstObjectByType<ScoreCardDrawFlow>();
        if (flow != null)
            flow.OnCardSelected(this);

        // ✅ 请求加分 + 销毁卡牌（服务端命令）
        player.CmdDebugAddScore(data.scoreValue);
        player.CmdRequestDestroyCard(cardId);

        // ✅ 通知服务端结束本回合，轮换下一个人
        player.CmdRequestScoreDrawEnd();
    }

    public void Deselect()
    {
        isSelected = false;
    }
}
