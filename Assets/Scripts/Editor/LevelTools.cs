using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TowerToolsWindow : EditorWindow
{
    private GameObject blockPrefab;
    private int steps = 100;
    private float radius = 6f;
    private float angleStep = 15f;
    private float heightStep = 1f;
    private float startHeight = 1f;
    private int seed = 12345;
    private bool continueFromSelection;
    private float stepDistance = 1.5f;

    [MenuItem("Tools/Level/Tower Tools")]
    public static void Open()
    {
        GetWindow<TowerToolsWindow>("Tower Tools");
    }

    private void OnGUI()
    {
        GUILayout.Label("螺旋塔生成", EditorStyles.boldLabel);

        blockPrefab = (GameObject)EditorGUILayout.ObjectField("方块预制体", blockPrefab, typeof(GameObject), false);
        steps = EditorGUILayout.IntSlider("步数", steps, 10, 400);
        radius = EditorGUILayout.FloatField("半径", radius);
        angleStep = EditorGUILayout.FloatField("每步角度", angleStep);
        heightStep = EditorGUILayout.FloatField("每步高度", heightStep);
        startHeight = EditorGUILayout.FloatField("起始高度（脚底）", startHeight);
        seed = EditorGUILayout.IntField("随机种子", seed);

        float chord = 2f * radius * Mathf.Sin(angleStep * 0.5f * Mathf.Deg2Rad);
        EditorGUILayout.HelpBox($"弦长 {chord:F2} m（建议 1.4~2.0），高度差 {heightStep:F2} m（建议 ≤1.0）", MessageType.Info);

        if (GUILayout.Button("生成螺旋段"))
        {
            GenerateSpiral();
        }

        GUILayout.Space(12f);
        GUILayout.Label("从选中方块继续", EditorStyles.boldLabel);
        continueFromSelection = EditorGUILayout.Toggle("启用", continueFromSelection);
        stepDistance = EditorGUILayout.FloatField("每步前进", stepDistance);

        if (GUILayout.Button("从选中方块生成路径段"))
        {
            GenerateFromSelection();
        }

        GUILayout.Space(12f);
        GUILayout.Label("校验", EditorStyles.boldLabel);

        if (GUILayout.Button("校验场景里的方块"))
        {
            LevelValidation.ValidateSceneBlocks();
        }
    }

    private void GenerateFromSelection()
    {
        GameObject anchor = Selection.activeGameObject;

        if (anchor == null || blockPrefab == null)
        {
            EditorUtility.DisplayDialog("Tower Tools", "先选中一个起点方块，并指定方块预制体。", "好");
            return;
        }

        Block anchorBlock = anchor.GetComponentInParent<Block>();
        float baseTop = anchorBlock != null ? anchorBlock.GetTopHeight() : anchor.transform.position.y;
        Vector3 position = anchor.transform.position;
        position.y = baseTop;
        Vector3 direction = anchor.transform.forward;

        GameObject root = new GameObject("PathSegment");
        Undo.RegisterCreatedObjectUndo(root, "Generate Path");

        for (int i = 0; i < steps; i++)
        {
            position += direction * stepDistance;
            position.y = baseTop + heightStep * (i + 1);
            direction = Quaternion.Euler(0f, angleStep, 0f) * direction;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, root.transform);
            instance.name = $"Path_{i:000}";
            instance.transform.position = position - Vector3.up * 0.5f;
            instance.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        Selection.activeGameObject = root;
        Debug.Log($"[Tower] 从 {anchor.name} 生成 {steps} 步，顶面高度 {baseTop + heightStep * steps:F1}m");
    }
    private void GenerateSpiral()
    {
        if (blockPrefab == null)
        {
            EditorUtility.DisplayDialog("Tower Tools", "先指定方块预制体。", "好");
            return;
        }

        Undo.IncrementCurrentGroup();

        GameObject root = new GameObject("SpiralTower");
        Undo.RegisterCreatedObjectUndo(root, "Generate Spiral");

        float angle = 0f;
        float height = startHeight;
        Random.InitState(seed);

        for (int i = 0; i < steps; i++)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(Mathf.Cos(rad) * radius, height - 0.5f, Mathf.Sin(rad) * radius);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, root.transform);
            instance.name = $"Step_{i:000}";
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, -angle, 0f);

            angle += angleStep;
            height += heightStep;
        }

        Selection.activeGameObject = root;
        Debug.Log($"[Tower] 生成 {steps} 步，顶面高度 {height - heightStep:F1}m，弦长 {2f * radius * Mathf.Sin(angleStep * 0.5f * Mathf.Deg2Rad):F2}m");
    }
}

public static class LevelValidation
{
    private const float MaxUpNormal = 1.2f;
    private const float MaxUpAbility = 3.4f;
    private const float MaxHorizontalNormal = 2.6f;
    private const float MaxHorizontalAbility = 4.5f;

