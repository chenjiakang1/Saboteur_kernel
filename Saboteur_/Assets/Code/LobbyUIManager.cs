using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RankingEntry
{
    public string username;
    public int score;
}

[System.Serializable]
public class RankingList
{
    public List<RankingEntry> list;
}

public class LobbyUIManager : MonoBehaviour
{
    public TMP_Text playerNameText;
    public TMP_Text playerScoreText; // 绑定 GoldAmount

    void Start()
    {
        playerNameText.text = UserSession.Username;
        StartCoroutine(LoadScore());
    }

    IEnumerator LoadScore()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:8080/api/user/ranking");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"list\":" + request.downloadHandler.text + "}";
            RankingList result = JsonUtility.FromJson<RankingList>(json);

            foreach (RankingEntry entry in result.list)
            {
                if (entry.username == UserSession.Username)
                {
                    playerScoreText.text = entry.score.ToString();
                    break;
                }
            }
        }
        else
        {
            playerScoreText.text = "0";
        }
    }
}