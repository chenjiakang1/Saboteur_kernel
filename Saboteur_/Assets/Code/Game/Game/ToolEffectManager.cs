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
        var localPlayer = NetworkClient.connection.identity.GetComponent<PlayerController>();

        if (target == localPlayer)
        {
            Debug.Log("⚠️ 不能破坏自己的工具！");
            if (breakSelfTipPanel != null)
            {
                breakSelfTipPanel.SetActive(true);
                CancelInvoke("HideBreakSelfTip");
                Invoke("HideBreakSelfTip", 2f);
            }
            return;
        }

        bool alreadyBroken =
            (pendingBreakEffect == "BreakLamp" && !target.hasLamp) ||
            (pendingBreakEffect == "BreakPickaxe" && !target.hasPickaxe) ||
            (pendingBreakEffect == "BreakMinecart" && !target.hasMineCart);

        if (alreadyBroken)
        {
            Debug.Log("⚠️ 工具已被破坏，无法重复破坏！");
            if (toolRepeatTipPanel != null)
            {
                toolRepeatTipPanel.SetActive(true);
                if (textToolAlreadyBroken != null) textToolAlreadyBroken.SetActive(true);
                if (textToolAlreadyRepaired != null) textToolAlreadyRepaired.SetActive(false);
                CancelInvoke("HideToolRepeatTip");
                Invoke("HideToolRepeatTip", 2f);
            }
            return;
        }

        switch (pendingBreakEffect)
        {
            case "BreakLamp": target.hasLamp = false; break;
            case "BreakPickaxe": target.hasPickaxe = false; break;
            case "BreakMinecart": target.hasMineCart = false; break;
        }

        gameManager.cardDeckManager.ReplaceUsedCard(pendingBreakCardIndex);
        ClearPendingBreak();

        playerUIManager.UpdateAllUI();
        TurnManager.Instance.NextTurn();
    }

    public void ApplyRepairEffectTo(PlayerController target)
    {
        bool didRepair = false;

        if (pendingRepairEffect == "RepairLamp" && !target.hasLamp)
        {
            target.hasLamp = true; didRepair = true;
        }
        else if (pendingRepairEffect == "RepairPickaxe" && !target.hasPickaxe)
        {
            target.hasPickaxe = true; didRepair = true;
        }
        else if (pendingRepairEffect == "RepairMinecart" && !target.hasMineCart)
        {
            target.hasMineCart = true; didRepair = true;
        }
        else if (pendingRepairEffect == "RepairPickaxeAndMinecart")
        {
            if (!target.hasPickaxe) { target.hasPickaxe = true; didRepair = true; }
            if (!target.hasMineCart) { target.hasMineCart = true; didRepair = true; }
        }
        else if (pendingRepairEffect == "RepairPickaxeAndLamp")
        {
            if (!target.hasPickaxe) { target.hasPickaxe = true; didRepair = true; }
            if (!target.hasLamp) { target.hasLamp = true; didRepair = true; }
        }
        else if (pendingRepairEffect == "RepairMinecartAndLamp")
        {
            if (!target.hasMineCart) { target.hasMineCart = true; didRepair = true; }
            if (!target.hasLamp) { target.hasLamp = true; didRepair = true; }
        }

        if (!didRepair)
        {
            Debug.Log("⚠️ 所有目标工具都已完好，无法修复！");
            if (toolRepeatTipPanel != null)
            {
                toolRepeatTipPanel.SetActive(true);
                if (textToolAlreadyBroken != null) textToolAlreadyBroken.SetActive(false);
                if (textToolAlreadyRepaired != null) textToolAlreadyRepaired.SetActive(true);
                CancelInvoke("HideToolRepeatTip");
                Invoke("HideToolRepeatTip", 2f);
            }
            return;
        }

        gameManager.cardDeckManager.ReplaceUsedCard(pendingRepairCardIndex);
        ClearPendingRepair();

        playerUIManager.UpdateAllUI();
        TurnManager.Instance.NextTurn();
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
        if (breakSelfTipPanel != null)
            breakSelfTipPanel.SetActive(false);
    }

    public void HideToolRepeatTip()
    {
        if (toolRepeatTipPanel != null) toolRepeatTipPanel.SetActive(false);
        if (textToolAlreadyBroken != null) textToolAlreadyBroken.SetActive(false);
        if (textToolAlreadyRepaired != null) textToolAlreadyRepaired.SetActive(false);
    }
}

