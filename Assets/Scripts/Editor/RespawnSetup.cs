using UnityEditor;
using UnityEngine;

public static class RespawnSetup
{
    [MenuItem("Tools/Setup/Add Death System To Player")]
    public static void AddToPlayer()
    {
        GameObject player = GameObject.Find("Player");

        if (player == null)
        {
            Debug.LogError("[Respawn] 场景里找不到 Player");
            return;
        }

        if (player.GetComponent<PlayerRespawnManager>() == null)
        {
            player.AddComponent<PlayerRespawnManager>();
        }

        Debug.Log("[Respawn] 已给 Player 添加 PlayerRespawnManager。"
                  + " 记得确认 HintCanvas 上有 FadePanel，且 PlayerBlockAgent 的 Auto Fall Respawn 未勾选。");
    }
}
