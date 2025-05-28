using UnityEngine;
using Mirror;

public class ToolEffectManager : MonoBehaviour
{
    [Header("引用")]
    public GameManager gameManager;
    public PlayerUIManager playerUIManager;

    [Header("提示面板")]
    public GameObject breakSelfTipPanel;
    public GameObject toolRepeatTipPanel;
    public GameObject textToolAlreadyBroken;
    public GameObject textToolAlreadyRepaired;

    [HideInInspector] public string pendingBreakEffect = null;
    [HideInInspector] public int pendingBreakCardIndex = -1;

    [HideInInspector] public string pendingRepairEffect = null;
    [HideInInspector] public int pendingRepairCardIndex = -1;

    public void ShowBreakToolPanel(string effect, int cardIndex)
    {
        pendingBreakEffect = effect;
        pendingBreakCardIndex = cardIndex;
        Debug.Log($"🎯 等待选择玩家破坏工具：{effect}");
    }

    public void ShowRepairToolPanel(string effect, int cardIndex)
    {
        pendingRepairEffect = effect;
        pendingRepairCardIndex = cardIndex;
        Debug.Log($"🔧 等待选择玩家修复工具：{effect}");
    }

    public void ApplyBreakEffectTo(PlayerController target)
    {
        var localPlayer = PlayerController.LocalInstance;
        if (!localPlayer.isMyTurn)
        {
            Debug.Log("⛔ 不是你的回合，不能使用破坏卡！");
            return;
        }

        if (target == localPlayer)
        {
            Debug.Log("⚠️ 不能破坏自己的工具！");
            breakSelfTipPanel?.SetActive(true);
            CancelInvoke(nameof(HideBreakSelfTip));
            Invoke(nameof(HideBreakSelfTip), 2f);
            return;
        }

        bool alreadyBroken =
            (pendingBreakEffect == "BreakLamp" && !target.hasLamp) ||
            (pendingBreakEffect == "BreakPickaxe" && !target.hasPickaxe) ||
            (pendingBreakEffect == "BreakMinecart" && !target.hasMineCart);

        if (alreadyBroken)
        {
            Debug.Log("⚠️ 工具已被破坏，无法重复破坏！");
            toolRepeatTipPanel?.SetActive(true);
            textToolAlreadyBroken?.SetActive(true);
            textToolAlreadyRepaired?.SetActive(false);
            CancelInvoke(nameof(HideToolRepeatTip));
            Invoke(nameof(HideToolRepeatTip), 2f);
            return;
        }

        localPlayer.CmdApplyToolEffect(target.netId, pendingBreakEffect);

        if (pendingBreakCardIndex >= 0)
        {
            var card = localPlayer.hand[pendingBreakCardIndex];
            localPlayer.CmdRequestPlaceCard(0,
                card.cardName, card.spriteName, card.toolEffect, card.cardType,
                false, false, false, false, false, false,
                pendingBreakCardIndex);
        }

        ClearPendingBreak();
        localPlayer.CmdEndTurn();
    }

    public void ApplyRepairEffectTo(PlayerController target)
    {
        var localPlayer = PlayerController.LocalInstance;
        if (!localPlayer.isMyTurn)
        {
            Debug.Log("⛔ 不是你的回合，不能使用修复卡！");
            return;
        }

        bool alreadyRepaired = false;

        switch (pendingRepairEffect)
        {
            case "RepairLamp": alreadyRepaired = target.hasLamp; break;
            case "RepairPickaxe": alreadyRepaired = target.hasPickaxe; break;
            case "RepairMinecart": alreadyRepaired = target.hasMineCart; break;
            case "RepairPickaxeAndMinecart": alreadyRepaired = target.hasPickaxe && target.hasMineCart; break;
            case "RepairPickaxeAndLamp": alreadyRepaired = target.hasPickaxe && target.hasLamp; break;
            case "RepairMinecartAndLamp": alreadyRepaired = target.hasMineCart && target.hasLamp; break;
        }

        if (alreadyRepaired)
        {
            Debug.Log("⚠️ 工具已完好，无法修复！");
            toolRepeatTipPanel?.SetActive(true);
            textToolAlreadyBroken?.SetActive(false);
            textToolAlreadyRepaired?.SetActive(true);
            CancelInvoke(nameof(HideToolRepeatTip));
            Invoke(nameof(HideToolRepeatTip), 2f);
            return;
        }

        localPlayer.CmdApplyToolEffect(target.netId, pendingRepairEffect);

        if (pendingRepairCardIndex >= 0)
        {
            var card = localPlayer.hand[pendingRepairCardIndex];
            localPlayer.CmdRequestPlaceCard(0,
                card.cardName, card.spriteName, card.toolEffect, card.cardType,
                false, false, false, false, false, false,
                pendingRepairCardIndex);
        }

        ClearPendingRepair();
        localPlayer.CmdEndTurn();
    }

    public void ClearPendingBreak()
    {
        pendingBreakEffect = null;
        pendingBreakCardIndex = -1;
    }

    public void ClearPendingRepair()
    {
        pendingRepairEffect = null;
        pendingRepairCardIndex = -1;
    }

    public void HideBreakSelfTip()
    {
        breakSelfTipPanel?.SetActive(false);
    }

    public void HideToolRepeatTip()
    {
        toolRepeatTipPanel?.SetActive(false);
        textToolAlreadyBroken?.SetActive(false);
        textToolAlreadyRepaired?.SetActive(false);
    }
}
