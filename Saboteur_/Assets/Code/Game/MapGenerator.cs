using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject mapCellPrefab;
    public Transform mapParent;
    public int cols = 10; // 列数（水平数量）
    public int rows = 5;  // 行数（垂直数量）

    public Sprite originSprite;
    public Sprite terminusSprite;

    private MapCell[,] mapCells;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        mapCells = new MapCell[rows, cols];  // 以行列的顺序存储

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                GameObject cellObj = Instantiate(mapCellPrefab, mapParent);
                MapCell cell = cellObj.GetComponent<MapCell>();
                mapCells[row, col] = cell;

                // 重要！！给cell名字，方便查看（非必要但推荐）
                cellObj.name = $"Cell ({row + 1},{col + 1})";

                // 特殊格子判断
                if (row == 2 && col == 1) // 第3行第2列 -> Origin (因为索引从0开始)
                {
                    cell.SetBlocked(originSprite);
                }
                else if ((row == 0 && col == 8) || (row == 2 && col == 8) || (row == 4 && col == 8))
                {
                    // 1,9  3,9  5,9 -> Terminus
                    cell.SetBlocked(terminusSprite);
                }
            }
        }
    }
}
