using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public GameObject mapCellPrefab;
    public RectTransform mapParent;
    public int rows = 5;
    public int cols = 10;

    public Sprite originSprite;
    public Sprite terminusSprite;

    [HideInInspector]
    public MapCell[,] mapCells;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        mapCells = new MapCell[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject cellGO = Instantiate(mapCellPrefab, mapParent);
                MapCell cell = cellGO.GetComponent<MapCell>();
                cell.row = r;
                cell.col = c;
                mapCells[r, c] = cell;

                // 默认隐藏格子的 Image
                cell.GetComponent<Image>().enabled = false;

                // ✅ 设置起点 (2,1)
                if (r == 2 && c == 1)
                {
                    cell.GetComponent<Image>().enabled = true;

                    cell.SetBlocked(originSprite);

                    Card originCard = new Card(true, true, true, true, "Origin");
                    originCard.sprite = originSprite;
                    originCard.isPathPassable = true;

                    GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, cell.transform);
                    cardGO.GetComponent<CardDisplay>().Init(originCard, originSprite);

                    RectTransform rt = cardGO.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    cell.isOccupied = true;
                }

                // ✅ 起点的上下左右格子也可见
                if ((r == 1 && c == 1) || (r == 3 && c == 1) || (r == 2 && c == 0) || (r == 2 && c == 2))
                {
                    cell.GetComponent<Image>().enabled = true;
                }

                // ✅ 设置终点 (0,9), (2,9), (4,9)
                if (c == 9 && (r == 0 || r == 2 || r == 4))
                {
                    cell.GetComponent<Image>().enabled = true;

                    cell.SetBlocked(terminusSprite);

                    Card terminalCard = new Card(true, true, true, true, "Terminal");
                    terminalCard.sprite = terminusSprite;
                    terminalCard.isPathPassable = true;

                    GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, cell.transform);
                    cardGO.GetComponent<CardDisplay>().Init(terminalCard, terminusSprite);

                    RectTransform rt = cardGO.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;

                    cell.isOccupied = true;
                }
            }
        }
    }
}
