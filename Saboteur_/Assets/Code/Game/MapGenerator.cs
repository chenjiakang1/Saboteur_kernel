using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject mapCellPrefab;  // 格子预制体
    public Transform mapParent;       // 地图容器

    public int cols = 5;              // 横向格子数量
    public int rows = 9;              // 纵向格子数量

    public Sprite specialSprite;     // ✅ 特殊格子图片（Origin）

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                GameObject cellObj = Instantiate(mapCellPrefab, mapParent);
                MapCell cell = cellObj.GetComponent<MapCell>();

                // ✅ 判断第三行第二列（第3行y=2，第2列x=1）
                if (x == 1 && y == 2)
                {
                    // 设置为Origin图片
                    cellObj.GetComponent<UnityEngine.UI.Image>().sprite = specialSprite;

                    // 禁止放置
                    cell.isBlocked = true;
                }
            }
        }
    }
}


