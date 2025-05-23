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

    // ✅ 统计补注册数量
    private static int lateRegisteredCount = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();

        state = GetComponent<MapCellState>();
        ui = GetComponent<MapCellUI>();

        // ✅ 设置 UI 父节点为 MapPanel
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

        // ✅ 等待 MapGenerator 初始化
        while (MapGenerator.LocalInstance == null && timer < timeout)
        {
            Debug.LogWarning("⏳ 等待 MapGenerator.LocalInstance...");
            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        // ✅ 等待 row/col/spriteName 同步
        yield return new WaitUntil(() =>
            row >= 0 && col >= 0 && !(row == 0 && col == 0 && spriteName == null)
        );

        // ✅ 同步本地状态
        state.row = row;
        state.col = col;

        // ✅ 显示起点或终点卡牌图像（如果 spriteName 存在）
        if (ui.cardDisplay == null && !string.IsNullOrEmpty(spriteName))
        {
            Sprite sprite = Resources.Load<Sprite>($"Images/{spriteName}");
            if (sprite != null)
            {
                CardData cardData = new CardData
                {
                    cardName = spriteName.Contains("Terminus") ? "Terminus" : "Origin",
                    spriteName = spriteName,
                    cardType = Card.CardType.Path,
                    up = true,
                    down = true,
                    left = true,
                    right = true,
                    blockedCenter = false,
                    isPathPassable = true
                };

                ui.ShowCard(cardData, sprite);
                state.SetCard(new Card(cardData));

                Debug.Log($"✅ 起点/终点 UI 显示完成：{spriteName} at ({row},{col})");
            }
            else
            {
                Debug.LogWarning($"❌ Resources.Load 加载失败：Images/{spriteName}");
            }
        }

        // ✅ 注册地图格子（确保只有同步完成才执行）
        MapGenerator.LocalInstance?.RegisterCell(this.GetComponent<MapCell>());

        // ✅ 统计并输出补注册信息
        lateRegisteredCount++;
        Debug.LogWarning($"🛠️ 延迟注册 MapCell ({row},{col})，当前累计补注册数量：{lateRegisteredCount}");
    }

    [Server]
    public void SetBlockedByName(string name)
    {
        state = GetComponent<MapCellState>();
        spriteName = name;
        state.isBlocked = true;

        Debug.Log($"[服务端] SetBlockedByName → spriteName={spriteName}, row={row}, col={col}, isBlocked={state.isBlocked}");
    }

    [ClientRpc]
    public void RpcRevealTerminal(string spriteName)
    {
        Sprite sprite = Resources.Load<Sprite>($"Images/{spriteName}");
        if (sprite == null)
        {
            Debug.LogWarning($"❌ RpcRevealTerminal 找不到图片：{spriteName}");
            return;
        }

        GetComponent<MapCell>()?.RevealTerminal(sprite);
    }

}
