using UnityEngine;
using System.Collections.Generic;

public class PathChecker : MonoBehaviour
{
    private MapCell[,] map;
    private int rows, cols;
    private bool[,] visited;

    public void CheckWinCondition()
    {
        map = GameManager.Instance.mapGenerator.mapCells;
        rows = map.GetLength(0);
        cols = map.GetLength(1);

        Vector2Int[] terminals = new Vector2Int[]
        {
            new Vector2Int(0, 9),
            new Vector2Int(2, 9),
            new Vector2Int(4, 9)
        };

        foreach (var terminal in terminals)
        {
            int r = terminal.x;
            int c = terminal.y;

            // 检查四周邻居是否可连通且能走到起点
            if (CheckNeighborVictory(r - 1, c, "down")) return;
            if (CheckNeighborVictory(r + 1, c, "up")) return;
            if (CheckNeighborVictory(r, c - 1, "right")) return;
            if (CheckNeighborVictory(r, c + 1, "left")) return;
        }

        Debug.Log("❌ 还未满足胜利条件");
    }

    private bool CheckNeighborVictory(int r, int c, string directionToTerminal)
    {
        if (r < 0 || r >= map.GetLength(0) || c < 0 || c >= map.GetLength(1))
            return false;

        MapCell cell = map[r, c];
        if (!cell.isOccupied) return false;

        Card card = cell.GetCard();
        if (card == null || card.blockedCenter) return false;

        bool connected = directionToTerminal switch
        {
            "up" => card.up,
            "down" => card.down,
            "left" => card.left,
            "right" => card.right,
            _ => false
        };

        if (!connected) return false;

        // ✅ 加入起点连通性检查
        if (!IsReachableFromStart(r, c))
        {
            Debug.Log($"⛔ 卡牌 {card.cardName} 虽连接终点，但路径中断，不能胜利");
            return false;
        }

        int playerID = GameManager.Instance.playerID;
        Debug.Log($"🎉 Victory! 玩家 {playerID} 放置的卡片触发胜利 → 卡牌：{card.cardName}，位置：({r},{c})");
        return true;
    }

    private bool IsReachableFromStart(int targetR, int targetC)
    {
        visited = new bool[rows, cols];
        return DFS(2, 1, targetR, targetC);
    }

    private bool DFS(int r, int c, int targetR, int targetC)
    {
        if (r < 0 || r >= rows || c < 0 || c >= cols)
            return false;
        if (visited[r, c]) return false;
        visited[r, c] = true;

        MapCell cell = map[r, c];
        if (!cell.isOccupied) return false;

        Card current = cell.GetCard();
        if (current == null || current.blockedCenter || !current.isPathPassable)
            return false;

        if (r == targetR && c == targetC)
            return true;

        // 四方向继续搜索（必须方向通）
        if (current.up && r > 0)
        {
            Card next = map[r - 1, c].GetCard();
            if (next != null && next.down && DFS(r - 1, c, targetR, targetC)) return true;
        }
        if (current.down && r < rows - 1)
        {
            Card next = map[r + 1, c].GetCard();
            if (next != null && next.up && DFS(r + 1, c, targetR, targetC)) return true;
        }
        if (current.left && c > 0)
        {
            Card next = map[r, c - 1].GetCard();
            if (next != null && next.right && DFS(r, c - 1, targetR, targetC)) return true;
        }
        if (current.right && c < cols - 1)
        {
            Card next = map[r, c + 1].GetCard();
            if (next != null && next.left && DFS(r, c + 1, targetR, targetC)) return true;
        }

        return false;
    }
}
