using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private PlayerData playerData;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClickRandomBreakTool);
        }
    }

    public void SetPlayer(PlayerData data)
    {
        playerData = data;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerData == null) return;

        if (nameText != null)
            nameText.text = playerData.Name;

        pickaxeImage.sprite = playerData.HasPickaxe ? pickaxeNormal : pickaxeDisabled;
        minecartImage.sprite = playerData.HasMineCart ? minecartNormal : minecartDisabled;
        lampImage.sprite = playerData.HasLamp ? lampNormal : lampDisabled;
    }

    public void OnClickRandomBreakTool()
    {
        if (playerData == null) return;

        if (!string.IsNullOrEmpty(GameManager.Instance.pendingBreakEffect))
        {
            GameManager.Instance.ApplyBreakEffectTo(playerData);
            return;
        }

        if (!string.IsNullOrEmpty(GameManager.Instance.pendingRepairEffect))
        {
            GameManager.Instance.ApplyRepairEffectTo(playerData);
            return;
        }
    }
} 
