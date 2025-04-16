using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class RegisterManager : MonoBehaviour
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

        public UserData(string u, string p)
        {
            username = u;
            password = p;
        }
    }

    [System.Serializable]
    public class ResponseData
    {
        public bool success;
        public string message;
    }

    public void OnRegisterButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Username and password cannot be empty.");
            return;
        }

        StartCoroutine(RegisterUser(username, password));
    }

    IEnumerator RegisterUser(string username, string password)
    {
        string jsonData = JsonUtility.ToJson(new UserData(username, password));
        UnityWebRequest request = new UnityWebRequest("http://localhost:8080/api/user/register", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Register response: " + json);

            ResponseData response = JsonUtility.FromJson<ResponseData>(json);
            if (response.success)
            {
                ShowMessage("Registration successful. Please log in.", true);
            }
            else
            {
                ShowMessage(response.message);
            }
        }
        else
        {
            Debug.LogError("Register error: " + request.error);
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
