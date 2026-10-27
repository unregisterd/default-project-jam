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
        GUILayout.Label("段落顺序（从上到下）", EditorStyles.boldLabel);

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(150f));

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
        blockPrefab = (GameObject)EditorGUILayout.ObjectField("填充方块预制体", blockPrefab, typeof(GameObject), false);

        fillerMin = EditorGUILayout.IntSlider("填充步数最小", fillerMin, 1, 30);
        fillerMax = EditorGUILayout.IntSlider("填充步数最大", fillerMax, 1, 30);
        stepDistance = EditorGUILayout.FloatField("每步前进", stepDistance);
        heightStep = EditorGUILayout.FloatField("每步高度", heightStep);
        angleStep = EditorGUILayout.FloatField("每步角度", angleStep);

        GUILayout.Space(6f);
        repeats = EditorGUILayout.IntSlider("本轮追加段数倍率", repeats, 1, 8);
        fillerGrowthPerRepeat = EditorGUILayout.IntSlider("每轮填充增加", fillerGrowthPerRepeat, 0, 5);
        shuffle = EditorGUILayout.Toggle("打乱顺序", shuffle);
        seed = EditorGUILayout.IntField("随机种子", seed);
        autoValidate = EditorGUILayout.Toggle("生成后校验", autoValidate);

        GUILayout.Space(10f);

        if (GUILayout.Button("从选中方块向上追加（推荐）", GUILayout.Height(32f)))
        {
            AppendFromSelection();
        }

        GUILayout.Space(6f);
        startTop = EditorGUILayout.FloatField("从零生成的起始顶面高度", startTop);

        if (GUILayout.Button("从零生成整塔"))
        {
            BuildFromScratch();
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

    private List<GameObject> PrepareOrder()
    {
        List<GameObject> ordered = new List<GameObject>();

        foreach (GameObject section in sections)
        {
            if (section != null)
            {
                ordered.Add(section);
            }
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

        return ordered;
    }

    private void BuildFromScratch()
    {
        if (blockPrefab == null)
        {
            EditorUtility.DisplayDialog("Assembler", "先指定填充方块预制体。", "好");
            return;
        }

        List<GameObject> ordered = PrepareOrder();

        if (ordered.Count == 0)
        {
            EditorUtility.DisplayDialog("Assembler", "段落列表是空的。", "好");
            return;
        }

        GameObject root = new GameObject("Tower");
        Undo.RegisterCreatedObjectUndo(root, "Assemble Tower");

        Vector3 position = CreateBlock(root.transform, Vector3.zero, startTop);
        float top = startTop;
        float finalTop = RunAssembler(root.transform, position, top, Vector3.forward, ordered);

        Selection.activeGameObject = root;

        if (autoValidate)
        {
            LevelValidation.ValidateHierarchy(root);
        }

        Debug.Log($"[Assembler] 完成，顶面高度约 {finalTop:F1}m");
    }

    private void AppendFromSelection()
    {
        if (blockPrefab == null)
        {
            EditorUtility.DisplayDialog("Assembler", "先指定填充方块预制体。", "好");
            return;
        }

        GameObject anchor = Selection.activeGameObject;

        if (anchor == null)
        {
            EditorUtility.DisplayDialog("Assembler", "先选中塔顶的那块方块（或它的 Exit）。", "好");
            return;
        }

        List<GameObject> ordered = PrepareOrder();

        if (ordered.Count == 0)
        {
            EditorUtility.DisplayDialog("Assembler", "段落列表是空的。", "好");
            return;
        }

        Block anchorBlock = anchor.GetComponentInParent<Block>();
        float top = anchorBlock != null ? anchorBlock.GetTopHeight() : anchor.transform.position.y;
        Vector3 position = anchor.transform.position;
        position.y = top;

        GameObject root = new GameObject("Tower_Extension");
        Undo.RegisterCreatedObjectUndo(root, "Append Tower");

        float finalTop = RunAssembler(root.transform, position, top, anchor.transform.forward, ordered);

        Selection.activeGameObject = root;

        if (autoValidate)
        {
            LevelValidation.ValidateHierarchy(root);
        }

        Debug.Log($"[Assembler] 追加完成：从 {anchor.name}（y={top:F1}）开始，顶面到 {finalTop:F1}m，共 +{finalTop - top:F1}m");
    }

    private float RunAssembler(Transform parent, Vector3 anchorPosition, float anchorTop, Vector3 direction, List<GameObject> ordered)
    {
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
                    CreateBlock(parent, anchorPosition, anchorTop);
                }

                direction = Quaternion.Euler(0f, angleStep, 0f) * direction;
                Vector3 entryTarget = anchorPosition + direction * stepDistance;
                entryTarget.y = anchorTop + heightStep;

                if (PlaceSection(parent, ordered[s], entryTarget, direction, out Vector3 exitPosition, out float exitTop))
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

        return anchorTop;
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
