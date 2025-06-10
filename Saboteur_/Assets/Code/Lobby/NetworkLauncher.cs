using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class NetworkLauncher : MonoBehaviour
{
    public InputField ipAddressInput;

    public void StartAsHost()
    {
        Debug.Log("🟢 启动 Host");
        NetworkManager.singleton.StartHost();  // 自动加载 RoomScene
    }

    public void StartAsClient()
    {
        string ip = ipAddressInput != null ? ipAddressInput.text : "localhost";
        Debug.Log($"🟡 尝试以 Client 连接到 Host（IP: {ip}）");

        NetworkManager.singleton.networkAddress = ip;
        NetworkManager.singleton.StartClient();  // 自动加载 RoomScene
    }
}
