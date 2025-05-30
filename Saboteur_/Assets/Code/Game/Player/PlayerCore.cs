using Mirror;
using UnityEngine;
using System.Collections.Generic;

public partial class PlayerController : NetworkBehaviour
{
    public static PlayerController LocalInstance;

    [SyncVar] public string playerName;
    [SyncVar] public int gold;

    [SyncVar(hook = nameof(OnPickaxeChanged))] public bool hasPickaxe = true;
    [SyncVar(hook = nameof(OnMinecartChanged))] public bool hasMineCart = true;
    [SyncVar(hook = nameof(OnLampChanged))] public bool hasLamp = true;

    [SyncVar] public int turnIndex = -1;
    [SyncVar] public bool isMyTurn = false;

    public readonly SyncList<CardData> hand = new SyncList<CardData>();

    private void OnPickaxeChanged(bool oldValue, bool newValue)
    {
        if (!PlayerController.isGameplayEnabled) return;

        GameManager.Instance?.playerUIManager?.UpdateAllUI();
    }

    private void OnMinecartChanged(bool oldValue, bool newValue)
    {
        if (!PlayerController.isGameplayEnabled) return;

        GameManager.Instance?.playerUIManager?.UpdateAllUI();
    }

    private void OnLampChanged(bool oldValue, bool newValue)
    {
        if (!PlayerController.isGameplayEnabled) return;

        GameManager.Instance?.playerUIManager?.UpdateAllUI();
    }

    public static void DebugClient(string msg)
    {
        if (LocalInstance != null)
            LocalInstance.CmdSendDebug(msg);
        else
            Debug.LogWarning("❗ LocalInstance 为 null，无法发送调试信息：" + msg);
    }
}
