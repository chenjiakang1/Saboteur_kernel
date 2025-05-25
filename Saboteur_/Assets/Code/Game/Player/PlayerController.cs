using Mirror;
using UnityEngine;

/// <summary>
/// 挂载用 PlayerController 主类，功能已模块化至多个 partial 脚本中。
/// </summary>
public partial class PlayerController : NetworkBehaviour
{
    // 所有功能在：PlayerCore.cs、PlayerLifecycle.cs、PlayerCardActions.cs、PlayerTurnManager.cs 中实现
}