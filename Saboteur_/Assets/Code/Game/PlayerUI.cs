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
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (player == null) return;

        if (nameText != null)
            nameText.text = player.playerName;

        pickaxeImage.sprite = player.hasPickaxe ? pickaxeNormal : pickaxeDisabled;
        minecartImage.sprite = player.hasMineCart ? minecartNormal : minecartDisabled;
        lampImage.sprite = player.hasLamp ? lampNormal : lampDisabled;
    }

    public void OnClickRandomBreakTool()
    {
        if (player == null || !player.isLocalPlayer) return;

        var toolEffect = GameManager.Instance.toolEffectManager;

        if (!string.IsNullOrEmpty(toolEffect.pendingBreakEffect))
        {
            toolEffect.ApplyBreakEffectTo(player);
            return;
        }

        if (!string.IsNullOrEmpty(toolEffect.pendingRepairEffect))
        {
            toolEffect.ApplyRepairEffectTo(player);
            return;
        }
    }

    public PlayerController Player => player;
}
