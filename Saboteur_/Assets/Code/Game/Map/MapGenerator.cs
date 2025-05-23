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
            Invoke(nameof(TryBuildMapReference), 3f);
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

        // ✅ 替换 GameObject 创建片段（MapGenerator.cs 中）
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject cellGO = Instantiate(mapCellPrefab);
                MapCell cell = cellGO.GetComponent<MapCell>();
                var state = cellGO.GetComponent<MapCellState>();
                var net = cellGO.GetComponent<MapCellNetwork>();

                // ✅ 设置 row/col → SyncVar 会标记为已修改
                state.row = r;
                state.col = c;
                net.row = r;
                net.col = c;

                // ✅ 延迟一帧，让 Mirror 捕捉 SyncVar 变化
                yield return new WaitForEndOfFrame();

                // ✅ 同步到网络前再赋值 mapCells
                NetworkServer.Spawn(cellGO);
                mapCells[r, c] = cell;

                // ✅ UI 显示控制
                cell.GetComponent<UnityEngine.UI.Image>().enabled = true;

                // ✅ 设置起点
                if (r == 2 && c == 1)
                {
                    net.SetBlockedByName("Origin_0");
                    state.isOccupied = true;
                }

                // ✅ 设置终点
                Vector2Int pos = new Vector2Int(r, c);
                if (isGoldMap.ContainsKey(pos))
                {
                    net.SetBlockedByName("Terminus_0");
                    state.isOccupied = true;
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

        mapCells = new MapCell[rows, cols];
        int syncedCount = 0;
        int skipped = 0;

        foreach (var cell in allCells)
        {
            var state = cell.GetComponent<MapCellState>();

            // ✅ 如果 row/col 尚未同步，跳过这个格子
            if (state.row == 0 && state.col == 0 && cell.GetInstanceID() != allCells[0].GetInstanceID())
            {
                skipped++;
                continue;
            }

            if (state.row >= 0 && state.row < rows && state.col >= 0 && state.col < cols)
            {
                mapCells[state.row, state.col] = cell;
                syncedCount++;
            }
        }

        hasGenerated = true;
        Debug.Log($"✅ 客户端构建 MapCell 引用成功：已同步 {syncedCount} 个格子，跳过 {skipped} 个未同步对象");

        // ✅ 可选：如果 skipped > 0，继续尝试补充注册（不影响正常游戏）
        if (skipped > 0)
        {
            Invoke(nameof(TryBuildMapReference), 1f);
        }
    }


    public void RegisterCell(MapCell cell)
    {
        if (mapCells == null)
        {
            mapCells = new MapCell[rows, cols];
        }

        var state = cell.GetComponent<MapCellState>();
        if (state.row >= 0 && state.row < rows && state.col >= 0 && state.col < cols)
        {
            mapCells[state.row, state.col] = cell;
            Debug.Log($"✅ 客户端地图格子引用注册成功：({state.row},{state.col})");
        }
        else
        {
            Debug.LogWarning($"❌ 地图格子坐标非法：({state.row},{state.col})");
        }
    }

    public void RevealTerminalAt(int row, int col)
    {
        Vector2Int pos = new Vector2Int(row, col);
        if (!isGoldMap.ContainsKey(pos)) return;

        var cell = mapCells[row, col];
        if (cell == null) return;

        var net = cell.GetComponent<MapCellNetwork>();

        if (isGoldMap[pos])
        {
            string name = "Gold";  // ✅ 正确的金矿图片名
            net?.RpcRevealTerminal(name); // ✅ 仅广播客户端显示

            if (!GameManager.Instance.gameStateManager.hasGameEnded)
                GameManager.Instance.gameStateManager.RpcGameOver(true); // ✅ 广播所有客户端
        }
        else
        {
            int rockIndex = Random.Range(0, rockSprites.Count);
            string name = $"Rock_{rockIndex}";
            net?.RpcRevealTerminal(name); // ✅ 广播客户端翻石头
        }
    }


}
