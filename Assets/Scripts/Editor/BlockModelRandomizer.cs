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

        int count = 0;
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
            count++;
        }

        EditorUtility.SetDirty(root);
        Debug.Log($"[Randomize] 已随机微调 {count} 个方块模型（±2° / ±8° / ±2°），物理碰撞不受影响");
    }
}
