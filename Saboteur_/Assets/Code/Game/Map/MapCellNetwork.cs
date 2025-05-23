using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// 管理地图格子与服务器之间的数据同步
/// </summary>
public class MapCellNetwork : NetworkBehaviour
{
    [SyncVar] public int row;
    [SyncVar] public int col;
    [SyncVar] private string spriteName;

    private MapCellState state;
    private MapCellUI ui;

    public override void OnStartClient()
    {
        base.OnStartClient();

        state = GetComponent<MapCellState>();
        ui = GetComponent<MapCellUI>();

        // 设置 UI 的父对象为 MapPanel
        Transform mapParent = GameObject.Find("MapPanel")?.transform;
        if (mapParent != null)
        {
            transform.SetParent(mapParent, false);
        }
        else
        {
            Debug.LogWarning("❗ [MapCellNetwork] 找不到 UI 中的 MapPanel");
        }

        StartCoroutine(WaitForSyncAndRegister());
    }

    private IEnumerator WaitForSyncAndRegister()
    {
        float timeout = 5f;
        float timer = 0f;

        // ✅ 等待 MapGenerator 准备好
        while (MapGenerator.LocalInstance == null && timer < timeout)
        {
            Debug.LogWarning("⏳ 等待 MapGenerator.LocalInstance...");
            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        // ✅ 等待 row/col 与 spriteName 都同步完成
        yield return new WaitUntil(() => row > 0 || col > 0 || spriteName != null);

        // ✅ 写入本地状态
        state.row = row;
        state.col = col;

        // ✅ 注册格子
        MapGenerator.LocalInstance?.RegisterCell(this.GetComponent<MapCell>());
        Debug.Log($"✅ [客户端] MapCell 注册完成 row:{row} col:{col}");

        // ✅ 创建 UI 图像（起点/终点/阻断）
        if (ui.cardDisplay == null && !string.IsNullOrEmpty(spriteName))
        {
            Sprite sprite = Resources.Load<Sprite>($"Images/{spriteName}");
            if (sprite != null)
            {
                CardData cardData = new CardData
                {
                    cardName = "Blocked",
                    spriteName = spriteName,
                    cardType = Card.CardType.Path,
                    up = true,
                    down = true,
                    left = true,
                    right = true,
                    isPathPassable = true
                };

                ui.ShowCard(cardData, sprite);
                state.SetCard(new Card(cardData));

                Debug.Log($"✅ [客户端] 创建阻断/起点/终点卡 UI：{sprite.name} at ({row},{col})");
            }
            else
            {
                Debug.LogWarning($"❌ [客户端] Resources 加载失败：Images/{spriteName}");
            }
        }
    }


    [Server]
    public void SetBlockedByName(string name)
    {
        state = GetComponent<MapCellState>();
        spriteName = name;
        state.isBlocked = true;

        Debug.Log($"[服务端] SetBlockedByName → spriteName={spriteName}, row={row}, col={col}, isBlocked={state.isBlocked}");
    }
}
