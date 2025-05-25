using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PlayerUI : MonoBehaviour
{
    [Header("文本显示")]
    public TextMeshProUGUI nameText;

    [Header("工具图标组件")]
    public Image pickaxeImage;
    public Image minecartImage;
    public Image lampImage;

    [Header("图像资源")]
    public Sprite pickaxeNormal;
    public Sprite pickaxeDisabled;
    public Sprite minecartNormal;
    public Sprite minecartDisabled;
    public Sprite lampNormal;
    public Sprite lampDisabled;

    private PlayerController player;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClickRandomBreakTool);
        }
    }

    public void SetPlayer(PlayerController player)
    {
        this.player = player;

        Debug.Log($"[PlayerUI] SetPlayer 被调用：playerName={player.playerName}, netId={player.netId}");

        if (nameText == null)
        {
            Debug.LogWarning("[PlayerUI] nameText 未在 Inspector 中绑定，尝试自动查找...");
            nameText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (nameText != null)
        {
            nameText.text = $"ID: {player.netId}";
            Debug.Log($"[PlayerUI] 设置 nameText.text 成功 → {nameText.text}");
        }
        else
        {
            Debug.LogError("[PlayerUI] ❌ 无法找到 nameText 组件，UI 不会显示 ID");
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (player == null) return;

        pickaxeImage.sprite = player.hasPickaxe ? pickaxeNormal : pickaxeDisabled;
        minecartImage.sprite = player.hasMineCart ? minecartNormal : minecartDisabled;
        lampImage.sprite = player.hasLamp ? lampNormal : lampDisabled;
    }

    public void OnClickRandomBreakTool()
    {
        if (player == null) return;

        // 获取本地玩家
        var localPlayer = PlayerController.LocalInstance;
        if (localPlayer == null) return;

        Debug.Log($"[点击UI] 目标玩家 netId = {player.netId}, 本地玩家 netId = {localPlayer.netId}");

        var toolEffect = GameManager.Instance.toolEffectManager;

        if (!string.IsNullOrEmpty(toolEffect.pendingBreakEffect))
        {
            toolEffect.ApplyBreakEffectTo(player); // 点击目标
            return;
        }

        if (!string.IsNullOrEmpty(toolEffect.pendingRepairEffect))
        {
            toolEffect.ApplyRepairEffectTo(player); // 点击目标
            return;
        }
    }
    public PlayerController Player => player;
}
