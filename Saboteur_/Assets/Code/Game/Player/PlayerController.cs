using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂载用 PlayerController 主类，功能已模块化至多个 partial 脚本中。
/// </summary>
public partial class PlayerController : NetworkBehaviour
{
    // 所有功能在：PlayerCore.cs、PlayerLifecycle.cs、PlayerCardActions.cs、PlayerTurnManager.cs 中实现

    // ✅ 添加用于控制是否启用正式游戏逻辑
    public static bool isGameplayEnabled = false;

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 如果是准备房间（RoomScene），禁用游戏逻辑
        isGameplayEnabled = currentScene != "RoomScene";

        Debug.Log($"🎮 PlayerController 初始化：当前场景 = {currentScene}，是否启用游戏逻辑 = {isGameplayEnabled}");
    }
}
