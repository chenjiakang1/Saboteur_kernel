using UnityEngine;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    public TMP_Text playerNameText;

    void Start()
    {
        playerNameText.text = UserSession.Username;
    }
}
