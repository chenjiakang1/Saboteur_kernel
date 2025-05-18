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

    // ✅ 添加按钮组件（可选，挂载方式也可以用事件绑定）
    private Button button;

    void Awake()
    {
        // 自动获取按钮组件（前提是此对象上有 Button）
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

    // ✅ 点击时随机损坏一个工具
    public void OnClickRandomBreakTool()
    {
        if (playerData == null) return;

        int r = Random.Range(0, 3); // 0: pickaxe, 1: minecart, 2: lamp

        if (r == 0 && playerData.HasPickaxe)
            playerData.HasPickaxe = false;
        else if (r == 1 && playerData.HasMineCart)
            playerData.HasMineCart = false;
        else if (r == 2 && playerData.HasLamp)
            playerData.HasLamp = false;
        else
        {
            // 若抽中的已经坏了，就递归再试一次（最多重试3次）
            OnClickRandomBreakTool();
            return;
        }

        UpdateUI();
    }
}
