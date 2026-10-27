using UnityEditor;
using UnityEngine;

public static class BlockModelRandomizer
{
    [MenuItem("Tools/Level/Randomize Block Models")]
    public static void RandomizeSelected()
    {
        GameObject root = Selection.activeGameObject;

        if (root == null)
        {
            Debug.LogWarning("[Randomize] 先选中塔的根节点。");
            return;
        }

        int rotated = 0;
        int scaled = 0;
        Block[] blocks = root.GetComponentsInChildren<Block>(true);

        foreach (Block block in blocks)
        {
            Transform model = block.transform.Find("Model");

            if (model == null)
            {
                continue;
            }

            Undo.RecordObject(model, "Randomize Block Model");

            model.localRotation = Quaternion.Euler(
                Random.Range(-2f, 2f),
                Random.Range(-8f, 8f),
                Random.Range(-2f, 2f));

            Bounds bounds = CalculateBounds(model);
            float current = Mathf.Max(bounds.size.x, bounds.size.z);
            float target = Random.Range(0.88f, 1f);

            if (current > 0.01f)
            {
                float factor = Mathf.Clamp(target / current, 0.8f, 1.05f);

                if (Mathf.Abs(factor - 1f) > 0.02f)
                {
                    Vector3 scale = model.localScale;
                    model.localScale = new Vector3(scale.x * factor, scale.y, scale.z * factor);
                    scaled++;
                }
            }

            rotated++;
        }

        EditorUtility.SetDirty(root);
        Debug.Log($"[Randomize] {rotated} 个模型：旋转 ±2°/±8°；其中 {scaled} 个做了 0.88~1.0 的占地缩放（高度不变，碰撞体不变）");
    }

    private static Bounds CalculateBounds(Transform model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length == 0)
        {
            return new Bounds(model.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
}
