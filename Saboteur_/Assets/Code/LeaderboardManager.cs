using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;

    void Start()
    {
        StartCoroutine(FetchLeaderboard());
    }

    IEnumerator FetchLeaderboard()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:8080/api/user/ranking");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            leaderboardText.text = "Failed to load leaderboard.";
        }
        else
        {
            // 手动把数组包装成一个对象（让 JsonUtility 可以解析）
            string json = "{\"list\":" + request.downloadHandler.text + "}";

            RankingList ranking = JsonUtility.FromJson<RankingList>(json);

            string result = "";
            for (int i = 0; i < ranking.list.Count; i++)
            {
                var entry = ranking.list[i];
                result += $"{i + 1}. {entry.username} - {entry.score} pts\n";
            }

            leaderboardText.text = result;
        }
    }
}
