// ✅ 终点卡改为金矿+石头区分版本 MapGenerator.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public GameObject mapCellPrefab;
    public RectTransform mapParent;
    public int rows = 5;
    public int cols = 10;

    public Sprite originSprite;
    public Sprite terminusBackSprite;
    public Sprite goldSprite;                   // ✅ 单独的金矿卡
    public List<Sprite> rockSprites;            // ✅ 石头卡列表

    [HideInInspector]
    public MapCell[,] mapCells;

    private Dictionary<Vector2Int, bool> isGoldMap = new(); // ✅ 终点位置是否是金矿

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        mapCells = new MapCell[rows, cols];

        Vector2Int[] terminalPositions = new Vector2Int[]
        {
            new Vector2Int(0, 9),
            new Vector2Int(2, 9),
            new Vector2Int(4, 9)
        };

        int goldIndex = Random.Range(0, 3);
        Debug.Log($"🎯 金矿生成在终点位置 {terminalPositions[goldIndex]}");

        for (int i = 0; i < terminalPositions.Length; i++)
        {
            Vector2Int pos = terminalPositions[i];
            isGoldMap[pos] = (i == goldIndex); // ✅ 只有一个终点是真正的金矿
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject cellGO = Instantiate(mapCellPrefab, mapParent);
                MapCell cell = cellGO.GetComponent<MapCell>();
                cell.row = r;
                cell.col = c;
                mapCells[r, c] = cell;

                cell.GetComponent<Image>().enabled = false;

                if (r == 2 && c == 1)
                {
                    cell.GetComponent<Image>().enabled = true;
                    cell.SetBlocked(originSprite);

                    Card originCard = new Card(true, true, true, true, "Origin");
                    originCard.sprite = originSprite;
                    originCard.isPathPassable = true;

                    GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, cell.transform);
                    var display = cardGO.GetComponent<CardDisplay>();
                    display.Init(originCard, originSprite);

                    cell.card = originCard;
                    cell.cardDisplay = display;
                    cell.isOccupied = true;
                }

                if ((r == 1 && c == 1) || (r == 3 && c == 1) || (r == 2 && c == 0) || (r == 2 && c == 2))
                {
                    cell.GetComponent<Image>().enabled = true;
                }

                Vector2Int pos = new Vector2Int(r, c);
                if (isGoldMap.ContainsKey(pos))
                {
                    cell.GetComponent<Image>().enabled = true;
                    cell.SetBlocked(terminusBackSprite);

                    Card terminalCard = new Card(true, true, true, true, "Terminal");
                    terminalCard.sprite = terminusBackSprite;
                    terminalCard.isPathPassable = true;

                    GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, cell.transform);
                    var display = cardGO.GetComponent<CardDisplay>();
                    display.Init(terminalCard, terminusBackSprite);

                    cell.card = terminalCard;
                    cell.cardDisplay = display;
                    cell.isOccupied = true;
                }
            }
        }
    }

    public void RevealTerminalAt(int row, int col)
    {
        Vector2Int pos = new Vector2Int(row, col);
        if (!isGoldMap.ContainsKey(pos)) return;

        var cell = mapCells[row, col];
        if (cell == null) return;

        if (isGoldMap[pos])
        {
            cell.RevealTerminal(goldSprite);

            if (!GameManager.Instance.hasGameEnded)
            {
                GameManager.Instance.GameOver(); // ✅ 正确触发游戏胜利
            }
        }
        else
        {
            int rockIndex = Random.Range(0, rockSprites.Count);
            Sprite rockSprite = rockSprites[rockIndex];
            cell.RevealTerminal(rockSprite);
            Debug.Log($"🪨 翻开的是石头终点 ({row},{col})，游戏继续...");
        }
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}