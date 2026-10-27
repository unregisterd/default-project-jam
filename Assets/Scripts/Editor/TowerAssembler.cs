using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TowerAssemblerWindow : EditorWindow
{
    private const string LevelFolder = "Assets/Prefabs/Level";

    private GameObject blockPrefab;
    private int fillerMin = 6;
    private int fillerMax = 10;
    private float stepDistance = 1.5f;
    private float heightStep = 1f;
    private float angleStep = 15f;
    private float startTop = 1f;
    private bool shuffle = true;
    private int seed = 2026;
    private bool autoValidate = true;

    [MenuItem("Tools/Level/Tower Assembler")]
    public static void Open()
    {
        GetWindow<TowerAssemblerWindow>("Tower Assembler");
    }

    private void OnGUI()
    {
        GUILayout.Label("整塔装配：段预制体 + 填充路", EditorStyles.boldLabel);

        blockPrefab = (GameObject)EditorGUILayout.ObjectField("填充方块预制体", blockPrefab, typeof(GameObject), false);
        fillerMin = EditorGUILayout.IntSlider("填充步数最小值", fillerMin, 1, 30);
        fillerMax = EditorGUILayout.IntSlider("填充步数最大值", fillerMax, 1, 30);
        stepDistance = EditorGUILayout.FloatField("每步前进", stepDistance);
        heightStep = EditorGUILayout.FloatField("每步高度", heightStep);
        angleStep = EditorGUILayout.FloatField("每步角度", angleStep);
        startTop = EditorGUILayout.FloatField("起始顶面高度", startTop);
        shuffle = EditorGUILayout.Toggle("随机顺序", shuffle);
        seed = EditorGUILayout.IntField("随机种子", seed);
        autoValidate = EditorGUILayout.Toggle("生成后校验", autoValidate);

        GUILayout.Space(8f);

        if (GUILayout.Button("生成整塔"))
        {
            Build();
        }

        if (GUILayout.Button("列出可用段"))
        {
            foreach (GameObject section in LoadSections())
            {
                Debug.Log($"[Assembler] {section.name}");
            }
        }
    }

    private void Build()
    {
        if (blockPrefab == null)
        {
            EditorUtility.DisplayDialog("Assembler", "先指定填充方块预制体（Block_Art_Red/Blue）。", "好");
            return;
        }

        List<GameObject> sections = LoadSections();

        if (sections.Count == 0)
        {
            EditorUtility.DisplayDialog("Assembler", $"在 {LevelFolder} 里没找到带 TowerSection 的预制体。", "好");
            return;
        }

        if (shuffle)
        {
            Random.InitState(seed);

            for (int i = sections.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (sections[i], sections[j]) = (sections[j], sections[i]);
            }
        }

        GameObject root = new GameObject("Tower");
        Undo.RegisterCreatedObjectUndo(root, "Assemble Tower");

        Vector3 anchorPosition = Vector3.zero;
        float anchorTop = startTop;
        Vector3 direction = Vector3.forward;

        anchorPosition = CreateBlock(root.transform, anchorPosition, anchorTop);

        Random.InitState(seed + 1);

        for (int s = 0; s < sections.Count; s++)
        {
            int filler = Random.Range(fillerMin, fillerMax + 1);

            for (int i = 0; i < filler; i++)
            {
                direction = Quaternion.Euler(0f, angleStep, 0f) * direction;
                anchorPosition += direction * stepDistance;
                anchorTop += heightStep;
                CreateBlock(root.transform, anchorPosition, anchorTop);
            }

            direction = Quaternion.Euler(0f, angleStep, 0f) * direction;
            Vector3 entryTarget = anchorPosition + direction * stepDistance;
            entryTarget.y = anchorTop + heightStep;

            if (PlaceSection(root.transform, sections[s], entryTarget, direction, out Vector3 exitPosition, out float exitTop))
            {
                anchorPosition = exitPosition;
                anchorTop = exitTop;
                Debug.Log($"[Assembler] {sections[s].name} → Exit 高度 {exitTop:F1}");
            }
            else
            {
                Debug.LogError($"[Assembler] {sections[s].name} 放置失败，后续段可能错位。");
            }
        }

        Selection.activeGameObject = root;

        if (autoValidate)
        {
            LevelValidation.ValidateHierarchy(root);
        }

        Debug.Log($"[Assembler] 完成：{sections.Count} 个段，顶面高度约 {anchorTop:F1}m");
    }

    private Vector3 CreateBlock(Transform parent, Vector3 position, float top)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, parent);
        instance.name = "Filler";
        instance.transform.position = new Vector3(position.x, top - 0.5f, position.z);
        return instance.transform.position;
    }

    private bool PlaceSection(Transform parent, GameObject sectionAsset, Vector3 entryTarget, Vector3 direction, out Vector3 exitPosition, out float exitTop)
    {
        exitPosition = entryTarget;
        exitTop = entryTarget.y;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sectionAsset, parent);
        TowerSection section = instance.GetComponent<TowerSection>();

        if (section == null || section.Entry == null || section.Exit == null)
        {
            Debug.LogWarning($"[Assembler] {sectionAsset.name} 缺少 TowerSection / Entry / Exit");
            return false;
        }

        Vector3 entryForward = section.Entry.forward;
        instance.transform.rotation = Quaternion.FromToRotation(entryForward, direction) * instance.transform.rotation;
        instance.transform.position += entryTarget - section.Entry.position;

        exitPosition = section.Exit.position;
        exitTop = exitPosition.y;

        return true;
    }

    private List<GameObject> LoadSections()
    {
        List<GameObject> result = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { LevelFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.StartsWith(LevelFolder))
            {
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null && prefab.GetComponent<TowerSection>() != null)
            {
                result.Add(prefab);
            }
        }

        result.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return result;
    }
}
