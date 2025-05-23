using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;

public class MapGenerator : NetworkBehaviour
{
    public static MapGenerator LocalInstance { get; private set; }

    public GameObject mapCellPrefab;
    public RectTransform mapParent;
    public int rows = 5;
    public int cols = 10;

    public Sprite originSprite;
    public Sprite terminusBackSprite;
    public Sprite goldSprite;
    public List<Sprite> rockSprites;

    [HideInInspector]
    public MapCell[,] mapCells;

    private Dictionary<Vector2Int, bool> isGoldMap = new();

    [SyncVar]
    private int syncedGoldIndex = -1;

    private bool hasGenerated = false;

    void Awake()
    {
        if (LocalInstance == null)
        {
            LocalInstance = this;
        }
        else
        {
            Debug.LogWarning("⚠️ Duplicate MapGenerator instance");
        }
    }

    void Start()
    {
        if (isServer)
        {
            Debug.Log("🧠 服务端开始生成地图...");
            GenerateAndSyncMap();
        }
        else if (isClient)
        {
            Debug.Log("🧠 客户端等待构建地图引用...");
            Invoke(nameof(TryBuildMapReference), 1f);
        }
    }

    [Server]
    private void GenerateAndSyncMap()
    {
        syncedGoldIndex = Random.Range(0, 3);
        GenerateMap(syncedGoldIndex);
    }

    public void GenerateMap(int goldIndex)
    {
        hasGenerated = true;
        mapCells = new MapCell[rows, cols];
        isGoldMap.Clear();

        Vector2Int[] terminalPositions = new Vector2Int[]
        {
            new Vector2Int(0, 9),
            new Vector2Int(2, 9),
            new Vector2Int(4, 9)
        };

        for (int i = 0; i < terminalPositions.Length; i++)
        {
            isGoldMap[terminalPositions[i]] = (i == goldIndex);
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject cellGO = Instantiate(mapCellPrefab); // ✅ 不设置父对象
                MapCell cell = cellGO.GetComponent<MapCell>();
                cell.row = r;
                cell.col = c;
                mapCells[r, c] = cell;

                cell.GetComponent<Image>().enabled = true;
                NetworkServer.Spawn(cellGO);

                // 起点
                if (r == 2 && c == 1)
                {
                    cell.SetBlocked(originSprite);
                    Card originCard = new Card(true, true, true, true, "Origin")
                    {
                        sprite = originSprite,
                        isPathPassable = true
                    };
                    GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, cell.transform);
                    var display = cardGO.GetComponent<CardDisplay>();
                    display.Init(originCard, originSprite);
                    cell.card = originCard;
                    cell.cardDisplay = display;
                    cell.isOccupied = true;
                }

                // 起点周围可见
                if ((r == 1 && c == 1) || (r == 3 && c == 1) || (r == 2 && c == 0) || (r == 2 && c == 2))
                {
                    cell.GetComponent<Image>().enabled = true;
                }

                // 终点
                Vector2Int pos = new Vector2Int(r, c);
                if (isGoldMap.ContainsKey(pos))
                {
                    cell.GetComponent<Image>().enabled = true;
                    cell.SetBlocked(terminusBackSprite);

                    Card terminalCard = new Card(true, true, true, true, "Terminal")
                    {
                        sprite = terminusBackSprite,
                        isPathPassable = true
                    };
                    GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, cell.transform);
                    var display = cardGO.GetComponent<CardDisplay>();
                    display.Init(terminalCard, terminusBackSprite);
                    cell.card = terminalCard;
                    cell.cardDisplay = display;
                    cell.isOccupied = true;
                }
            }
        }

        Debug.Log("✅ 服务端地图生成完毕");
    }

    private void TryBuildMapReference()
    {
        if (hasGenerated) return;

        var allCells = FindObjectsByType<MapCell>(FindObjectsSortMode.None);
        if (allCells.Length == 0)
        {
            Debug.LogWarning("⏳ 客户端未收到 MapCell 网络对象，延迟重试...");
            Invoke(nameof(TryBuildMapReference), 1f);
            return;
        }

        // 检查 row/col 是否已同步
        bool ready = true;
        foreach (var cell in allCells)
        {
            if (cell.row == 0 && cell.col == 0 && cell != allCells[0])
            {
                ready = false;
                break;
            }
        }

        if (!ready)
        {
            Debug.LogWarning("⏳ MapCell.row/col 尚未同步，延迟重试...");
            Invoke(nameof(TryBuildMapReference), 1f);
            return;
        }

        mapCells = new MapCell[rows, cols];
        foreach (var cell in allCells)
        {
            mapCells[cell.row, cell.col] = cell;
        }

        hasGenerated = true;
        Debug.Log($"✅ 客户端构建 MapCell 引用成功，共 {allCells.Length} 个格子");
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
            if (!GameManager.Instance.gameStateManager.hasGameEnded)
                GameManager.Instance.gameStateManager.GameOver();
        }
        else
        {
            int rockIndex = Random.Range(0, rockSprites.Count);
            Sprite rockSprite = rockSprites[rockIndex];
            cell.RevealTerminal(rockSprite);
        }
    }

    public void RegisterCell(MapCell cell)
    {
        if (mapCells == null)
        {
            mapCells = new MapCell[rows, cols];
        }

        if (cell.row >= 0 && cell.row < rows && cell.col >= 0 && cell.col < cols)
        {
            mapCells[cell.row, cell.col] = cell;
            Debug.Log($"✅ 客户端地图格子引用注册成功：({cell.row},{cell.col})");
        }
        else
        {
            Debug.LogWarning($"❌ 地图格子坐标非法：({cell.row},{cell.col})");
        }
    }
}
