using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class ChangePasswordManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField oldPasswordInput;
    public TMP_InputField newPasswordInput;

    public GameObject messagePanel;
    public TextMeshProUGUI messageText;

    private const string changePasswordUrl = "http://localhost:8080/api/user/change-password";

    public void OnChangePasswordClicked()
    {
        string username = usernameInput.text;
        string oldPassword = oldPasswordInput.text;
        string newPassword = newPasswordInput.text;

        StartCoroutine(ChangePassword(username, oldPassword, newPassword));
    }

    IEnumerator ChangePassword(string username, string oldPassword, string newPassword)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("oldPassword", oldPassword);
        form.AddField("newPassword", newPassword);

        UnityWebRequest www = UnityWebRequest.Post(changePasswordUrl, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            StartCoroutine(ShowMessage("Network error: " + www.error, Color.red));
        }
        else
        {
            string json = www.downloadHandler.text;
            if (json.Contains("true"))
            {
                StartCoroutine(ShowMessage("Password changed successfully", Color.green));
            }
            else
            {
                StartCoroutine(ShowMessage("Failed to change password", Color.red));
            }
        }
    }

    IEnumerator ShowMessage(string message, Color color)
    {
        messagePanel.SetActive(true);
        messageText.text = message;
        messageText.color = color;
        yield return new WaitForSeconds(3f);
        messagePanel.SetActive(false);
    }
}
