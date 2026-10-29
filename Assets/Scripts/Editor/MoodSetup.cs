using UnityEditor;
using UnityEngine;

public static class MoodSetup
{
    [MenuItem("Tools/Environment/Setup Mood")]
    public static void Setup()
    {
        GameObject existing = GameObject.Find("Environment");
        GameObject go = existing != null ? existing : new GameObject("Environment");

        if (go.GetComponent<SkyMoodController>() == null)
        {
            go.AddComponent<SkyMoodController>();
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 15f;

        Selection.activeGameObject = go;

        Debug.Log("[Mood] 已创建 Environment（SkyMoodController）。"
                  + " 黄昏/夜晚天空渐变、背景板、BGM 交叉淡入、涟漪颜色都会跟随。");
    }
}
