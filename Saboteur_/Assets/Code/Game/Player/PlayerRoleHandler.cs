using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PlayerRole { Undefined, Miner, Saboteur }

public partial class PlayerController
{
    [SyncVar(hook = nameof(OnRoleChanged))]
    public PlayerRole assignedRole = PlayerRole.Undefined;

    private TextMeshProUGUI roleTextUI;
    private GameObject minerImage;
    private GameObject saboteurImage;

    void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        if (!isLocalPlayer) return;

        Debug.Log($"🧾 You are assigned the role: {newRole}");

        if (PlayerController.isGameplayEnabled)
        {
            // 找到 UI 元素
            if (roleTextUI == null)
            {
                GameObject textGO = GameObject.Find("RoleText");
                if (textGO != null)
                    roleTextUI = textGO.GetComponent<TextMeshProUGUI>();
            }

            if (minerImage == null)
                minerImage = GameObject.Find("MinerImage");
            if (saboteurImage == null)
                saboteurImage = GameObject.Find("SaboteurImage");

            // 设置文本
            if (roleTextUI != null)
            {
                roleTextUI.text = $"{newRole}";
                roleTextUI.color = (newRole == PlayerRole.Saboteur) ? Color.red : Color.green;
            }

            // 显示对应图片
            if (minerImage != null) minerImage.SetActive(newRole == PlayerRole.Miner);
            if (saboteurImage != null) saboteurImage.SetActive(newRole == PlayerRole.Saboteur);
        }
    }
    public PlayerRole GetRole()
    {
        return assignedRole;
    }

}
