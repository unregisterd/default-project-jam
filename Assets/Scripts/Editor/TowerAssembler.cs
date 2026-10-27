using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TowerAssemblerWindow : EditorWindow
{
    private const string LevelFolder = "Assets/Prefabs/Level";

    [SerializeField] private List<GameObject> sections = new List<GameObject>();
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private int fillerMin = 6;
    [SerializeField] private int fillerMax = 10;
    [SerializeField] private float stepDistance = 1.5f;
    [SerializeField] private float heightStep = 1f;
    [SerializeField] private float angleStep = 15f;
    [SerializeField] private float startTop = 1f;
    [SerializeField] private int repeats = 1;
    [SerializeField] private int fillerGrowthPerRepeat = 1;
    [SerializeField] private bool shuffle;
    [SerializeField] private int seed = 2026;
    [SerializeField] private bool autoValidate = true;

    private Vector2 _scroll;

    [MenuItem("Tools/Level/Tower Assembler")]
    public static void Open()
    {
        GetWindow<TowerAssemblerWindow>("Tower Assembler");
    }

    private void OnEnable()
    {
        if (sections.Count == 0)
        {
            ScanSections();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("整塔装配", EditorStyles.boldLabel);

        blockPrefab = (GameObject)EditorGUILayout.ObjectField("填充方块预制体", blockPrefab, typeof(GameObject), false);

        GUILayout.Space(6f);
        GUILayout.Label("段落顺序（从上到下）", EditorStyles.boldLabel);

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(160f));

        for (int i = 0; i < sections.Count; i++)
        {
            GUILayout.BeginHorizontal();

            sections[i] = (GameObject)EditorGUILayout.ObjectField($"{i + 1}", sections[i], typeof(GameObject), false);

            GUI.enabled = i > 0;

            if (GUILayout.Button("↑", GUILayout.Width(24f)))
            {
                (sections[i - 1], sections[i]) = (sections[i], sections[i - 1]);
            }

            GUI.enabled = i < sections.Count - 1;

            if (GUILayout.Button("↓", GUILayout.Width(24f)))
            {
                (sections[i + 1], sections[i]) = (sections[i], sections[i + 1]);
            }

            GUI.enabled = true;

            if (GUILayout.Button("✕", GUILayout.Width(24f)))
            {
                sections.RemoveAt(i);
                break;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("扫描 Level 文件夹"))
        {
            ScanSections();
        }

        if (GUILayout.Button("添加空位"))
        {
            sections.Add(null);
        }

        if (GUILayout.Button("清空"))
        {
            sections.Clear();
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        GUILayout.Label("填充 / 路径", EditorStyles.boldLabel);

        fillerMin = EditorGUILayout.IntSlider("填充步数最小", fillerMin, 1, 30);
        fillerMax = EditorGUILayout.IntSlider("填充步数最大", fillerMax, 1, 30);
        stepDistance = EditorGUILayout.FloatField("每步前进", stepDistance);
        heightStep = EditorGUILayout.FloatField("每步高度", heightStep);
        angleStep = EditorGUILayout.FloatField("每步角度", angleStep);
        startTop = EditorGUILayout.FloatField("起始顶面高度", startTop);
        repeats = EditorGUILayout.IntSlider("重复轮数", repeats, 1, 8);
        fillerGrowthPerRepeat = EditorGUILayout.IntSlider("每轮填充增加", fillerGrowthPerRepeat, 0, 5);

        GUILayout.Space(6f);
        shuffle = EditorGUILayout.Toggle("打乱顺序", shuffle);
        seed = EditorGUILayout.IntField("随机种子", seed);
        autoValidate = EditorGUILayout.Toggle("生成后校验", autoValidate);

        GUILayout.Space(8f);

        if (GUILayout.Button("生成整塔", GUILayout.Height(30f)))
        {
            Build();
        }
    }

    private void ScanSections()
    {
        sections.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { LevelFolder });
        List<GameObject> found = new List<GameObject>();

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
                found.Add(prefab);
            }
        }

        found.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        sections.AddRange(found);
    }

    private void Build()
    {
        if (blockPrefab == null)
        {
            EditorUtility.DisplayDialog("Assembler", "先指定填充方块预制体。", "好");
            return;
        }

        List<GameObject> ordered = new List<GameObject>();

        foreach (GameObject section in sections)
        {
            if (section != null)
            {
                ordered.Add(section);
            }
        }

        if (ordered.Count == 0)
        {
            EditorUtility.DisplayDialog("Assembler", "段落列表是空的。", "好");
            return;
        }

        Random.InitState(seed);

        if (shuffle)
        {
            for (int i = ordered.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (ordered[i], ordered[j]) = (ordered[j], ordered[i]);
            }
        }

        GameObject root = new GameObject("Tower");
        Undo.RegisterCreatedObjectUndo(root, "Assemble Tower");

        Vector3 anchorPosition = Vector3.zero;
        float anchorTop = startTop;
        Vector3 direction = Vector3.forward;

        anchorPosition = CreateBlock(root.transform, anchorPosition, anchorTop);

        int total = ordered.Count * repeats;
        int index = 0;

        for (int round = 0; round < repeats; round++)
        {
            for (int s = 0; s < ordered.Count; s++)
            {
                index++;
                int growth = round * fillerGrowthPerRepeat;
                int filler = Random.Range(fillerMin + growth, fillerMax + growth + 1);

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

                if (PlaceSection(root.transform, ordered[s], entryTarget, direction, out Vector3 exitPosition, out float exitTop))
                {
                    anchorPosition = exitPosition;
                    anchorTop = exitTop;
                    Debug.Log($"[Assembler] {index}/{total} {ordered[s].name} → Exit 高度 {exitTop:F1}");
                }
                else
                {
                    Debug.LogError($"[Assembler] {ordered[s].name} 放置失败。");
                }
            }
        }

        Selection.activeGameObject = root;

        if (autoValidate)
        {
            LevelValidation.ValidateHierarchy(root);
        }

        Debug.Log($"[Assembler] 完成：{total} 个段，顶面高度约 {anchorTop:F1}m");
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
}
