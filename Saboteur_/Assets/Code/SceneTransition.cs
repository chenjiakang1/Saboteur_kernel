using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string targetSceneName; // 要返回的场景名（可在 Inspector 设置）

    public void OnReturnButtonClick()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ 未设置返回的场景名！");
        }
    }
}
