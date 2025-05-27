// MapCellUI.cs
using UnityEngine;
using UnityEngine.UI;

public class MapCellUI : MonoBehaviour
{
    private Image image;
    public CardDisplay cardDisplay;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    /// <summary>
    /// 设置格子显示的精灵
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
        image.color = Color.white;
    }

    /// <summary>
    /// 显示卡牌的图像并生成 UI 组件
    /// </summary>
    public void ShowCard(CardData cardData, Sprite sprite)
    {
        GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, transform);
        var display = cardGO.GetComponent<CardDisplay>();
        display.Init(cardData, sprite);

        RectTransform rt = cardGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        cardDisplay = display;

        // ✅ 确保地图上的卡牌不会阻挡点击（探查卡可点击穿透）
        CanvasGroup cg = cardGO.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = cardGO.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    /// <summary>
    /// 销毁当前的卡牌图像
    /// </summary>
    public void ClearCardDisplay()
    {
        if (cardDisplay != null)
        {
            Destroy(cardDisplay.gameObject);
            cardDisplay = null;
        }
    }

    /// <summary>
    /// 揭示格子周围四个方向的邻居格子
    /// </summary>
    public void RevealNeighbors(int r, int c)
    {
        var map = GameManager.Instance.mapGenerator?.mapCells;
        if (map == null)
        {
            Debug.LogWarning($"❌ RevealNeighbors: 地图未初始化 ({r},{c})");
            return;
        }

        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        void TryReveal(int rr, int cc)
        {
            if (rr >= 0 && rr < rows && cc >= 0 && cc < cols)
            {
                if (map[rr, cc] != null)
                {
                    var image = map[rr, cc].GetComponent<Image>();
                    if (image != null)
                        image.enabled = true;
                }
            }
        }

        TryReveal(r - 1, c);
        TryReveal(r + 1, c);
        TryReveal(r, c - 1);
        TryReveal(r, c + 1);
    }

    /// <summary>
    /// 在终点格子显示最终金矿或石头图片
    /// </summary>
    public void RevealTerminal(Sprite faceSprite)
    {
        if (cardDisplay == null) return;

        cardDisplay.Init("Terminal", faceSprite);

        // ✅ 确保终点卡图像不阻挡点击
        CanvasGroup cg = cardDisplay.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = cardDisplay.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        PlayerController.DebugClient($"🪙 RevealTerminal → 显示终点 sprite: {faceSprite.name}");
    }
}
