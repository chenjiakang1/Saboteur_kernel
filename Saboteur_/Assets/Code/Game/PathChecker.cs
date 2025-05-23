using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class PathChecker : NetworkBehaviour
{
    private MapCell[,] map;
    private int rows, cols;
    private bool[,] visited;

    // ✅ 只由服务器进行胜利判断
    public void CheckWinCondition()
    {
        if (!isServer) return;

        map = GameManager.Instance.mapGenerator.mapCells;
        rows = map.GetLength(0);
        cols = map.GetLength(1);

        Vector2Int[] terminals = new Vector2Int[]
        {
            new Vector2Int(0, 9),
            new Vector2Int(2, 9),
            new Vector2Int(4, 9)
        };

        bool anyVictory = false;

        foreach (var terminal in terminals)
        {
            int r = terminal.x;
            int c = terminal.y;

            if (CheckNeighborVictory(r - 1, c, r, c, "down")) anyVictory = true;
            if (CheckNeighborVictory(r + 1, c, r, c, "up")) anyVictory = true;
            if (CheckNeighborVictory(r, c - 1, r, c, "right")) anyVictory = true;
            if (CheckNeighborVictory(r, c + 1, r, c, "left")) anyVictory = true;
        }

        if (!anyVictory)
        {
            Debug.Log("❌ 还未满足任何终点胜利条件");
        }
    }

    private bool CheckNeighborVictory(int r, int c, int targetRow, int targetCol, string directionToTerminal)
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

        if (!IsReachableFromStart(r, c))
        {
            Debug.Log($"⛔ 卡牌 {card.cardName} 虽连接终点但不连通起点 ({r},{c})");
            return false;
        }

        Debug.Log($"🎉 终点 ({targetRow},{targetCol}) 已被成功连通，通过位置：({r},{c})");

        GameManager.Instance.mapGenerator.RevealTerminalAt(targetRow, targetCol);
        return true;
    }

    private bool IsReachableFromStart(int targetR, int targetC)
    {
        visited = new bool[rows, cols];
        return DFS(2, 1, targetR, targetC); // 起点固定为 (2,1)
    }

    private bool DFS(int r, int c, int targetR, int targetC)
    {
        if (r < 0 || r >= rows || c < 0 || c >= cols) return false;
        if (visited[r, c]) return false;
        visited[r, c] = true;

        MapCell cell = map[r, c];
        if (!cell.isOccupied) return false;

        Card current = cell.GetCard();
        if (current == null || current.blockedCenter || !current.isPathPassable)
            return false;

        if (r == targetR && c == targetC) return true;

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
