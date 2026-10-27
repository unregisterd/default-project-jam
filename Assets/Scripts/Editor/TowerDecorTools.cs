using UnityEditor;
using UnityEngine;

public static class TowerDecorTools
{
    private const string GrayMaterialPath = "Assets/Art/Materials/M_Block_Gray.mat";
    private const string DecorMaterialPath = "Assets/Art/Materials/M_Decor.mat";

    [MenuItem("Tools/Level/Add Decorations")]
    public static void AddDecorations()
    {
        GameObject root = Selection.activeGameObject;

        if (root == null)
        {
            Debug.LogWarning("[Decor] 先选中塔的根节点。");
            return;
        }

        Material material = GetDecorMaterial();
        Block[] blocks = root.GetComponentsInChildren<Block>(true);
        Vector3 center = CalculateCenter(blocks);

        GameObject decorRoot = new GameObject("Decor");
        Undo.RegisterCreatedObjectUndo(decorRoot, "Add Decorations");

        int count = 0;

        foreach (Block block in blocks)
        {
            if (Random.value > 0.45f)
            {
                continue;
            }

            Vector3 position = block.transform.position;
            Vector3 outward = position - center;
            outward.y = 0f;

            if (outward.sqrMagnitude < 0.01f)
            {
                outward = Random.insideUnitSphere;
                outward.y = 0f;
            }

            outward.Normalize();

            float height = Random.Range(0.8f, 2.2f);
            GameObject debris = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debris.name = "Debris";

            Collider collider = debris.GetComponent<Collider>();

            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            debris.transform.SetParent(decorRoot.transform, true);
            debris.transform.position = position + outward * Random.Range(0.9f, 1.4f) + Vector3.up * (height * 0.5f - 0.4f);
            debris.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-6f, 6f));
            debris.transform.localScale = new Vector3(Random.Range(0.15f, 0.3f), height, Random.Range(0.6f, 1.6f));

            Renderer renderer = debris.GetComponent<Renderer>();

            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            debris.isStatic = true;
            count++;
        }

        Debug.Log($"[Decor] 生成 {count} 块残垣（无碰撞、共享材质，挂在 Decor 下）");
    }

    [MenuItem("Tools/Level/Add Core Pillar")]
    public static void AddCorePillar()
    {
        GameObject root = Selection.activeGameObject;

        if (root == null)
        {
            Debug.LogWarning("[Core] 先选中塔的根节点。");
            return;
        }

        Block[] blocks = root.GetComponentsInChildren<Block>(true);

        if (blocks.Length == 0)
        {
            Debug.LogWarning("[Core] 没找到方块。");
            return;
        }

        float minY = float.MaxValue;
        float maxY = float.MinValue;
        Vector3 center = CalculateCenter(blocks);

        foreach (Block block in blocks)
        {
            float top = block.GetTopHeight();
            minY = Mathf.Min(minY, top - 1f);
            maxY = Mathf.Max(maxY, top);
        }

        float height = Mathf.Max(1f, maxY - minY + 2f);
        const float radius = 1.5f;

        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "CorePillar";
        pillar.transform.position = new Vector3(center.x, minY + height * 0.5f, center.z);
        pillar.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);

        Renderer renderer = pillar.GetComponent<Renderer>();

        if (renderer != null)
        {
            Material material = GetDecorMaterial();

            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        Undo.RegisterCreatedObjectUndo(pillar, "Add Core Pillar");
        Debug.Log($"[Core] 中心柱：半径 {radius}m，高度 {height:F1}m（保留碰撞体，防止掉进中心）");
    }

    private static Vector3 CalculateCenter(Block[] blocks)
    {
        Vector3 sum = Vector3.zero;

        foreach (Block block in blocks)
        {
            sum += block.transform.position;
        }

        Vector3 center = sum / Mathf.Max(1, blocks.Length);
        center.y = 0f;
        return center;
    }

    private static Material GetDecorMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(DecorMaterialPath);

        if (material != null)
        {
            return material;
        }

        Material gray = AssetDatabase.LoadAssetAtPath<Material>(GrayMaterialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
        {
            return gray;
        }

        material = new Material(shader);
        material.name = "M_Decor";
        material.SetColor("_BaseColor", new Color(0.28f, 0.26f, 0.3f));
        material.SetFloat("_Smoothness", 0.15f);

        EnsureFolder("Assets/Art/Materials");
        AssetDatabase.CreateAsset(material, DecorMaterialPath);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent))
        {
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
