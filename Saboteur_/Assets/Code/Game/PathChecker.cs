using UnityEngine;

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
        visited = new bool[rows, cols];

        bool win = DFS(2, 1); // 起点在 map[2,1]

        if (win)
        {
            Debug.Log("🎉 Victory! A path has reached a goal!");
            // TODO: 显示胜利面板或弹窗
        }
    }

    private bool DFS(int r, int c)
    {
        // 越界或已访问判断
        if (r < 0 || r >= rows || c < 0 || c >= cols) return false;
        if (visited[r, c]) return false;
        if (!map[r, c].isOccupied) return false;

        visited[r, c] = true;

        Card current = map[r, c].GetCard();
        if (current == null) return false;

        // ✅ 若卡片不能通行，直接中止 DFS
        if (!current.isPathPassable) return false;

        // ↑ 上
        if (current.up && r > 0)
        {
            Card neighbor = map[r - 1, c].GetCard();
            if (neighbor != null && neighbor.down && DFS(r - 1, c)) return true;
        }

        // ↓ 下
        if (current.down && r < rows - 1)
        {
            Card neighbor = map[r + 1, c].GetCard();
            if (neighbor != null && neighbor.up && DFS(r + 1, c)) return true;
        }

        // ← 左
        if (current.left && c > 0)
        {
            Card neighbor = map[r, c - 1].GetCard();
            if (neighbor != null && neighbor.right && DFS(r, c - 1)) return true;
        }

        // → 右
        if (current.right && c < cols - 1)
        {
            int rightRow = r;
            int rightCol = c + 1;

            // ✅ 若是终点格，优先判断胜利
            if (rightCol == 9 && (rightRow == 0 || rightRow == 2 || rightRow == 4))
            {
                Debug.Log($"✅ Path reached terminal at ({rightRow},{rightCol})");
                return true;
            }

            Card neighbor = map[rightRow, rightCol].GetCard();
            if (neighbor != null && neighbor.left && DFS(rightRow, rightCol)) return true;
        }

        return false;
    }
}
