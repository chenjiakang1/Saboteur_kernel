using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public GameObject messagePanel;
    public TMP_Text messageText;

    [System.Serializable]
    public class UserData
    {
        public string username;
        public string password;

        public UserData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }

    [System.Serializable]
    public class ResponseData
    {
        public bool success;
        public string message;
    }

    public void OnLoginButtonClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Please enter both username and password.");
            return;
        }

        StartCoroutine(LoginUser(username, password));
    }

    IEnumerator LoginUser(string username, string password)
    {
        string jsonData = JsonUtility.ToJson(new UserData(username, password));

        UnityWebRequest request = new UnityWebRequest("http://localhost:8080/api/user/login", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Login response: " + json);

            ResponseData response = JsonUtility.FromJson<ResponseData>(json);
            if (response.success)
            {
                UserSession.Username = username;
                ShowMessage("Login successful. Welcome " + username + "!", true);
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene("LobbyScene");
            }
            else
            {
                ShowMessage(response.message);
            }
        }
        else
        {
            Debug.LogError("Login failed: " + request.error);
            ShowMessage("Network error: " + request.error);
        }
    }

    void ShowMessage(string msg, bool isSuccess = false)
    {
        messagePanel.SetActive(true);
        messageText.text = msg;
        messageText.color = isSuccess ? Color.green : Color.red;
        StartCoroutine(HideMessageAfterDelay());
    }

    IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(2.5f);
        messagePanel.SetActive(false);
    }
}
