using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
            Debug.Log("✅ MapGenerator.LocalInstance 设置成功");
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
            syncedGoldIndex = Random.Range(0, 3);
            StartCoroutine(GenerateMap(syncedGoldIndex));
        }
        else if (isClient)
        {
            Debug.Log("🧠 客户端等待构建地图引用...");
            Invoke(nameof(TryBuildMapReference), 1f);
        }
    }

    public IEnumerator GenerateMap(int goldIndex)
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
                GameObject cellGO = Instantiate(mapCellPrefab);
                MapCell cell = cellGO.GetComponent<MapCell>();
                cell.row = r;
                cell.col = c;

                yield return null; // ✅ 关键：等待 1 帧，确保 SyncVar 正常注册

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

                    GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, cell.transform.position, Quaternion.identity);
                    NetworkServer.Spawn(cardGO);
                    var display = cardGO.GetComponent<CardDisplay>();
                    display.Init(originCard, originSprite);

                    cell.card = originCard;
                    cell.cardDisplay = display;
                    cell.isOccupied = true;
                }

                // 起点周围格子显示
                if ((r == 1 && c == 1) || (r == 3 && c == 1) || (r == 2 && c == 0) || (r == 2 && c == 2))
                {
                    cell.GetComponent<Image>().enabled = true;
                }

                // 终点设置
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

                    GameObject cardGO = Instantiate(GameManager.Instance.cardPrefab, cell.transform.position, Quaternion.identity);
                    NetworkServer.Spawn(cardGO);
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
        Debug.Log($"🧩 MapCell 总数：{allCells.Length}");

        if (allCells.Length == 0)
        {
            Debug.LogWarning("⏳ 客户端未收到 MapCell 网络对象，延迟重试...");
            Invoke(nameof(TryBuildMapReference), 1f);
            return;
        }

        List<string> unsyncedCells = new List<string>();
        int syncedCount = 0;

        for (int i = 0; i < allCells.Length; i++)
        {
            var cell = allCells[i];
            string status = $"【{i}】→ name:{cell.name}, ID:{cell.GetInstanceID()}, row:{cell.row}, col:{cell.col}, isServer:{cell.isServer}, isClient:{cell.isClient}";

            if (cell.row == 0 && cell.col == 0 && i != 0)
            {
                unsyncedCells.Add($"{cell.name} (ID:{cell.GetInstanceID()})");
                Debug.LogWarning($"⚠️ 未同步 MapCell → {status}");
            }
            else
            {
                syncedCount++;
                Debug.Log($"✅ 同步 MapCell → {status}");
            }
        }

        if (unsyncedCells.Count > 0)
        {
            Debug.LogWarning($"⏳ MapCell.row/col 尚未同步的对象有 {unsyncedCells.Count} 个，延迟重试...\n未同步对象列表: {string.Join(", ", unsyncedCells)}");
            Invoke(nameof(TryBuildMapReference), 1f);
            return;
        }

        mapCells = new MapCell[rows, cols];
        foreach (var cell in allCells)
        {
            if (cell.row >= 0 && cell.row < rows && cell.col >= 0 && cell.col < cols)
            {
                mapCells[cell.row, cell.col] = cell;
            }
            else
            {
                Debug.LogWarning($"❌ MapCell 坐标非法 → row:{cell.row}, col:{cell.col}, ID:{cell.GetInstanceID()}");
            }
        }

        hasGenerated = true;
        Debug.Log($"✅ 客户端构建 MapCell 引用成功：已同步 {syncedCount} 个格子，共 {allCells.Length} 个对象");
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
}