    [MenuItem("Tools/Level/Validate Blocks")]
    public static void ValidateSceneBlocks()
    {
        Validate(CollectBlocks(null));
    }

    [MenuItem("Tools/Level/Validate Selected Hierarchy")]
    public static void ValidateSelectedHierarchy()
    {
        GameObject root = Selection.activeGameObject;

        if (root == null)
        {
            Debug.LogWarning("[Validate] 先选中要校验的根节点。");
            return;
        }

        Validate(CollectBlocks(root.transform));
    }

    private static List<Block> CollectBlocks(Transform root)
    {
        List<Block> result = new List<Block>();

        if (root != null)
        {
            result.AddRange(root.GetComponentsInChildren<Block>(true));
            return result;
        }

        result.AddRange(Object.FindObjectsOfType<Block>());
        return result;
    }

    public static void ValidateHierarchy(GameObject root)
    {
        if (root == null)
        {
            Debug.LogWarning("[Validate] 根节点为空。");
            return;
        }

        Validate(CollectBlocks(root.transform));
    }

    private static void Validate(List<Block> blocks)
    {
        if (blocks.Count == 0)
        {
            Debug.LogWarning("[Validate] 没有 Block。");
            return;
        }

        float lowestTop = float.MaxValue;

        for (int i = 0; i < blocks.Count; i++)
        {
            float top = blocks[i].GetTopHeight();

            if (top < lowestTop)
            {
                lowestTop = top;
            }
        }

        List<Block> sorted = new List<Block>(blocks);
        sorted.Sort((a, b) => a.GetTopHeight().CompareTo(b.GetTopHeight()));

        List<string> noNormalPath = new List<string>();
        List<string> noAnyPath = new List<string>();

        for (int i = 0; i < sorted.Count; i++)
        {
            Block block = sorted[i];
            float top = block.GetTopHeight();

            if (top <= lowestTop + 0.01f)
            {
                continue;
            }

            bool hasNormal = false;
            bool hasAny = false;

            for (int j = 0; j < sorted.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                Block other = sorted[j];
                float otherTop = other.GetTopHeight();
                float up = top - otherTop;

                if (up < -0.01f)
                {
                    continue;
                }

                float horizontal = HorizontalDistance(block, other);

                if (up <= MaxUpNormal && horizontal <= MaxHorizontalNormal)
                {
                    hasNormal = true;
                }

                if (up <= MaxUpAbility && horizontal <= MaxHorizontalAbility)
                {
                    hasAny = true;
                }
            }

            if (!hasNormal)
            {
                noNormalPath.Add($"{block.name} (top {top:F1})");
            }

            if (!hasAny)
            {
                noAnyPath.Add($"{block.name} (top {top:F1})");
            }
        }

        Debug.Log($"[Validate] 方块 {blocks.Count} 个；纯跳跃不可达 {noNormalPath.Count} 个；任何方式都不可达 {noAnyPath.Count} 个");

        if (noNormalPath.Count > 0)
        {
            Debug.Log("[Validate] 纯跳跃不可达（需要技能，或摆放错误）：\n" + string.Join("\n", noNormalPath.GetRange(0, Mathf.Min(15, noNormalPath.Count))));
        }

        if (noAnyPath.Count > 0)
        {
            Debug.LogError("[Validate] 任何方式都不可达（一定是摆错了）：\n" + string.Join("\n", noAnyPath.GetRange(0, Mathf.Min(15, noAnyPath.Count))));
        }
    }

    [MenuItem("Tools/Level/Place Section At Selected")]
    public static void PlaceSectionAtSelected()
    {
        GameObject prefab = null;
        Transform anchor = null;

        foreach (Object selected in Selection.objects)
        {
            if (selected is GameObject go)
            {
                if (PrefabUtility.IsPartOfPrefabAsset(go) && go.GetComponent<TowerSection>() != null)
                {
                    prefab = go;
                }
                else if (anchor == null)
                {
                    anchor = go.transform;
                }
            }
        }

        if (prefab == null || anchor == null)
        {
            Debug.LogWarning("[Section] 选中一个带 TowerSection 的段预制体 + 一个场景里的锚点物体。");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        TowerSection section = instance.GetComponent<TowerSection>();

        if (section == null || section.Entry == null)
        {
            Debug.LogWarning("[Section] 预制体上没有 TowerSection 或 Entry。");
            return;
        }

        Undo.RegisterCreatedObjectUndo(instance, "Place Section");
        instance.transform.position += anchor.position - section.Entry.position;

        Debug.Log($"[Section] 已放置 {prefab.name}，Entry 对齐 {anchor.name}，Exit 世界坐标 {section.Exit.position}");
    }

    private static float HorizontalDistance(Block a, Block b)
    {
        Vector3 delta = a.transform.position - b.transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }
}
